using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace WanJieRuLin.Characters;

public sealed class WanJieRuLinCardPool : TypeListCardPoolModel
{
    // 墨黑 + 幽紫的主色调，贴合"鬼墨"主题。
    private static readonly Material? PoolFrameTintMaterial =
        MaterialUtils.CreateReplaceHueShaderMaterial(0.36f, 0.27f, 0.52f);

    /// <summary>
    /// 本卡池中所有「技能牌」的类型。
    /// 供「贝雷帽」等需要随机取技能牌的效果使用。
    /// 新增技能牌时请同步登记到这里。
    ///
    /// 注意：这里只登记可以正常进入玩家牌组的技能牌。
    /// 「墨染江山」是先古卡、「鬼气森森」是达佛事件卡，
    /// 都不应出现在随机奖励里，因此不登记。
    /// </summary>
    public static readonly Type[] SkillCardTypes =
    [
        typeof(Cards.FangYu),            // 防御
        typeof(Cards.Hui),               // 绘
        typeof(Cards.FengBi),            // 封笔
        typeof(Cards.MoDian),            // 墨点
        typeof(Cards.GuangMingQieGe),    // 光明切割
        typeof(Cards.ShenShiDuoShi),     // 审时度势
        typeof(Cards.XiaoHuo),           // 消火
        typeof(Cards.DaMengYiChang),     // 大梦一场
        typeof(Cards.CengCengQinShi),    // 层层侵蚀
        typeof(Cards.WoRuoWeiGui),       // 我若为鬼
        typeof(Cards.WoRuoWeiShen),      // 我若为神
        typeof(Cards.ZanBiFengMang),     // 暂避锋芒
        typeof(Cards.YaZhiYuWang),       // 压制欲望
        typeof(Cards.ShenShengBaoZou),   // 神圣暴走
        typeof(Cards.DuZiShengJi),       // 独自升级
        typeof(Cards.DianLongHuFa),      // 电龙护法
        typeof(Cards.HeiAnBiZhang),      // 黑暗壁障
        typeof(Cards.AoJiao),            // 傲娇
        typeof(Cards.Jian),              // 剑！
        typeof(Cards.PingXingShiJie),    // 平行世界
        typeof(Cards.ShiTong),           // 尸瞳
        typeof(Cards.WoBuWanLe)          // 我不玩了
    ];

    /// <summary>随机挑一张技能牌的类型；卡池为空时返回 null。</summary>
    public static Type? PickRandomSkillType() =>
        SkillCardTypes.Length == 0
            ? null
            : SkillCardTypes[Random.Shared.Next(SkillCardTypes.Length)];

    public override string Title => "WanJieRuLin";
    public override string EnergyColorName => "WanJieRuLin";

    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/energy_text.png";

    public override Color DeckEntryCardColor => WanJieRuLinCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.36f, 0.20f, 0.52f);
    public override Material? PoolFrameMaterial => PoolFrameTintMaterial;

    public override bool IsColorless => false;
}
