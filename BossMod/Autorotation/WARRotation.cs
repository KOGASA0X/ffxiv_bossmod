﻿using System;
using System.Collections.Generic;

namespace BossMod
{
    public class WARRotation
    {
        public enum AID : uint
        {
            None = 0,

            // single target GCDs
            HeavySwing = 31,
            Maim = 37,
            StormPath = 42,
            StormEye = 45,
            InnerBeast = 49,
            FellCleave = 3549,
            InnerChaos = 16465,
            PrimalRend = 25753,

            // aoe GCDs
            Overpower = 41,
            MythrilTempest = 16462,
            SteelCyclone = 51,
            Decimate = 3550,
            ChaoticCyclone = 16463,

            // oGCDs
            Infuriate = 52,
            Onslaught = 7386,
            Upheaval = 7387,
            Orogeny = 25752,

            // offsensive CDs
            Berserk = 38,
            InnerRelease = 7389,

            // defensive CDs
            Rampart = 7531,
            Vengeance = 44,
            ThrillOfBattle = 40,
            Holmgang = 43,
            Equilibrium = 3552,
            Reprisal = 7535,
            ShakeItOff = 7388,
            RawIntuition = 3551,
            NascentFlash = 16464,
            Bloodwhetting = 25751,
            ArmsLength = 7548,

            // misc
            Tomahawk = 46,
            Defiance = 48,
            Provoke = 7533,
            Shirk = 7537,
            LowBlow = 7540,
            Interject = 7538,
        }
        public static ActionID IDSprint = new(ActionType.General, 4);
        public static ActionID IDStatPotion = new(ActionType.Item, 1036109); // hq grade 6 tincture of strength

        public enum SID : uint
        {
            None = 0,
            SurgingTempest = 2677, // applied by StormEye, damage buff
            NascentChaos = 1897, // applied by Infuriate, converts next FC to IC
            InnerRelease = 1177, // applied by InnerRelease, next 3 GCDs should be free FCs
            PrimalRend = 2624, // applied by InnerRelease, allows casting PR
        }

