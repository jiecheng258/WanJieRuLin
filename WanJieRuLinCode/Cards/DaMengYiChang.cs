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
/// ★ **血墨**：对自己造成 2 点伤害，获得 14 点格挡；下回合抽 1 张牌。
/// 升级后 18 点格挡（代价不变）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DaMengYiChang : WanJieRuLinCardModel
{
    public DaMengYiChang() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(14m, ValueProp.Move),
        ModCardVars.Cards(1),
        ModCardVars.Int("BloodCost", 2)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is { } self)
        {
            await CreatureCmd.Damage(choiceContext, self.Creature,
                DynamicVars.GetIntOrDefault("BloodCost", 2), ValueProp.Move, self.Creature, null, null);
        }
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
        await ApplySelf<DrawCardsNextTurnPower>(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);    }
}
