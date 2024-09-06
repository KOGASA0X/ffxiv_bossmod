namespace BossMod;

[ConfigDisplay(Name = "动作调整", Order = 4)]
public sealed class ActionTweaksConfig : ConfigNode
{
    // TODO: 考虑将最大延迟暴露给配置；0表示'移除所有延迟'，最大值表示'禁用'
    [PropertyDisplay("移除瞬发技能因延迟引起的额外动画锁定延迟（请阅读提示！）", tooltip: "请不要与XivAlexander或NoClippy一起使用——如果检测到这些工具，它应会自动禁用，但请务必先检查！")]
    public bool RemoveAnimationLockDelay = false;

    [PropertyDisplay("移除因帧率引起的额外冷却延迟", tooltip: "动态调整冷却和动画锁定，以确保无论帧率限制如何，队列中的动作都能立即执行")]
    public bool RemoveCooldownDelay = false;

    [PropertyDisplay("防止施法时移动")]
    public bool PreventMovingWhileCasting = false;


    public enum ModifierKey
    {
        [PropertyDisplay("None")]
        None,

        [PropertyDisplay("Control")]
        Ctrl,

        [PropertyDisplay("Alt")]
        Alt,

        [PropertyDisplay("Shift")]
        Shift,

        [PropertyDisplay("LMB + RMB")]
        M12
    }


    [PropertyDisplay("按住此键可在施法时允许移动", tooltip: "需要同时启用上面的设置")]
    public ModifierKey MoveEscapeHatch = ModifierKey.None;

    [PropertyDisplay("当目标死亡时自动取消施法")]
    public bool CancelCastOnDeadTarget = false;

    [PropertyDisplay("使用技能后恢复角色朝向", tooltip: "如果游戏设置中的\"自动面向目标\"选项被禁用，此选项将无效")]
    public bool RestoreRotation = false;

    [PropertyDisplay("对鼠标悬停的目标使用技能")]
    public bool PreferMouseover = false;

    [PropertyDisplay("智能技能目标选择", tooltip: "如果通常的目标（鼠标悬停/主要目标）不适合使用某个技能，则自动选择下一个最佳目标（例如为副坦使用Shirk）")]
    public bool SmartTargets = true;

    [PropertyDisplay("为手动按下的技能使用自定义队列", tooltip: "此设置可以更好地与自动循环结合，并防止在自动循环过程中按下治疗技能时出现三次编织或GCD漂移的情况")]
    public bool UseManualQueue = false;

    [PropertyDisplay("自动下坐骑以执行技能")]
    public bool AutoDismount = true;

    public enum GroundTargetingMode
    {
        [PropertyDisplay("通过额外点击手动选择位置（正常游戏行为）")]
        Manual,

        [PropertyDisplay("在当前鼠标位置施放")]
        AtCursor,

        [PropertyDisplay("在选定目标的位置施放")]
        AtTarget
    }

    [PropertyDisplay("地面目标技能的自动目标选择")]
    public GroundTargetingMode GTMode = GroundTargetingMode.Manual;
}
