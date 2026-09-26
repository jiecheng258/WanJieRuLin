using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 层层侵蚀 —— 技能牌 1 能量：获得 2 点鬼气，抽 1 张牌。
///
/// ★ 反无限（R6）—— 旧写法是 **0 费**、且打出后若鬼气归零会「返还 2 点鬼气」，
///   等于**免费产鬼气**。0 费产气是最典型的无限燃料：它本身不花任何代价，
///   于是任何「鬼气 → 资源」的兑换器都能被它无限喂饱。
///
///   收紧：**0 费 → 1 费**。产气必须花能量，燃料就不免费了。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class CengCengQinShi : WanJieRuLinCardModel
{
    private const int QiGain = 2;

    public CengCengQinShi() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiGainVarOf(QiGain),
        ModCardVars.Cards(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainGhostQi(DynamicVars.GetIntOrDefault("GhostQiGain", QiGain));
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：双升级 —— 鬼气 +1 **且** 抽牌 +1。
        DynamicVars["GhostQiGain"].UpgradeValueBy(1);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
