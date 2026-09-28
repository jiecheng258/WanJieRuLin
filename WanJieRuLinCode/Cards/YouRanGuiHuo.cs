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
/// 给予 2 层易伤。造成 5 点伤害，共 2 次；鬼气每有 5 点，次数 +1。
/// 升级质变：每 4 点即可 +1 次。★ 梯度过渡牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YouRanGuiHuo : WanJieRuLinCardModel
{
    public YouRanGuiHuo() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        ModCardVars.Repeat(2),
        ModCardVars.Int("Vulnerable", 2),
        ModCardVars.Int("QiPerExtraHit", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await ApplyTo<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars.GetIntOrDefault("Vulnerable", 2));
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerExtraHit", 5));
        var hits = DynamicVars.Repeat.IntValue + MyGhostQi / per;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(hits)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerExtraHit"].UpgradeValueBy(-1);    }
}
