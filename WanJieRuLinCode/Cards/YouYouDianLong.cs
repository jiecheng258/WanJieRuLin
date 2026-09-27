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
/// 鬼气不低于 10 时才能打出。消耗所有鬼气，每 3 点造成 8 点伤害，共 2 次。
/// 升级质变：每 2 点即可换一次。★ H 流硬条件收尾牌。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YouYouDianLong : WanJieRuLinCardModel
{
    public YouYouDianLong() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        ModCardVars.Repeat(2),
        ModCardVars.Int("QiPerHit", 3)
    ];

    protected override bool? PlayCondition => GhostQiAtLeast(10);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var spent = await ClearGhostQi();
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerHit", 3));
        var hits = Math.Max(1, spent / per) * DynamicVars.Repeat.IntValue;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(hits)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerHit"].UpgradeValueBy(-1);    }
}
