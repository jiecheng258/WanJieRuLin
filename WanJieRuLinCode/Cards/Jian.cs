using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 剑！ —— 技能牌 2 能量：接下来 3 回合获得 5 把剑的效果。消耗。
/// 升级后接下来 4 回合获得。剑的效果见 <see cref="Swords"/>。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class Jian : WanJieRuLinCardModel
{
    private const int BaseTurns = 3;
    private const int SwordsPerTurn = 5;

    public Jian() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Turns", BaseTurns),
        ModCardVars.Int("SwordsPerTurn", SwordsPerTurn)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var turns = DynamicVars.GetIntOrDefault("Turns", BaseTurns);
        var perTurn = DynamicVars.GetIntOrDefault("SwordsPerTurn", SwordsPerTurn);

        if (turns <= 0 || perTurn <= 0)
        {
            return;
        }

        var power = await ApplySelfAndGet<SwordStormPower>(choiceContext, turns);
        if (power is not null)
        {
            power.SwordsPerTurn = perTurn;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Turns"].UpgradeValueBy(1);
    }
}
