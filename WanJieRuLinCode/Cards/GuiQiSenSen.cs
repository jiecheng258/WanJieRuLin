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
/// 鬼气森森 —— 达佛（Darv）给予的能力卡。
/// 3 点能量：你接下来的回合不再获得能量，每回合获得的能量转为鬼气，
/// 所有能量牌消耗变为 0 能量并获得消耗。
/// 升级后能量消耗为 2，到手时默认已是升级状态。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class GuiQiSenSen : WanJieRuLinCardModel
{
    public GuiQiSenSen() : base(3, CardType.Power, CardRarity.Event, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("EnergyToGhostQi", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<EnergyToGhostQiPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
