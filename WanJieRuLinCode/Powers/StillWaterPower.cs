using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 静水 —— 能力：**本回合**内你无法获得鬼气。
///
/// 用法是「锁定相位」：把鬼气冻在当前值，于是「墨浓」的伤害惩罚或「墨淡」的
/// 额外抽牌在本回合内不会因为遗物/牌序而漂移，
/// 配合「鬼气不高于 N 才能打出」的门槛牌可以先冻住再开门。
///
/// ★ 继承 <see cref="WanJieTurnScopedPower"/>：文案写「本回合」就必须真的在
///   自己这一方回合结束时撤掉，否则会变成整场战斗永久封锁鬼气增长。
///
/// 只挡「获得」，不挡「消耗/失去」—— 支付费用与「失去所有鬼气」类效果照常生效，
/// 否则玩家会被自己的牌锁死。
///
/// ⚠️ <see cref="ISecondaryResourceHookListener.ModifySecondaryResourceGain"/> 是
///    **接口成员**（不是 virtual），所以只能隐式实现、**不能写 override**
///    （写了会报 CS0115）。
/// </summary>
[RegisterPower]
public sealed class StillWaterPower : WanJieTurnScopedPower, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>把本回合的鬼气「获得」全部吞掉（返回 0）。</summary>
    public decimal ModifySecondaryResourceGain(
        SecondaryResourceContext context, decimal amount)
    {
        if (context.Definition.Id != ModResources.GhostQiId || amount <= 0m)
        {
            return amount;
        }

        Flash();
        return 0m;
    }
}
