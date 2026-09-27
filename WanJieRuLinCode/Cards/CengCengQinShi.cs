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
/// 获得 2 点鬼气，抽 1 张牌。升级后 ★ 双升级：鬼气 3 点且抽 2 张。
/// ★ 反无限（R6）：原为 **0 费** 且会返还鬼气 = 免费燃料，已改 1 费。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class CengCengQinShi : WanJieRuLinCardModel
{
    public CengCengQinShi() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("GhostQiGain", 2),
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(DynamicVars.GetIntOrDefault("GhostQiGain", 2));
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GhostQiGain"].UpgradeValueBy(1);        DynamicVars.Cards.UpgradeValueBy(1);    }
}
