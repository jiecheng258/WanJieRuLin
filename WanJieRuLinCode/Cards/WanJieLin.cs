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
/// 万界林 —— 你的作祟三档全强化：点档 +2 能量、线档 +2 力 +2 敏、面档复制 2 张。
/// 升级：各再 +1，且作祟不再 [消耗]。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class WanJieLin : WanJieRuLinCardModel
{
    public WanJieLin() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("PointEnergy", 2m),
        new DynamicVar("LineStat", 2m),
        new DynamicVar("FaceCopy", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<WanJieLinPower>(choiceContext, 1m);
        if (power is { } p)
        {
            p.IsUpgraded = IsUpgraded;
            p.ZuoSuiNoExhaust = IsUpgraded;
        }
    }

    protected override void OnUpgrade()
    {
        // 升级 = 各档再 +1，且作祟去消耗
    }
}
