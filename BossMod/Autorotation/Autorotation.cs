﻿using ImGuiNET;

namespace BossMod;

sealed class Autorotation : IDisposable
{
    public readonly AutorotationConfig Config = Service.Config.Get<AutorotationConfig>();
    public readonly BossModuleManager Bossmods;
    public WorldState WorldState => Bossmods.WorldState;
    public readonly AIHints Hints;
    public readonly ActionManagerEx ActionManager; // TODO: make private
    private readonly UISimpleWindow _ui;
    private readonly EventSubscriptions _subscriptions;

    public CommonActions? ClassActions { get; private set; }

    public Actor? PrimaryTarget; // this is usually a normal (hard) target, but AI can override; typically used for damage abilities
    public Actor? SecondaryTarget; // this is usually a mouseover, but AI can override; typically used for heal and utility abilities
    public float EffAnimLock => ActionManager.EffectiveAnimationLock;
    public float AnimLockDelay => ActionManager.AnimationLockDelayEstimate;

    public unsafe Autorotation(BossModuleManager bossmods, AIHints hints, ActionManagerEx amex)
    {
        Bossmods = bossmods;
        Hints = hints;
        ActionManager = amex;
        _ui = new("Autorotation", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing) { RespectCloseHotkey = false };
        _subscriptions = new
        (
            WorldState.Client.ActionRequested.Subscribe(OnActionRequested),
            WorldState.Actors.CastEvent.Subscribe(OnCastEvent)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        _ui.Dispose();
        ClassActions?.Dispose();
    }

    public unsafe void Update(ActionQueue queue)
    {
        var player = WorldState.Party.Player();
        PrimaryTarget = Hints.ForcedTarget ?? WorldState.Actors.Find(player?.TargetID ?? 0);
        SecondaryTarget = WorldState.Actors.Find(Utils.MouseoverID());

        // update forced target, if needed (TODO: move outside maybe?)
        if (Hints.ForcedTarget != null)
        {
            var obj = Hints.ForcedTarget.SpawnIndex >= 0 ? FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[Hints.ForcedTarget.SpawnIndex].Value : null;
            if (obj != null && obj->EntityId != Hints.ForcedTarget.InstanceID)
                Service.Log($"[AR] Unexpected new target: expected {Hints.ForcedTarget.InstanceID:X} at #{Hints.ForcedTarget.SpawnIndex}, but found {obj->EntityId:X}");
            FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->Target = obj;
        }

        // TODO: move to rotationmodulemanager...
        Type? classType = null;
        if (Config.Enabled && player != null)
        {
            classType = player.Class switch
            {
                Class.WAR => typeof(WAR.Actions),
                //Class.PLD => player.Level <= 60 ? typeof(PLD.Actions) : null,
                //Class.MNK => typeof(MNK.Actions),
                //Class.DRG => typeof(DRG.Actions),
                //Class.BRD => typeof(BRD.Actions),
                //Class.BLM => player.Level <= 60 ? typeof(BLM.Actions) : null,
                //Class.SMN => player.Level <= 30 ? typeof(SMN.Actions) : null,
                //Class.WHM => typeof(WHM.Actions),
                //Class.SCH => player.Level <= 60 ? typeof(SCH.Actions) : null,
                //Class.RPR => typeof(RPR.Actions),
                //Class.GNB => typeof(GNB.Actions),
                //Class.SAM => typeof(SAM.Actions),
                //Class.DNC => typeof(DNC.Actions),
                _ => null
            };
        }

        if (ClassActions?.GetType() != classType || ClassActions?.Player != player)
        {
            ClassActions?.Dispose();
            ClassActions = classType != null ? (CommonActions?)Activator.CreateInstance(classType, this, player) : null;
        }

        ClassActions?.Update();
        ClassActions?.CalculateNextActions(queue);
        ClassActions?.FillStatusesToCancel(Hints.StatusesToCancel);

        foreach (var s in Hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId);
            Service.Log($"[AR] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }

        _ui.IsOpen = ClassActions != null && Config.ShowUI;

        if (Config.ShowPositionals && PrimaryTarget != null && ClassActions != null && ClassActions.AutoAction != CommonActions.AutoActionNone && !PrimaryTarget.Omnidirectional)
        {
            var strategy = ClassActions.GetStrategy();
            var color = PositionalColor(strategy);
            switch (strategy.NextPositional)
            {
                case Positional.Flank:
                    Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation + 90.Degrees(), 45.Degrees(), color);
                    Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation - 90.Degrees(), 45.Degrees(), color);
                    break;
                case Positional.Rear:
                    Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation + 180.Degrees(), 45.Degrees(), color);
                    break;
            }
        }
    }

    private void DrawOverlay()
    {
        if (ClassActions == null)
            return;
        var next = ActionManager.AutoQueue;
        var state = ClassActions.GetState();
        var strategy = ClassActions.GetStrategy();
        ImGui.TextUnformatted($"[{ClassActions.AutoAction}] Next: {next.Action} ({next.Priority:f2})");
        if (ClassActions.AutoAction != CommonActions.AutoActionNone && strategy.NextPositional != Positional.Any)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, PositionalColor(strategy));
            ImGui.TextUnformatted(strategy.NextPositional.ToString());
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        ImGui.TextUnformatted(strategy.ToString());
        ImGui.TextUnformatted($"Raidbuffs: {state.RaidBuffsLeft:f2}s left, next in {strategy.RaidBuffsIn:f2}s");
        ImGui.TextUnformatted($"Downtime: {strategy.FightEndIn:f2}s, pos-lock: {strategy.PositionLockIn:f2}");
        ImGui.TextUnformatted($"GCD={WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining:f3}, AnimLock={EffAnimLock:f3}+{AnimLockDelay:f3}, Combo={state.ComboTimeLeft:f3}");
    }

    private void OnActionRequested(ClientState.OpActionRequest op)
    {
        ClassActions?.NotifyActionExecuted(op.Request);
    }

    private void OnCastEvent(Actor actor, ActorCastEvent cast)
    {
        if (cast.SourceSequence != 0 && actor == WorldState.Party.Player())
            ClassActions?.NotifyActionSucceeded(cast);
    }

    private uint PositionalColor(CommonRotation.Strategy strategy)
    {
        return strategy.NextPositionalImminent
            ? (strategy.NextPositionalCorrect ? 0xff00ff00 : 0xff0000ff)
            : (strategy.NextPositionalCorrect ? 0xffffffff : 0xff00ffff);
    }
}
