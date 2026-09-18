using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
            // 必须从 ModelDb 取规范实例再克隆。
            // 直接 new YinSenSen() 会走 AbstractModel 构造 → DuplicateModelException
            // （CARD.WAN_JIE_RU_LIN_CARD_YIN_SEN_SEN 已经映射过了）。
            var token = ModelDb.Card<YinSenSen>().CreateCloneForPlayer(player);
            await ShuffleToDrawPile(token);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