        // full state needed for determining next action
        public class State
        {
            public int Level;
            public float GCD; // 2.5 max (decreased by SkS), 0 if not on gcd
            public float AnimationLock; // typical actions have 0.6 delay, but some (notably primal rend and potion) are >1
            public float AnimationLockDelay; // average time between action request and confirmation; this is added to effective animation lock for actions
            public float ComboTimeLeft; // 0 if not in combo, max 30
            public AID ComboLastMove;
            public int Gauge; // 0 to 100
            public float RaidBuffsLeft; // 0 if no damage-up status is up, otherwise it is time left on longest
            public float SurgingTempestLeft; // 0 if buff not up, max 60
            public float NascentChaosLeft; // 0 if buff not up, max 30
            public float PrimalRendLeft; // 0 if buff not up, max 30
            public float InnerReleaseLeft; // 0 if buff not up, max 15
            public int InnerReleaseStacks; // 0 if buff not up, max 3
            public float InnerReleaseCD; // 60 max, 0 if ready
            public float InfuriateCD; // 120 max, >60 if 0 stacks ready, >0 if 1 stack ready, ==0 if 2 stacks ready
            public float UpheavalCD; // 60 max, 0 if ready
            public float OnslaughtCD; // 90 max, >60 if 0 stacks ready, >30 if 1 stack ready, >0 if 2 stacks ready, ==0 if 3 stacks ready
            public float RampartCD; // 90 max, 0 if ready
            public float VengeanceCD; // 120 max, 0 if ready
            public float ThrillOfBattleCD; // 90 max, 0 if ready
            public float HolmgangCD; // 240 max, 0 if ready
            public float EquilibriumCD; // 60 max, 0 if ready
            public float ReprisalCD; // 60 max, 0 if ready
            public float ShakeItOffCD; // 90 max, 0 if ready
            public float BloodwhettingCD; // 25 max, 0 if ready (also applies to nascent flash)
            public float ArmsLengthCD; // 120 max, 0 if ready
            public float ProvokeCD; // 30 max, 0 if ready
            public float ShirkCD; // 120 max, 0 if ready
            public float SprintCD; // 60 max, 0 if ready
            public float PotionCD; // variable max, 0 if ready

            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
            public bool UnlockedMaim => Level >= 4;
            public bool UnlockedBerserk => Level >= 6;
            public bool UnlockedRampart => Level >= 8;
            public bool UnlockedOverpower => Level >= 10;
            public bool UnlockedDefiance => Level >= 10;
            public bool UnlockedLowBlow => Level >= 12;
            public bool UnlockedProvoke => Level >= 15;
            public bool UnlockedTomahawk => Level >= 15; // quest-locked
            public bool UnlockedInterject => Level >= 18;
            public bool UnlockedReprisal => Level >= 22;
            public bool UnlockedStormPath => Level >= 26;
            public bool UnlockedThrillOfBattle => Level >= 30; // quest-locked
            public bool UnlockedArmsLength => Level >= 32;
            public bool UnlockedInnerBeast => Level >= 35; // quest-locked
            public bool UnlockedVengeance => Level >= 38;
            public bool UnlockedMythrilTempest => Level >= 40; // quest-locked
            public bool UnlockedHolmgang => Level >= 42;
            public bool UnlockedSteelCyclone => Level >= 45; // quest-locked
            public bool UnlockedShirk => Level >= 48;
            public bool UnlockedStormEye => Level >= 50;
            public bool UnlockedInfuriate => Level >= 50; // quest-locked
            public bool UnlockedFellCleave => Level >= 54; // quest-locked
            public bool UnlockedRawIntuition => Level >= 56; // quest-locked
            public bool UnlockedEquilibrium => Level >= 58; // quest-locked
            public bool UnlockedDecimate => Level >= 60; // quest-locked
            public bool UnlockedOnslaught => Level >= 62;
            public bool UnlockedUpheaval => Level >= 64;
            public bool UnlockedEnhancedInfuriate => Level >= 66; // passive, gauge spenders reduce infCD by 5
            public bool UnlockedShakeItOff => Level >= 68;
            public bool UnlockedInnerRelease => Level >= 70; // quest-locked
            public bool UnlockedChaoticCyclone => Level >= 72;
            public bool UnlockedMasteringTheBeast => Level >= 74; // passive, mythril tempest gives gauge
            public bool UnlockedNascentFlash => Level >= 76;
            public bool UnlockedInnerChaos => Level >= 80;
            public bool UnlockedBloodwhetting => Level >= 82;
            public bool UnlockedOrogeny => Level >= 86;
            public bool UnlockedEnhancedOnslaught => Level >= 88; // passive, gives third charge to onslaught
            public bool UnlockedPrimalRend => Level >= 90;
        }

        // strategy configuration
        // many strategy decisions are represented as "need-something-in" counters; 0 means "use asap", >0 means "do not use unless value is larger than cooldown" (so 'infinity' means 'free to use')
        // for planning, we typically use "windows" (as in, some CD has to be pressed from this point and up to this point);
        // before "min point" counter is >0, between "min point" and "max point" it is == 0, after "max point" we switch to next planned action (assuming if we've missed the window, CD is no longer needed)
        public class Strategy
        {
            public enum PotionUse { Manual, DelayUntilBuffs, DelayUntilIR, Immediate }

