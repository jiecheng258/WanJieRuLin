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
/// 能力 —— **纯点流**：每回合你打出的第一张「点」牌额外抽 1 张牌。
/// 升级 ★ 效果升级：改为抽 2 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class RunBi : WanJieRuLinCardModel
{
    public RunBi() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Draw", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<RunBiPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Amount = DynamicVars.GetIntOrDefault("Draw", 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1);    }
}
