using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 电龙形态 —— 能力牌 3 能量：回合开始时获得 10 点活力、3 点临时力量。虚无。
/// 升级后移除虚无词条。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class DianLongXingTai : WanJieRuLinCardModel
{
    public DianLongXingTai() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplySelf<DianLongFormPower>(choiceContext, 1m);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
