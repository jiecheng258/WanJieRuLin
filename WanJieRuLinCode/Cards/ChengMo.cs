using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 澄墨 —— 技能牌 1 能量：失去所有鬼气，每 4 点抽 1 张牌。[消耗]
///
/// ★ 反无限（R3）—— 旧写法是**无限元凶之三**：
///   「每 2 点鬼气 → 1 点能量 **+ 1 张牌**，且不消耗」。
///   一次兑换同时给两种资源，手牌只增不减 → 配产气牌即循环。
///
///   收紧两处：
///     1. **只给手牌，不再给能量**（R3：鬼气换牌的牌不该同时产能量）
///     2. 加 **[消耗]**，兑换率 2:1 → **4:1**
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChengMo : WanJieRuLinCardModel
{
    /// <summary>每多少点鬼气抽 1 张牌。</summary>
    private const int QiPerCard = 4;

    public ChengMo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerCard", QiPerCard)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = await ClearGhostQi();
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerCard", QiPerCard));
        var times = spent / per;

        if (times > 0)
        {
            await Draw(choiceContext, times);
        }
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：汇率从 4:1 提到 3:1。
        DynamicVars["QiPerCard"].UpgradeValueBy(-1);
    }
}
