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
        // ---- Basic ----
        typeof(Cards.FangYu),   // 防御
        typeof(Cards.ZuoSui),   // 作祟

        // ---- Common ----
        typeof(Cards.MoDun),    // 墨盾
        typeof(Cards.JuQi),     // 聚气
        typeof(Cards.HuiMo),    // 回墨
        typeof(Cards.ChengXi),  // 澄息
        typeof(Cards.YinQi),    // 引气
        typeof(Cards.GuiBi),    // 鬼壁
        typeof(Cards.MoYun),    // 墨涌
        typeof(Cards.MoYi),     // 墨移

        // ---- Uncommon ----
        typeof(Cards.YinBi),    // 隐壁
        typeof(Cards.MoQi),     // 墨气
        typeof(Cards.HuiHun),   // 回魂
        typeof(Cards.NingMo),   // 凝墨
        typeof(Cards.TuoYing),  // 脱影
        typeof(Cards.AnXiang),  // 暗香
        typeof(Cards.MoHua),    // 墨花
        typeof(Cards.XunFeng),  // 巡风
        typeof(Cards.MingJing), // 明镜
        typeof(Cards.JieYin),   // 借阴
        typeof(Cards.GuiZhao),  // 鬼爪
        typeof(Cards.MoWu),     // 墨舞
        typeof(Cards.HuiMoLian),// 回墨链

        // ---- Rare ----
        typeof(Cards.WanJieBi), // 万界壁
        typeof(Cards.XuShen),   // 虚神
        typeof(Cards.MoYuan),   // 墨渊
        typeof(Cards.LingXi),   // 灵犀
        typeof(Cards.HuiTian),  // 回天
        typeof(Cards.MoWuJue),  // 墨舞·绝
        typeof(Cards.WanNian),  // 万年
        typeof(Cards.LingXu),   // 灵虚
        typeof(Cards.MoSheng),  // 墨生
        typeof(Cards.ZuoSuiYin),// 作祟引
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
