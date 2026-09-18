using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;

namespace WanJieRuLin.Cards;

/// <summary>
/// 阴森 —— 攻击牌 2 能量：造成 9 点伤害 2 次，消耗。将一张「阴森森」洗入抽牌堆。
/// 升级后洗入两张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class YinSen : WanJieRuLinCardModel
{
    private const int BaseHits = 2;

    public YinSen() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
        ModCardVars.Repeat(BaseHits),
        ModCardVars.Cards(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);

        if (Owner is not { } player)
        {
            return;
        }

        var copies = DynamicVars.Cards.IntValue;
        for (var i = 0; i < copies; i++)
        {
            // ★ 造牌姿势（IL 反编译确认）：
            //
            // 不能 `new YinSenSen()` —— 走 AbstractModel 构造会
            // DuplicateModelException（该模型已被 [RegisterCard] 注册）。
            //
            // 也不能 `ModelDb.Card<YinSenSen>().ToMutable().CreateCloneForPlayer(player)`：
            // `CreateCloneForPlayer` 的 IL 只有 `CreateClone()` + `_owner = player`，
            // 从不把牌放进堆；而 `CreateClone()` 一开头就读 `get_Pile()`，
            // 对一张还没有堆的新牌必然 NullReferenceException。
            //
            // 正确做法是走战斗内的官方造牌工厂
            // `ICardScope.CreateCard(canonicalCard, owner)`，它的 IL 是：
            //     CardModel.ToMutable()          ← 取可变副本
            //     CombatState.AddCard(mutable, owner)  ← 设 Owner + 登记进 CombatState
            //     CardModel.AfterCreated()       ← 派生类钩子
            // 一步到位，且牌会被正确登记（CreateClone() 之后才读得到 Pile）。
            var token = MakeToken(player);

            await ShuffleToDrawPile(token);
        }
    }

    /// <summary>
    /// 造一张「阴森森」。优先走战斗内的官方造牌工厂；
    /// 若不在战斗上下文则退化为「ToMutable + 设 Owner」。
    /// </summary>
    private static CardModel MakeToken(Player player)
    {
        var canonical = ModelDb.Card<YinSenSen>();

        if (player.Creature?.CombatState is { } combatState)
        {
            return combatState.CreateCard(canonical, player);
        }

        var mutable = canonical.ToMutable();
        mutable.Owner = player;
        return mutable;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
