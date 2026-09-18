using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 独自升级 —— 技能牌 0 能量：下回合随机获得 2 点能量或 2 点鬼气。消耗。
/// 升级后随机获得 3 点能量或者 3 点鬼气。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DuZiShengJi : WanJieRuLinCardModel
{
    private const int BaseAmount = 2;

    public DuZiShengJi() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Amount", BaseAmount)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<SoloLevelUpPower>(
            choiceContext, DynamicVars.GetIntOrDefault("Amount", BaseAmount));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(1);
    }
}
