using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 耗费所有鬼气，每 3 点换 1 点能量。[消耗]
/// ★ 反无限元凶之一：原为 0 费 / 1:1 / 还抽牌 / 不消耗。升级质变：汇率 3:1→2:1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShenShiDuoShi : WanJieRuLinCardModel
{
    public ShenShiDuoShi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerEnergy", 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = GhostQiXValue(cardPlay);
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerEnergy", 3));
        var gained = spent / per;
        if (gained > 0)
        {
            await GainEnergy(gained);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerEnergy"].UpgradeValueBy(-1);    }
}
