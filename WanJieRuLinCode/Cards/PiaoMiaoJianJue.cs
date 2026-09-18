using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 飘渺剑诀 —— 攻击牌 2 能量 2 鬼气：造成 6 点伤害，随机获得一把剑。
/// 消耗。升级后能量变为 1。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class PiaoMiaoJianJue : WanJieRuLinCardModel
{
    private const int BaseEnergyCost = 2;
    private const int GhostQiCost = 2;

    public PiaoMiaoJianJue() : base(BaseEnergyCost, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(2),
        new DamageVar(6m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player || cardPlay.Target is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        await Swords.GrantRandom(choiceContext, player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
