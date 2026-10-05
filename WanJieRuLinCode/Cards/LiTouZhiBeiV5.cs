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
/// 能力 —— **纯线流**：每回合开始时获得 2 点临时力量。
/// 升级 ★ 效果升级：3 点。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class LiTouZhiBeiV5 : WanJieRuLinCardModel
{
    public LiTouZhiBeiV5() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("TempStr", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await ApplySelfAndGet<LiTouPower>(choiceContext, 1m);
        if (power is not null)
        {
            power.Force = DynamicVars.GetIntOrDefault("TempStr", 2);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TempStr"].UpgradeValueBy(1);    }
}
