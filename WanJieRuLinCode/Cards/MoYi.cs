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
/// 墨移 —— 本回合你下一张牌费用 -1，抽 1 张牌。升级：-2 费，抽 1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoYi : WanJieRuLinCardModel
{
    public MoYi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CostReduction", IsUpgraded ? 2m : 1m),
        new DynamicVar("Draw", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本回合下一张牌费用 -N：挂一个一次性的减费能力。
        await PowerCmd.Apply<MoYiNextCardPower>(
            choiceContext, Owner!.Creature, IsUpgraded ? 2m : 1m, Owner.Creature, this);

        await Draw(choiceContext, 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CostReduction"].UpgradeValueBy(1m); // 1 → 2
    }
}
