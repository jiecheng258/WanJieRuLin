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
        typeof(Cards.YunBi),   // 运笔

        // ---- Common ----
        typeof(Cards.BiZhi),   // 笔直
        typeof(Cards.ChenMo),   // 沉墨
        typeof(Cards.DaMian),   // 大面
        typeof(Cards.DianPo),   // 点破
        typeof(Cards.DianRan),   // 点染
        typeof(Cards.GouXian),   // 勾线
        typeof(Cards.LuoDian),   // 落点
        typeof(Cards.MoMian),   // 磨面
        typeof(Cards.ShuBi),   // 数笔
        typeof(Cards.ShuXian),   // 竖线

        // ---- Uncommon ----
        typeof(Cards.BiFengYiZhuan),   // 笔锋一转
        typeof(Cards.ChanSi),   // 缠丝
        typeof(Cards.DianJing),   // 点睛
        typeof(Cards.DianShi),   // 点石
        typeof(Cards.DianYin),   // 点引
        typeof(Cards.JuanZhou),   // 卷轴
        typeof(Cards.KuangCao),   // 狂草
        typeof(Cards.LiDaoHuiLiu),   // 力道回流
        typeof(Cards.LiTouZhiBei),   // 力透纸背
        typeof(Cards.LuoMo),   // 落墨
        typeof(Cards.ManZhiYunYan),   // 满纸云烟
        typeof(Cards.MoYunTianChengV5),   // 墨韵天成
        typeof(Cards.ShuSan),   // 疏散
        typeof(Cards.WanHeQianYan),   // 万壑千岩
        typeof(Cards.YunJinChengFeng),   // 运斤成风
        typeof(Cards.ZongHeng),   // 纵横

        // ---- Rare ----
        typeof(Cards.BiLaoMoXiu),   // 笔老墨秀
        typeof(Cards.ChangJuan),   // 长卷
        typeof(Cards.DianJingZhiBi),   // 点睛之笔
        typeof(Cards.DianShiChengJin),   // 点石成金
        typeof(Cards.DianXianChengMian),   // 点线成面
        typeof(Cards.JingShuiLiuShen),   // 静水流深
        typeof(Cards.MoHai),   // 墨海
        typeof(Cards.MoShou),   // 墨守
        typeof(Cards.ShanGaoShuiChang),   // 山高水长
        typeof(Cards.ShuMiYouZhi),   // 疏密有致
        typeof(Cards.XingYunLiuShui),   // 行云流水
        typeof(Cards.XuZhi),   // 续纸
        typeof(Cards.ZhongFeng),   // 中锋

        // ---- Ancient ----
        typeof(Cards.WanJieRuLinAncient),   // 万界如林
        typeof(Cards.WuMingZhiShi),   // 无名之始

        // ---- Event ----
        typeof(Cards.Mi),   // 谜
        typeof(Cards.TaDeLaiChu),   // 她的来处
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
