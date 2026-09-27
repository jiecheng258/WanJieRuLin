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
/// 你接下来获得的能量全部转为鬼气（永久）。每回合开始时额外获得 2 点鬼气。
/// 升级后额外 3 点。
/// ★ 补充：光把能量换成鬼气是等价交换，没有净收益，所以配一份额外产出。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LiGuiFuSu : WanJieRuLinCardModel
{
    public LiGuiFuSu() : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiPerTurn", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<EnergyToGhostQiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Configure(EnergyToGhostQiPower.Permanent);
            power.GhostQiPerTurn = DynamicVars.GetIntOrDefault("GhostQiPerTurn", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GhostQiPerTurn"].UpgradeValueBy(1);    }
}
