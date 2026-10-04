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
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// ★ **先古牌** —— 每 3 点[gold]墨韵[/gold]，获得 1 点能量并抽 1 张牌。[消耗]
/// [gold]保留[/gold]。★ 不直接造成伤害 —— 它把墨韵变成你继续运转的燃料。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WanJieRuLinAncient : WanJieRuLinCardModel
{
    public WanJieRuLinAncient() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("PerMoYun", 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var mo = MyMoYun;
        if (mo <= 0)
        {
            return;
        }
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("PerMoYun", 3));
        var n = mo / per;
        if (n > 0)
        {
            await GainEnergy(n);
            await Draw(choiceContext, n);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
