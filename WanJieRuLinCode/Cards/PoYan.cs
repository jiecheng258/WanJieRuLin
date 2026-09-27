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
/// 失去所有鬼气，每 4 点获得 1 点能量；抽 1 张牌。[消耗]
/// ★ 反无限（R2）：原为 0 费 / 3 鬼气换 2 能量 + 1 牌 / 不消耗 → 无限元凶。
/// 升级质变：汇率 4:1→3:1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PoYan : WanJieRuLinCardModel
{
    public PoYan() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("QiPerEnergy", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var spent = await ClearGhostQi();
        var per = Math.Max(1, DynamicVars.GetIntOrDefault("QiPerEnergy", 4));
        var gained = spent / per;
        if (gained > 0)
        {
            await GainEnergy(gained);
        }
        await Draw(choiceContext, 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["QiPerEnergy"].UpgradeValueBy(-1);    }
}
