using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using WanJieRuLin.Cards;

namespace WanJieRuLin.Powers;

/// <summary>
/// 作祟 —— ★ 角色核心引擎能力。
///
/// 由「作祟」牌打出时施加，记录「接下来 X 张牌」的计数。
/// 每当玩家打出 1 张牌（作祟自己之外的任何牌），按该牌的能量费判定档位并注入效果：
///
///   - 0–1 费（点档）：打出时 +1 能量
///   - 2 费（线档）：打出时 +1 力量 +1 敏捷，抽 2 张
///   - 3+ 费（面档）：打出时把这张牌的一张 0 费复制（附 [消耗]）放入弃牌堆
///
/// 计数耗尽后自删。这是一个「常驻计数」能力：它不随回合结束消失，
/// 而是靠消耗张数走完 —— 所以继承 <see cref="ModPowerTemplate"/>，
/// 不继承 <see cref="WanJieTurnScopedPower"/>（后者会在回合末自删）。
/// </summary>
[RegisterPower]
public sealed class ZuoSuiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>档位效果数值（点档 +1 能量 / 线档 +1 力 +1 敏 / 抽 2）。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("TierEnergy", 1m),
        new DynamicVar("TierStat", 1m),
        new DynamicVar("TierDraw", 2m),
    ];

    /// <summary>剩余待强化的张数。施加时 = X（升级后 X+1）。</summary>
    private int _remaining;

    /// <summary>被打出的这张牌，其能量费落在哪个档位。</summary>
    private static int TierOf(CardModel card)
    {
        var cost = card.EnergyCost.Canonical;
        if (cost <= 1)
        {
            return 0; // 点档：0–1 费
        }

        return cost == 2 ? 1 : 2; // 线档：2 费；面档：3+ 费
    }

    /// <summary>被施加时设置剩余张数（= X）。</summary>
    public void Configure(int count)
    {
        _remaining = Math.Max(0, count);
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Player is not { } player || _remaining <= 0)
        {
            return;
        }

        // 作祟牌自己不打进计数（引擎不是被强化的对象）。
        if (cardPlay.Card is ZuoSui)
        {
            return;
        }

        _remaining--;

        var creature = player.Creature;
        var tier = TierOf(cardPlay.Card);

        // 读取三档强化能力（存在即为生效；数值是设计定稿的固定增量）。
        bool dianZeng = creature.GetPowerAmount<DianZengPower>() > 0;   // 点增：点档 +1 能量（升级抽 1）
        bool xianZeng = creature.GetPowerAmount<XianZengPower>() > 0;   // 线增：线档 +2 格挡（升级 +1 力量）
        bool mianZeng = creature.GetPowerAmount<MianZengPower>() > 0;   // 面增：面档复制 2 张
        bool wanJieLin = creature.GetPowerAmount<WanJieLinPower>() > 0; // 万界林：三档全强化

        switch (tier)
        {
            case 0: // 点档：+1 能量（点增 +1、万界林再 +1）
            {
                Flash();
                decimal energy = 1m
                    + (dianZeng ? 1m : 0m)
                    + (wanJieLin ? 1m : 0m);
                await PlayerCmd.GainEnergy(energy, player);

                // 点增（升级）额外抽 1 张。
                if (dianZeng && TryGetUpgraded<DianZengPower>(creature))
                {
                    await CardPileCmd.Draw(choiceContext, 1, player);
                }

                break;
            }

            case 1: // 线档：+1 力量 +1 敏捷，抽 2 张（线增 +2 格挡；万界林 +1 力 +1 敏）
            {
                Flash();
                decimal strength = 1m + (wanJieLin ? 1m : 0m);
                decimal dexterity = 1m + (wanJieLin ? 1m : 0m);

                await PowerCmd.Apply<WanJieTempStrengthPower>(
                    choiceContext, creature, strength, creature, null);
                await PowerCmd.Apply<WanJieTempDexterityPower>(
                    choiceContext, creature, dexterity, creature, null);

                // 线增额外 +2 格挡（升级 +4）。
                if (xianZeng)
                {
                    decimal block = TryGetUpgraded<XianZengPower>(creature) ? 4m : 2m;
                    await CreatureCmd.GainBlock(creature, block, ValueProp.Move, null);

                    // 线增（升级）额外 +1 力量。
                    if (TryGetUpgraded<XianZengPower>(creature))
                    {
                        await PowerCmd.Apply<WanJieTempStrengthPower>(
                            choiceContext, creature, 1m, creature, null);
                    }
                }

                await CardPileCmd.Draw(choiceContext, 2, player);
                break;
            }

            case 2: // 面档：一张 0 费复制（附 [消耗]）放入弃牌堆（面增/万界林复制更多）
            {
                Flash();
                int copies = 1;
                if (mianZeng)
                {
                    copies = TryGetUpgraded<MianZengPower>(creature) ? 3 : 2;
                }
                else if (wanJieLin)
                {
                    copies = 2;
                }

                for (var i = 0; i < copies; i++)
                {
                    await SpawnZeroCostCopy(choiceContext, cardPlay.Card, player);
                }

                break;
            }
        }

        if (_remaining <= 0)
        {
            await PowerCmd.Remove(this);
        }
    }

    /// <summary>读取指定能力是否处于「升级后」状态（由卡牌 OnPlay 时设置 IsUpgraded）。</summary>
    private static bool TryGetUpgraded<TPower>(Creature creature)
        where TPower : ModPowerTemplate, IUpgradablePower
    {
        return CharacterCombatExtensions.FindPower<TPower>(creature)?.IsUpgraded == true;
    }

    /// <summary>
    /// 面档：把「这张牌」的一张 0 费复制（附 [消耗]）放进弃牌堆。
    /// 复用卡牌基类的 <see cref="WanJieRuLinCardModel.ReturnFaceZeroCostCopy"/> 造牌姿势，
    /// 但这里由能力触发，所以把同一套 CombatState.CreateCard 逻辑内联实现。
    /// </summary>
    private static async Task SpawnZeroCostCopy(
        PlayerChoiceContext choiceContext, CardModel source, Player player)
    {
        if (source.CombatState is not MegaCrit.Sts2.Core.Combat.CombatState combat)
        {
            return;
        }

        try
        {
            var created = combat.CreateCard(source, player);
            created.EnergyCost.SetCustomBaseCost(0);
            created.AddKeyword(CardKeyword.Exhaust);

            await CardPileCmd.AddGeneratedCardToCombat(
                created, PileType.Discard, player, CardPilePosition.Random);
        }
        catch (Exception)
        {
            // 造牌失败不应连累其余档位效果。
        }
    }
}
