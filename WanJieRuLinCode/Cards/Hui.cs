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
/// 耗费 1 点鬼气。获得 4 点格挡，抽 1 张牌。
/// ★ 原为「0 费 / X 鬼气」—— 起手牌不该一次清空全部鬼气，改成固定 1 点。升级 ★ 质变：抽牌 1→2。
/// </summary>
[RegisterCharacterStarterCard(typeof(WanJieRuLinCharacter), 2)]
[RegisterArchaicToothTranscendence(typeof(MoRanJiangShan))]
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class Hui : WanJieRuLinCardModel
{
    public Hui() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        SetGhostQiCost(1);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiCost", 1),
        new BlockVar(4m, ValueProp.Move),
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
