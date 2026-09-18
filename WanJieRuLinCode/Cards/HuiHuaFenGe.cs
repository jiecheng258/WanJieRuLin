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
/// 绘画分割 —— 攻击牌 3 能量：造成 28 点伤害。如果对方死亡，则获得 2 点鬼气。
/// 升级后造成 36 点伤害。
///
/// 数值说明（对齐原版）：原版 3 费单体伤害的顶格是 Bludgeon 32（罕见）
/// 与 KnockoutBlow 30（罕见），两者都没有附加收益。
/// 本卡额外有「击杀返鬼气」，而 1 点鬼气在本模组里 ≈ 1 点能量
/// （「审时度势」1 鬼气换 1 能量），所以原来 35 伤 + 返 4 鬼气
/// 相当于 3 费打出 35 伤再倒贴 4 费，明显超模。
/// 现改为 28 伤 + 返 2 鬼气（≈2 能量），仍略强于原版但符合「罕见」定位。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HuiHuaFenGe : WanJieRuLinCardModel
{
    private const int GhostQiOnKill = 2;
    private const int BaseDamage = 28;

    public HuiHuaFenGe() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        GhostQiGainVarOf(GhostQiOnKill)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var target = cardPlay.Target;
        var wasAlive = target.IsAlive;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .Execute(choiceContext);

        if (wasAlive && !target.IsAlive)
        {
            await GainGhostQi(GhostQiOnKill);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
