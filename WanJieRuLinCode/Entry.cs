using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace WanJieRuLin;

[ModInitializer(nameof(Initialize))]
public partial class Entry
{
    // ModId 需要和 WanJieRuLin.json 里的 id 保持一致。
    // res://WanJieRuLin/... 里的 WanJieRuLin 是 PCK 资源目录，不是 C# namespace。
    public const string ModId = "WanJieRuLin";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Godot C# 脚本注册只负责让 pck 中的脚本类型能被 Godot 找到。
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // 自动注册扫描会读取当前程序集里的 RegisterCard/RegisterRelic 等 attribute。
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        // 注册"鬼气"次要资源。必须在游戏内容加载前完成，否则卡牌费用无法解析。
        ModResources.Register();

        // 鬼域需要向框架注册一个「本回合第 N 张牌免费」的检测器。
        Powers.GuiYuPower.EnsureDetectorRegistered();

        // 把先古卡「墨染江山」注入原版先古之民（Neow / Darv）的初始选项。
        Ancients.WanJieRuLinAncientOptions.Register();

        Logger.Info("WanJieRuLin initialized.");
    }
}
