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
/// 将鬼气变为 10 减去当前值，抽 1 张牌。升级后抽 2 张。
/// ★ 镜像不产净资源 —— 高鬼气变低、低鬼气变高，但总量守恒。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuanQi : WanJieRuLinCardModel
{
    public HuanQi() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Mirror", 10),
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SetGhostQi(Math.Max(0, DynamicVars.GetIntOrDefault("Mirror", 10) - MyGhostQi));
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
