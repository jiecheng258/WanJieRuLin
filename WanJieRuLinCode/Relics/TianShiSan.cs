using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Relics;

/// <summary>
/// 天师伞（特殊遗物）—— 消耗 4 点鬼气获得 1 点能量；消耗 4 点能量时获得 1 点鬼气。
/// 双向兑换：把鬼气经济与能量经济打通。
/// </summary>
[RegisterRelic(typeof(WanJieRuLinRelicPool))]
public sealed class TianShiSan : WanJieRuLinRelic, ISecondaryResourceHookListener
{
    /// <summary>兑换阈值（鬼气 ↔ 能量）。</summary>
    public const int ExchangeThreshold = 4;

    /// <summary>已累计但尚未兑换的鬼气消耗。</summary>
    private int _ghostQiSpent;

    /// <summary>已累计但尚未兑换的能量消耗。</summary>
    private int _energySpent;

    public override RelicRarity Rarity => RelicRarity.Rare;

    /// <summary>累计鬼气消耗，每满 4 点换 1 点能量。</summary>
    public Task AfterSecondaryResourceSpent(SecondaryResourceSpendContext context)
    {
        if (context.Definition.Id != ModResources.GhostQiId ||
            Owner is not { } player ||
            context.Amount <= 0)
        {
            return Task.CompletedTask;
        }

        _ghostQiSpent += context.Amount;

        while (_ghostQiSpent >= ExchangeThreshold)
        {
            _ghostQiSpent -= ExchangeThreshold;
            Flash();
            _ = PlayerCmd.GainEnergy(1, player);
        }

        return Task.CompletedTask;
    }

    /// <summary>累计能量消耗，每满 4 点换 1 点鬼气。</summary>
    public override Task AfterEnergySpent(CardModel card, int amount)
    {
        if (Owner is not { } player || amount <= 0)
        {
            return Task.CompletedTask;
        }

        _energySpent += amount;

        while (_energySpent >= ExchangeThreshold)
        {
            _energySpent -= ExchangeThreshold;
            Flash();
            _ = GhostQi.Gain(player, 1);
        }

        return Task.CompletedTask;
    }
}