            public float FightEndIn; // how long fight will last (we try to spend all resources before this happens)
            public float RaidBuffsIn; // estimate time when new raidbuff window starts (if it is smaller than FightEndIn, we try to conserve resources)
            public float PositionLockIn; // time left to use moving abilities (Primal Rend and Onslaught) - we won't use them if it is ==0; setting this to 2.5f will make us use PR asap
            public float FirstChargeIn; // when do we need to use onslaught charge (0 means 'use asap if out of melee range', >0 means that we'll try to make sure 1 charge is available in this time)
            public float SecondChargeIn; // when do we need to use two onslaught charges in a short amount of time
            public PotionUse Potion; // strategy for automatic potion use
            public bool EnableUpheaval = true; // if true, enable using upheaval when needed; setting to false is useful during opener before first party buffs
            public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net
            // 'execute' flags: if true, we execute corresponding action in next free ogcd slot (potentially delaying a bit to avoid losing damage)
            // we assume the logic setting the flag ensures the action isn't wasted (e.g. equilibrium at full hp, reprisal out of range of any enemies, SIO out of range of full raid, etc.)
            public bool ExecuteRampart;
            public bool ExecuteVengeance; // note: it has some minor offensive value, but we prefer letting plan the usage explicitly for that
            public bool ExecuteThrillOfBattle;
            public bool ExecuteHolmgang;
            public bool ExecuteEquilibrium;
            public bool ExecuteReprisal;
            public bool ExecuteShakeItOff;
            public bool ExecuteBloodwhetting;
            public bool ExecuteNascentFlash;
            public bool ExecuteArmsLength;
            public bool ExecuteProvoke;
            public bool ExecuteShirk;
            public bool ExecuteSprint;
        }

        public static int GaugeGainedFromAction(State state, AID action)
        {
            return action switch
            {
                AID.Maim or AID.StormEye => 10,
                AID.StormPath => 20,
                AID.MythrilTempest => state.UnlockedMasteringTheBeast ? 20 : 0,
                _ => 0
            };
        }

        public static AID GetNextSTComboAction(AID comboLastMove, AID finisher)
        {
            return comboLastMove switch
            {
                AID.Maim => finisher,
                AID.HeavySwing => AID.Maim,
                _ => AID.HeavySwing
            };
        }

        public static int GetSTComboLength(AID comboLastMove)
        {
            return comboLastMove switch
            {
                AID.Maim => 1,
                AID.HeavySwing => 2,
                _ => 3
            };
        }

        public static AID GetNextMaimComboAction(AID comboLastMove)
        {
            return comboLastMove == AID.HeavySwing ? AID.Maim : AID.HeavySwing;
        }

        public static AID GetNextAOEComboAction(AID comboLastMove)
        {
            return comboLastMove == AID.Overpower ? AID.MythrilTempest : AID.Overpower;
        }

