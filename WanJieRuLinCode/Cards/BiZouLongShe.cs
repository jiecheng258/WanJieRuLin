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
/// 能力 —— **纯点流**：你每打出一张「点」牌，额外获得 1 点能量（点牌本身已自带 +1）。
/// 拥有后每张点牌共回 2 点能量。
/// 升级 ★ 效果升级：额外回 2 点（共 3）。
/// 
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class BiZouLongShe : WanJieRuLinCardModel
{
    public BiZouLongShe() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("EnergyGain", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<BiZouLongShePower>(choiceContext, 1m);
        if (power is not null)
        {
            power.EnergyGain = DynamicVars.GetIntOrDefault("EnergyGain", 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnergyGain"].UpgradeValueBy(1);    }
}
