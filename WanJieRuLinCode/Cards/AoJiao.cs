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
/// 抽 3 张牌，获得这些牌面板伤害合计的格挡。[消耗]
/// ★ 恢复原设计（重写时曾误简化成「只抽 1 张」）。升级 ★ 质变：抽 3→4 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class AoJiao : WanJieRuLinCardModel
{
    public AoJiao() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(3)
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
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
