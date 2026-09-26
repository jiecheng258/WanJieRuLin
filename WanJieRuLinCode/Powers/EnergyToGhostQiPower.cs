using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace WanJieRuLin.Powers;

/// <summary>
/// 能量转鬼气 —— 能力：你获得的**能量**不再入手，改为等量的[鬼气]。
///
/// 参照原版 <c>NoEnergyGainPower</c> 的实现方式：
/// <c>ModifyEnergyGain</c> 把增量吞掉并记账，<c>AfterModifyingEnergyGain</c> 再把账折算成鬼气。
///
/// ==================================================================
/// ★ 时长是可配的（2026-09-26 修正）
///
/// 三张牌共用本能力，但它们文案要求的时长**各不相同**：
///
/// | 卡 | 文案 | 时长 |
/// |---|---|---|
/// | 我若为鬼（1能 / Uncommon / 消耗） | 「**下回合**获得的能量全部转为鬼气」 | **1 回合** |
/// | 厉鬼复苏（3能 / Rare） | 「**每回合**获得的能量全部转为鬼气」 | **永久** |
/// | 鬼气森森（3能 / Event） | 「你**接下来的回合**不再获得能量」 | **永久** |
///
/// 早先的实现把「永久」写死（<c>_armed</c> 一旦置位永不复位），
/// 于是「我若为鬼」实际是**整场战斗再也拿不到能量** —— 与文案不符，是 bug。
/// 但后来一刀切改成「只作用 1 回合」，又把另外两张（文案写「每回合」）**误伤成废牌**。
///
/// 正解就是本类的做法：**时长由卡牌在施加时通过 <see cref="Configure"/> 指定**
/// （<see cref="Permanent"/> = 永久，正数 = 多少回合）。
/// ==================================================================
/// </summary>
[RegisterPower]
public sealed class EnergyToGhostQiPower : ModPowerTemplate
{
    /// <summary>永久生效（不清除）。</summary>
    public const int Permanent = -1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>本轮被拦截、待折算成鬼气的能量。</summary>
    private int _pending;

    /// <summary>本轮是否处于拦截窗口内。</summary>
    private bool _armed;

    /// <summary>还剩多少回合，<see cref="Permanent"/> 表示永久。默认永久。</summary>
    private int _turnsRemaining = Permanent;

    /// <summary>
    /// 每回合开始时额外获得的鬼气。0 = 无。
    /// 供「厉鬼复苏」使用：光把能量换成鬼气是等价交换（1 鬼气 ≈ 1 能量），
    /// 没有净收益，所以要给一份额外产出，否则 3 费买了个纯限制。
    /// </summary>
    public int GhostQiPerTurn { get; set; }

    /// <summary>
    /// 每回合开始时额外抽的牌数。0 = 无。供「鬼气森森」使用。
    /// </summary>
    public int DrawPerTurn { get; set; }

    /// <summary>
    /// 由卡牌在施加后调用，指定生效时长。
    /// </summary>
    /// <param name="turns"><see cref="Permanent"/> 或正整数（生效的回合数）。</param>
    public void Configure(int turns) => _turnsRemaining = Math.Max(Permanent, turns);

    /// <summary>
    /// 回合开始时打开拦截窗口，并结算额外产出。
    ///
    /// 注意：卡牌是在**玩家自己的回合内**打出的，所以这里指的是「下一回合开始」——
    /// 正好对应「下回合获得的能量全部转为鬼气」。
    /// </summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        // 0 表示回合数已用尽（此时也已经被移除了，这里是兜底）。
        if (_turnsRemaining == 0)
        {
            return;
        }

        _armed = true;

        // ---- 额外产出（这两项与「是否拦截到能量」无关，按回合无条件给）----
        if (GhostQiPerTurn > 0)
        {
            Flash();
            await GhostQi.Gain(player, GhostQiPerTurn);
        }

        if (DrawPerTurn > 0)
        {
            await CardPileCmd.Draw(choiceContext, DrawPerTurn, player);
        }
    }

    /// <summary>吞掉能量增量，返回 0（改为记账）。</summary>
    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        if (!_armed || player.Creature != Owner || amount <= 0m)
        {
            return amount;
        }

        _pending += (int)amount;
        return 0m;
    }

    /// <summary>把这一批被吞掉的能量一次性折算成鬼气；到期的能力随即自我移除。</summary>
    public override async Task AfterModifyingEnergyGain()
    {
        if (!_armed)
        {
            return;
        }

        _armed = false;

        // 消耗一个回合额度（永久则永远不递减）。
        if (_turnsRemaining > 0)
        {
            _turnsRemaining--;
        }

        if (_pending > 0 && Owner.Player is { } player)
        {
            var toConvert = _pending;
            _pending = 0;

            Flash();
            await GhostQi.Gain(player, toConvert);
        }

        // 额度用尽 → 从状态栏移除（它不再有任何效果，留着只会误导玩家）。
        if (_turnsRemaining == 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
