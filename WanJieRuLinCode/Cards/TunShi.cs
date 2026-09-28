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
/// 每回合最多触发 2 次：获得鬼气时，抽 1 张牌。
/// ★ 反无限（R5）：原为「每获得 1 点鬼气就抽 1 张」，无上限 → 最直接的无限引擎。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class TunShi : WanJieRuLinCardModel
{
    public TunShi() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<TunShiPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);    }
}
