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
using WanJieRuLin.Aspects;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 面 —— 受到 4 点伤害，抽 5 张牌。[消耗]
/// 每场战斗首次打出时，将一张此牌的 0 费版本放入弃牌堆。
/// 升级后抽 6 张。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class KuangCao : WanJieRuLinCardModel
{
    public KuangCao() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    /// <inheritdoc />
    public override WanJieAspect Aspect => WanJieAspect.Face;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(5),
        ModCardVars.Int("BloodCost", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner is { } self)
        {
            await CreatureCmd.Damage(choiceContext, self.Creature,
                DynamicVars.GetIntOrDefault("BloodCost", 4), ValueProp.Move, self.Creature, null, null);
        }
        await Draw(choiceContext, DynamicVars.Cards.IntValue);
        await ReturnFaceZeroCostCopy(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);    }
}
