using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 掠夺 —— 攻击牌 1 能量：造成 6 点伤害，获得 1 点鬼气。升级后造成 9 点伤害，获得 2 点鬼气。
/// 打人回鬼气，是最朴素的鬼气来源。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LueDuo : WanJieRuLinCardModel
{
    public LueDuo() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        GhostQiGainVarOf(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await GainGhostQi(DynamicVars.GetIntOrDefault(GhostQiGainVar, 1));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars[GhostQiGainVar].UpgradeValueBy(1);
    }
}
