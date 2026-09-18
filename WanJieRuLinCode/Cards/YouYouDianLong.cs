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
/// 悠悠电龙 —— 攻击牌 7 鬼气：给予 1 层易伤，造成 6 点伤害，重新打出你上回合的所有牌。
/// 升级后给予 2 层易伤。
///
/// 数值说明（对齐原版）：这是本模组最强的单卡效果。
/// 「重放上回合所有牌」约等于白送一整个回合，而原版最接近的「回响形态」
/// （3 费，稀有）也只是每回合把**第一张**牌多打一次。
/// 原来只要 3 鬼气（≈3 能量）且是「罕见」，能稳定拆掉原版所有后期构筑。
/// 现改为 7 鬼气 + 稀有 —— 需要攒 7 点鬼气（软上限 20，回合供给 1-2）才打得出来，
/// 定位为「需要铺垫的终局技」。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YouYouDianLong : WanJieRuLinCardModel
{
    private const int GhostQiCost = 7;
    private const int BaseVulnerable = 1;

    public YouYouDianLong() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(GhostQiCost),
        new DamageVar(6m, ValueProp.Move),
        ModCardVars.Power<VulnerablePower>(BaseVulnerable)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is not { } player || cardPlay.Target is not { } target)
        {
            return;
        }

        await ApplyTo<VulnerablePower>(
            choiceContext, target,
            DynamicVars.GetIntOrDefault(nameof(VulnerablePower), BaseVulnerable));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        // 上回合记录的牌由常驻的追踪能力提供；没有记录时不重放。
        var tracker = player.Creature.Powers.OfType<LastTurnCardsPower>().FirstOrDefault();
        if (tracker is null)
        {
            return;
        }

        foreach (var card in tracker.LastTurnCards.ToList())
        {
            if (card is null || !card.CanPlay())
            {
                continue;
            }

            await CardCmd.AutoPlay(choiceContext, card, null, AutoPlayType.Default, true, true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(VulnerablePower)].UpgradeValueBy(1);
    }
}
