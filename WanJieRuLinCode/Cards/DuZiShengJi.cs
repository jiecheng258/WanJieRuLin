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
/// 获得 3 点鬼气，抽 1 张牌。[消耗]
/// ★ 0 费产气必须 [消耗]（R6），否则是免费燃料。升级后鬼气 4 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DuZiShengJi : WanJieRuLinCardModel
{
    public DuZiShengJi() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiGain", 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(DynamicVars.GetIntOrDefault("GhostQiGain", 3));
        await Draw(choiceContext, 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GhostQiGain"].UpgradeValueBy(1);    }
}