        public static AID GetNextUnlockedComboAction(State state, float minBuffToRefresh, bool aoe)
        {
            if (aoe && state.UnlockedOverpower)
            {
                // for AOE rotation, assume dropping ST combo is fine
                return state.UnlockedMythrilTempest && state.ComboLastMove == AID.Overpower ? AID.MythrilTempest : AID.Overpower;
            }
            else
            {
                // for ST rotation, assume dropping AOE combo is fine (HS is 200 pot vs MT 100, is 20 gauge + 30 sec ST worth it?..)
                return state.ComboLastMove switch
                {
                    AID.Maim => state.UnlockedStormPath ? (state.UnlockedStormEye && state.SurgingTempestLeft < minBuffToRefresh ? AID.StormEye : AID.StormPath) : AID.HeavySwing,
                    AID.HeavySwing => state.UnlockedMaim ? AID.Maim : AID.HeavySwing,
                    _ => AID.HeavySwing
                };
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            // we spend resources either under raid buffs or if another raid buff window will cover at least 4 GCDs of the fight
            bool spendGauge = state.RaidBuffsLeft > state.GCD || strategy.FightEndIn <= strategy.RaidBuffsIn + 10;
            float primalRendWindow = MathF.Min(state.PrimalRendLeft, strategy.PositionLockIn);
            var nextFCAction = state.NascentChaosLeft > state.GCD ? (state.UnlockedInnerChaos && !aoe ? AID.InnerChaos : AID.ChaoticCyclone)
                : (aoe && state.UnlockedSteelCyclone) ? (state.UnlockedDecimate ? AID.Decimate : AID.SteelCyclone)
                : (state.UnlockedFellCleave ? AID.FellCleave : AID.InnerBeast);

            // 1. if it is the last CD possible for PR/NC, don't waste them
            float gcdDelay = state.GCD + (strategy.Aggressive ? 0 : 2.5f);
            float secondGCDIn = gcdDelay + 2.5f;
            float thirdGCDIn = gcdDelay + 5f;
            if (primalRendWindow > state.GCD && primalRendWindow < secondGCDIn)
                return AID.PrimalRend;
            if (state.NascentChaosLeft > state.GCD && state.NascentChaosLeft < secondGCDIn)
                return nextFCAction;
            if (primalRendWindow > state.GCD && state.NascentChaosLeft > state.GCD && primalRendWindow < thirdGCDIn && state.NascentChaosLeft < thirdGCDIn)
                return AID.PrimalRend; // either is fine

            // 2. if IR/berserk is up, don't waste charges
            if (state.InnerReleaseStacks > 0)
            {
                if (state.UnlockedInnerRelease)
                {
                    // it is only safe to cast FC/PR
                    // currently we prioritize PR in 'spend' phase, unless doing so will cost us IR stacks
                    bool preferPR = primalRendWindow > state.GCD && spendGauge && state.InnerReleaseLeft >= (gcdDelay + state.InnerReleaseStacks * 2.5);
                    return preferPR ? AID.PrimalRend : nextFCAction;
                }
                else if (state.Gauge >= 50 && (state.UnlockedFellCleave || state.ComboLastMove != AID.Maim || aoe && state.UnlockedSteelCyclone))
                {
                    // single-target: FC > SE/ST > IB > Maim > HS
                    // aoe: Decimate > SC > Combo
                    return nextFCAction;
                }
            }

            // 3. no ST (or it will expire if we don't combo asap) => apply buff asap
            // TODO: what if we have really high gauge and low ST? is it worth it to delay ST application to avoid overcapping gauge?
            if (!aoe)
            {
                if (state.UnlockedStormEye && state.SurgingTempestLeft <= state.GCD + 2.5f * (GetSTComboLength(state.ComboLastMove) - 1))
                    return GetNextSTComboAction(state.ComboLastMove, AID.StormEye);
            }
            else
            {
                if (state.UnlockedMasteringTheBeast && state.SurgingTempestLeft <= state.GCD + (state.ComboLastMove != AID.Overpower ? 2.5f : 0))
                    return GetNextAOEComboAction(state.ComboLastMove);
            }

            // 4. if we're delaying IR due to nascent chaos, cast it asap
            if (state.NascentChaosLeft > 0 && state.InnerReleaseCD <= secondGCDIn && state.Gauge >= 50)
                return nextFCAction;

            // 5. if we're delaying Infuriate due to gauge, cast FC asap (7.5 for FC)
            if (state.Gauge > 50 && state.UnlockedInfuriate && state.InfuriateCD <= gcdDelay + 7.5)
                return nextFCAction;

            // 6. if we have >50 gauge, IR is imminent, and casting it will cause us to overcap infuriate, spend gauge asap, so that we can infuriate after
            // 30 seconds is for FC + IR + 3xFC - this is 4 gcds (10 sec) and 4 FCs (another 20 sec)
            if (state.Gauge > 50 && state.UnlockedInfuriate && state.InfuriateCD <= gcdDelay + 30 && state.InnerReleaseCD < thirdGCDIn)
                return nextFCAction;

            // 7. if there is no chance we can delay PR until next raid buffs, just cast it now
            if (primalRendWindow > state.GCD && primalRendWindow <= strategy.RaidBuffsIn)
                return AID.PrimalRend;

            // TODO: do not spend gauge if we're delaying berserk
            if (!spendGauge)
            {
                // we want to delay spending gauge unless doing so will cause us problems later
                var maxSTToAvoidOvercap = 20 + Math.Clamp(state.InnerReleaseCD, 0, 10);
                var nextCombo = GetNextUnlockedComboAction(state, maxSTToAvoidOvercap, aoe);
                if (state.Gauge + GaugeGainedFromAction(state, nextCombo) <= 100)
                    return nextCombo;
            }

            // ok at this point, we just want to spend gauge - either because we're using greedy strategy, or something prevented us from casting combo
            if (primalRendWindow > state.GCD)
                return AID.PrimalRend;
            if (state.Gauge >= 50)
                return nextFCAction;

            // TODO: reconsider min time left...
            return GetNextUnlockedComboAction(state, gcdDelay + 12.5f, aoe);
        }

        public static bool IsOGCDAvailable(float cooldown, float actionLock, float delay, float windowEnd)
        {
            return MathF.Max(cooldown, 0) + actionLock + delay <= windowEnd;
        }

        public static float GetOGCDDelay(float animLockDelay)
        {
            return 0.1f; // TODO: consider returning animLockDelay instead...
        }

        // check whether berserk should be delayed (we want to spend it on FCs)
        // this is relevant only until we unlock IR
        public static bool DelayBerserk(State state)
        {
            if (state.UnlockedInfuriate)
            {
                // we really want to cast SP + 2xIB or 3xIB under berserk; check whether we'll have infuriate before third GCD
                var availableGauge = state.Gauge;
                if (state.InfuriateCD <= 65)
                    availableGauge += 50;
                return state.ComboLastMove switch
                {
                    AID.Maim => availableGauge < 80,
                    _ => availableGauge < 150
                };
            }
            else if (state.UnlockedInnerBeast)
            {
                // pre level 50 we ideally want to cast SP + 2xIB under berserk (we need to have 80+ gauge for that)
                // however, we are also content with casting Maim + SP + IB (we need to have 20+ gauge for that; but if we have 70+, it is better to delay for 1 GCD)
                // alternatively, we could delay for 3 GCDs at 40+ gauge - TODO determine which is better
                return state.ComboLastMove switch
                {
                    AID.HeavySwing => state.Gauge < 20 || state.Gauge >= 70,
                    AID.Maim => state.Gauge < 80,
                    _ => true,
                };
            }
            else
            {
                // pre level 35 there is no point delaying berserk at all
                return false;
            }
        }

        // window-end is either GCD or GCD - time-for-second-ogcd; we are allowed to use ogcds only if their animation lock would complete before window-end
        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe)
        {
            var lockDelay = GetOGCDDelay(state.AnimationLockDelay);

            bool infuriateAvailable = state.UnlockedInfuriate && IsOGCDAvailable(state.InfuriateCD - 60, 0.6f, lockDelay, windowEnd); // note: for second stack, this will be true if casting it won't delay our next gcd
            infuriateAvailable &= state.Gauge <= 50; // never cast infuriate if doing so would overcap gauge
            if (state.UnlockedChaoticCyclone)
            {
                // if we have NC, we should not cast infuriate under IR or if we haven't spent NC yet
                infuriateAvailable &= state.InnerReleaseLeft <= state.GCD ; // never cast infuriate if IR is up for next GCD
                infuriateAvailable &= state.NascentChaosLeft <= state.GCD; // never cast infuriate if NC from previous infuriate is still up for next GCD
            }

            // 1. spend second infuriate stacks asap (unless have IR, another NC, or >50 gauge)
            // note that next-best-gcd could be FC, so we bump up min CD to ensure we don't overcap
            if (infuriateAvailable && state.InfuriateCD <= state.GCD + 7.5)
                return ActionID.MakeSpell(AID.Infuriate);

            // 2. use cooldowns if requested in rough priority order
            if (strategy.ExecuteProvoke && state.UnlockedProvoke && IsOGCDAvailable(state.ProvokeCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.Provoke);
            if (strategy.ExecuteShirk && state.UnlockedShirk && IsOGCDAvailable(state.ShirkCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.Shirk);
            if (strategy.ExecuteHolmgang && state.UnlockedHolmgang && IsOGCDAvailable(state.HolmgangCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.Holmgang);
            if (strategy.ExecuteArmsLength && state.UnlockedArmsLength && IsOGCDAvailable(state.ArmsLengthCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.ArmsLength);
            if (strategy.ExecuteThrillOfBattle && state.UnlockedThrillOfBattle && IsOGCDAvailable(state.ThrillOfBattleCD, 0.6f, lockDelay, windowEnd)) // prefer using thrill before SOI, so that it can be eaten
                return ActionID.MakeSpell(AID.ThrillOfBattle);
            if (strategy.ExecuteEquilibrium && state.UnlockedEquilibrium && IsOGCDAvailable(state.EquilibriumCD, 0.6f, lockDelay, windowEnd)) // prefer to use equilibrium after thrill for extra healing
                return ActionID.MakeSpell(AID.Equilibrium);
            if (strategy.ExecuteShakeItOff && state.UnlockedShakeItOff && IsOGCDAvailable(state.ShakeItOffCD, 0.6f, lockDelay, windowEnd)) // prefer to use SOI after thrill (to consume it), but before vengeance & bloodwhetting (too useful)
                return ActionID.MakeSpell(AID.ShakeItOff);
            if (strategy.ExecuteVengeance && state.UnlockedVengeance && IsOGCDAvailable(state.VengeanceCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.Vengeance);
            if (strategy.ExecuteRampart && state.UnlockedRampart && IsOGCDAvailable(state.RampartCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.Rampart);
            if (strategy.ExecuteReprisal && state.UnlockedReprisal && IsOGCDAvailable(state.ReprisalCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.Reprisal);
            if (strategy.ExecuteBloodwhetting && state.UnlockedRawIntuition && IsOGCDAvailable(state.BloodwhettingCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(state.UnlockedBloodwhetting ? AID.Bloodwhetting : AID.RawIntuition);
            if (strategy.ExecuteNascentFlash && state.UnlockedNascentFlash && IsOGCDAvailable(state.BloodwhettingCD, 0.6f, lockDelay, windowEnd))
                return ActionID.MakeSpell(AID.NascentFlash);
            if (strategy.ExecuteSprint && IsOGCDAvailable(state.SprintCD, 0.6f, lockDelay, windowEnd))
                return IDSprint;

            // 3. potion, if required by strategy, and not too early in opener (TODO: reconsider priority)
            bool allowPotion = strategy.Potion switch
            {
                Strategy.PotionUse.DelayUntilBuffs => state.RaidBuffsLeft > 0 || strategy.RaidBuffsIn <= 2.5, // TODO: reconsider timings?..
                Strategy.PotionUse.DelayUntilIR => state.InnerReleaseStacks > 0 || state.InnerReleaseCD < 15, // TODO: reconsider timings?..
                Strategy.PotionUse.Immediate => true,
                _ => false,
            };
            // note: this check will not allow using potions before lvl 50, but who cares...
            if (allowPotion && IsOGCDAvailable(state.PotionCD, 1.1f, lockDelay, windowEnd) && (state.SurgingTempestLeft > 0 || state.ComboLastMove == AID.Maim))
                return IDStatPotion;

            // 4. upheaval, if surging tempest up and not forbidden
            // TODO: delay for 1 GCD during opener...
            if (state.UnlockedUpheaval && IsOGCDAvailable(state.UpheavalCD, 0.6f, lockDelay, windowEnd) && state.SurgingTempestLeft > MathF.Max(state.UpheavalCD, 0) && strategy.EnableUpheaval)
                return ActionID.MakeSpell(aoe && state.UnlockedOrogeny ? AID.Orogeny : AID.Upheaval);

            // 5. inner release, if surging tempest up and no nascent chaos up
            // if not unlocked yet, use berserk instead, but only if we have enough gauge
            bool irReady = IsOGCDAvailable(state.InnerReleaseCD, 0.6f, lockDelay, windowEnd);
            if (state.UnlockedBerserk && irReady && (state.SurgingTempestLeft > state.GCD + 5 || !state.UnlockedStormEye) && state.NascentChaosLeft <= state.GCD)
            {
                if (state.UnlockedInnerRelease)
                    return ActionID.MakeSpell(AID.InnerRelease);
                else if (aoe || !DelayBerserk(state))
                    return ActionID.MakeSpell(AID.Berserk);
            }

            // 6. infuriate - this is complex decision
            bool spendGauge = state.RaidBuffsLeft >= state.GCD || strategy.FightEndIn <= strategy.RaidBuffsIn + 10;
            if (infuriateAvailable)
            {
                if (!state.UnlockedInnerRelease)
                {
                    // no inner release yet; pressing infuriate under berserk is actually quite a good idea
                    // otherwise, press it while spending gauge, unless berserk is imminent
                    if (state.InnerReleaseLeft > state.GCD)
                        return ActionID.MakeSpell(AID.Infuriate);
                    if (spendGauge && state.InnerReleaseCD > state.GCD + 10) // TODO: think about exact condition
                        return ActionID.MakeSpell(AID.Infuriate);
                }
                else if (!irReady)
                {
                    // inner release unlocked, we'll not overcap gauge or interfere with IR/NC, so we can press infuriate, but it still might be worth to delay it a bit
                    // if we're spending gauge - just do it, this is the moment we've been delaying it for :)
                    // otherwise, we're hitting infuriate when its CD is too low and we risk overcapping it:
                    // - if IR is imminent, we need at least 22.5 secs of CD (IR+3xFC is 7.5s from spent gcds and 15s from FCs)
                    // - if next combo action would overcap our gauge, we need at least 10 secs of CD (it+FC would take 2 gcds)
                    // - otherwise we need to still be not overcapping by the next GCD
                    if (spendGauge)
                        return ActionID.MakeSpell(AID.Infuriate);

                    float gcdDelay = state.GCD + (strategy.Aggressive ? 0 : 2.5f);
                    var irImminent = state.InnerReleaseCD < gcdDelay + 2.5;
                    int gaugeCap = state.ComboLastMove == AID.None ? 50 : (state.ComboLastMove == AID.HeavySwing ? 40 : 30);
                    float maxInfuriateCD = irImminent ? 22.5f : (state.Gauge > gaugeCap ? 10f : 2.5f);
                    if (state.InfuriateCD <= gcdDelay + maxInfuriateCD)
                        return ActionID.MakeSpell(AID.Infuriate);
                }
            }

            // 7. onslaught, if surging tempest up and not forbidden
            if (state.UnlockedOnslaught && IsOGCDAvailable(state.OnslaughtCD - (state.UnlockedEnhancedOnslaught ? 60 : 30), 0.6f, lockDelay, windowEnd) && strategy.PositionLockIn > state.AnimationLock && state.SurgingTempestLeft > state.AnimationLock)
            {
                if (state.OnslaughtCD < state.GCD + 2.5)
                    return ActionID.MakeSpell(AID.Onslaught); // onslaught now, otherwise we risk overcapping charges

                if (strategy.FirstChargeIn <= 0)
                    return ActionID.MakeSpell(AID.Onslaught); // onslaught now, since strategy asks for it

                // check whether using onslaught now won't prevent us from using it when strategy demands
                bool safeToUseOnslaught = state.UnlockedEnhancedOnslaught ? (state.OnslaughtCD <= strategy.FirstChargeIn + 30 && state.OnslaughtCD <= strategy.SecondChargeIn) : (state.OnslaughtCD <= strategy.FirstChargeIn);

                // use onslaught now if it's safe and we're either spending gauge or won't be able to delay it until next buff window anyway
                if (safeToUseOnslaught && (spendGauge || state.OnslaughtCD <= strategy.RaidBuffsIn))
                    return ActionID.MakeSpell(AID.Onslaught);
            }

            // no suitable oGCDs...
            return new();
        }

        public static ActionID GetNextBestAction(State state, Strategy strategy, bool aoe)
        {
            var ogcdSlotLength = 0.6f + GetOGCDDelay(state.AnimationLockDelay);

            // first ogcd slot
            var doubleWeavingWindowEnd = state.GCD - ogcdSlotLength;
            if (state.AnimationLock + ogcdSlotLength <= doubleWeavingWindowEnd)
            {
                var ogcd = GetNextBestOGCD(state, strategy, doubleWeavingWindowEnd, aoe);
                if (ogcd)
                    return ogcd;
            }

            // second/only ogcd slot
            if (state.AnimationLock + ogcdSlotLength <= state.GCD)
            {
                var ogcd = GetNextBestOGCD(state, strategy, state.GCD, aoe);
                if (ogcd)
                    return ogcd;
            }

            return ActionID.MakeSpell(GetNextBestGCD(state, strategy, aoe));
        }

        // short string for supported action
        public static string ActionShortString(ActionID action)
        {
            return action == IDSprint ? "Sprint" : action == IDStatPotion ? "StatPotion" : ((AID)action.ID).ToString();
        }
    }
}
