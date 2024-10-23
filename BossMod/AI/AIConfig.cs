namespace BossMod.AI;

[ConfigDisplay(Name = "AI配置", Order = 7)]
sealed class AIConfig : ConfigNode
{
    public enum Slot
    {
        One,
        Two,
        Three,
        Four
    }

    // UI设置
    [PropertyDisplay("启用AI", tooltip: "免责声明：AI处于非常实验性的阶段，请自行承担风险！")]
    public bool Enabled = false;

    [PropertyDisplay("显示游戏内UI")]
    public bool DrawUI = false;

    [PropertyDisplay("在UI中显示高级选项")]
    public bool ShowExtraUIOptions = true;

    [PropertyDisplay("在游戏内UI标题栏中显示AI状态")]
    public bool ShowStatusOnTitlebar = true;

    [PropertyDisplay("在服务器信息栏中显示AI状态")]
    public bool ShowDTR = true;

    // AI设置
    [PropertyDisplay("覆盖位置偏好")]
    public bool OverridePositional = false;

    [PropertyDisplay("期望的位置偏好")]
    public Positional DesiredPositional = 0;

    [PropertyDisplay("跟随槽位")]
    public Slot FollowSlot = 0;

    [PropertyDisplay("覆盖跟随距离")]
    public bool OverrideRange = false;

    [PropertyDisplay("跟随槽位距离")]
    public float MaxDistanceToSlot = 1;

    [PropertyDisplay("跟随目标")]
    public bool FollowTarget = false;

    [PropertyDisplay("跟随目标距离")]
    public float MaxDistanceToTarget = 2.6f;

    [PropertyDisplay("在活跃的boss模块期间跟随")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("在战斗中跟随")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("在非战斗状态下跟随")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("禁止移动")]
    public bool ForbidMovement = false;

    [PropertyDisplay("禁止动作")]
    public bool ForbidActions = false;

    [PropertyDisplay("自动攻击FATE怪物", since: "0.0.0.253")]
    public bool AutoFate = true;

    [PropertyDisplay("目标设置为焦点")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("将按键广播到其他窗口")]
    public bool BroadcastToSlaves = false;
}