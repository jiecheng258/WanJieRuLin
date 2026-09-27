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
/// 鬼气 ≥6：失去 4 点鬼气并抽 2 张；否则获得 4 点鬼气并抽 1 张。
/// ★ 失去侧**不给能量**（R3），不构成往返循环。升级质变：两侧抽牌同时 +1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class NongDanXiangSheng : WanJieRuLinCardModel
{
    public NongDanXiangSheng() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(2),
        ModCardVars.Int("QiSwing", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (MyGhostQi >= 6)
        {
            await LoseGhostQi(DynamicVars.GetIntOrDefault("QiSwing", 4));
            await Draw(choiceContext, DynamicVars.Cards.IntValue);
        }
        else
        {
            await GainGhostQi(DynamicVars.GetIntOrDefault("QiSwing", 4));
            await Draw(choiceContext, Math.Max(1, DynamicVars.Cards.IntValue - 1));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
