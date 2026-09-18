using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 电龙护法 —— 技能牌 1 能量：本回合获得 6 点格挡，下回合获得 3 点格挡。
/// 升级后本回合 9 点，下回合 6 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianLongHuFa : WanJieRuLinCardModel
{
    private const string NextTurnBlockVar = "NextTurnBlock";
    private const int BlockNow = 6;
    private const int BlockNextTurn = 3;

    public DianLongHuFa() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(BlockNow, ValueProp.Move),
        ModCardVars.Block(NextTurnBlockVar, BlockNextTurn)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);

        var nextTurn = DynamicVars.GetIntOrDefault(NextTurnBlockVar, BlockNextTurn);
        if (nextTurn > 0)
        {
            await ApplySelf<BlockNextTurnPower>(choiceContext, nextTurn);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars[NextTurnBlockVar].UpgradeValueBy(3m);
    }
}
