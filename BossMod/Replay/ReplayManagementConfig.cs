namespace BossMod;

[ConfigDisplay(Name = "录像设置", Order = 0)]
public class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("显示录像管理界面")]
    public bool ShowUI = false;
    
    [PropertyDisplay("副本开始自动录像")]
    public bool AutoRecord = true;

    [PropertyDisplay("最大录像数量")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 20;

    [PropertyDisplay("在录像中记录和存储服务器数据包")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("在录像中存储服务器数据包")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("Ignore packets for other players when dumping to dalamud.log")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("Dump client packets into dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("格式化录像 log")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;
}
