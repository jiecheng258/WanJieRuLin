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
/// 墨盾 —— 获得 6 格挡。若本回合已打作祟，+4 格挡。升级：8 格挡 / 条件 +6。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class MoDun : WanJieRuLinCardModel
{
    public MoDun() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new BlockVar("Bonus", 4m, ValueProp.Move)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = DynamicVars.Block.BaseValue;

        if (ZuoSuiPlayedThisTurn)
        {
            block += DynamicVars["Bonus"].BaseValue;
        }

        await GainBlock(choiceContext, block);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);   // 6 → 8
        DynamicVars["Bonus"].UpgradeValueBy(2m); // 4 → 6
    }
}
