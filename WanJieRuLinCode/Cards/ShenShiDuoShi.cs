using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 审时度势 —— 技能牌 1 能量：耗费所有鬼气，每 3 点鬼气获得 1 点能量。[消耗]
///
/// ★ 反无限（R1 / R3 / R7）—— 这张牌原本是**无限的第一元凶**：
///   旧写法「0 费；X 鬼气 → X 能量 **+ 1 张牌**，且不消耗」。
///   1 鬼气净赚 1 能量**外加手牌不减**，必然自我循环；
///   再配一张产鬼气的牌（例如当时 0 费的「层层侵蚀」）就是无限。
///
///   四处同时收紧，缺一不可：
///     1. 0 费 → **1 费**（R1：0 费不该产能量）
///     2. 汇率 1:1 → **3:1**
///     3. **去掉抽牌**（R3 / R7：同时给能量与手牌就是净赚）
///     4. 加 **[消耗]**（R2：兑换器必须一次性，否则会被反复循环利用）
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class ShenShiDuoShi : WanJieRuLinCardModel
{
    /// <summary>每多少点鬼气换 1 点能量。</summary>
    private const int QiPerEnergy = 3;

    public ShenShiDuoShi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerEnergy", QiPerEnergy)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = GhostQiXValue(cardPlay);
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerEnergy", QiPerEnergy));
        var gained = spent / per;

        if (gained > 0)
        {
            await GainEnergy(gained);
        }
    }

    protected override void OnUpgrade()
    {
        // ★ 质变：汇率从 3:1 提到 2:1，不只是加数值。
        DynamicVars["QiPerEnergy"].UpgradeValueBy(-1);
    }
}
