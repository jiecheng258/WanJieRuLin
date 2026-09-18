using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 傲娇 —— 技能牌 1 能量：抽 3 张牌，获得这 3 张牌所造成伤害的格挡。消耗。
/// 升级后抽 4 张牌，获得 4 张牌所造成伤害的格挡。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class AoJiao : WanJieRuLinCardModel
{
    private const int BaseDraw = 3;

    public AoJiao() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(BaseDraw)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player)
        {
            return;
        }

        var count = DynamicVars.Cards.IntValue;
        var drawn = (await CardPileCmd.Draw(choiceContext, count, player)).ToList();

        // 把这批抽到的牌的面板伤害累加为格挡。
        var total = 0m;
        foreach (var card in drawn)
        {
            if (card.Type != CardType.Attack)
            {
                continue;
            }

            if (card.DynamicVars.TryGetValue("Damage", out var dmg))
            {
                total += dmg.BaseValue;
            }
        }

        if (total > 0m)
        {
            await GainBlock(choiceContext, total);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
