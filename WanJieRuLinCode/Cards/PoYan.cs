using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 破砚 —— 技能牌 0 能量：失去所有鬼气，每 4 点获得 1 点能量；抽 1 张牌。[消耗]
///
/// ★ 反无限（R2 / R3）—— 旧写法是**无限元凶之一**：
///   「0 费；每 3 点鬼气 → 2 点能量 **+ 1 张牌**，且不消耗」。
///   兑换率赚（1.5 鬼气换 1 能量）**还白送手牌**，且不消耗 → 可被反复循环。
///
///   收紧三处：
///     1. 汇率 3:2 → **4:1**（R2）
///     2. 加 **[消耗]**（R2：兑换器必须一次性）
///     3. 抽牌固定为 1（不再随兑换次数放大）
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PoYan : WanJieRuLinCardModel
{
    /// <summary>每多少点鬼气换 1 点能量。</summary>
    private const int QiPerEnergy = 4;

    public PoYan() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerEnergy", QiPerEnergy)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = await ClearGhostQi();
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerEnergy", QiPerEnergy));
        var gained = spent / per;

        if (gained > 0)
        {
            await GainEnergy(gained);
        }

        await Draw(choiceContext, 1);
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：汇率从 4:1 提到 3:1。
        DynamicVars["QiPerEnergy"].UpgradeValueBy(-1);
    }
}
