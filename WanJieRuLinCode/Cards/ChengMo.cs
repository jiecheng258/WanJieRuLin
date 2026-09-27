using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 失去所有鬼气，每 4 点抽 1 张牌。[消耗]
/// ★ 反无限（R3）：原为「2 鬼气换 1 能量 **+1 张牌**」且不消耗 → 无限元凶。
/// 升级质变：汇率 4:1→3:1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ChengMo : WanJieRuLinCardModel
{
    public ChengMo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerCard", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = await ClearGhostQi();
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerCard", 4));
        var times = spent / per;
        if (times > 0)
        {
            await Draw(choiceContext, times);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerCard"].UpgradeValueBy(-1);    }
}
