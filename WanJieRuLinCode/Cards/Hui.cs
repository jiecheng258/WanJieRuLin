using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 绘 —— 技能牌 0 点能量、X 点鬼气：
/// 本回合内提升 X 点临时力量、X 点临时敏捷。升级后获得「保留」。
///
/// 这是角色最核心的 X 费牌：鬼气越多，本回合爆发越高。
/// 用本模组的 WanJieTempStrengthPower / WanJieTempDexterityPower
/// （RitsuLib 临时能力包装，回合结束自动撤销）——
/// 不能裸用原版 Temporary*Power，否则能力半初始化会卡住出牌。
///
/// 同时它也是「古旧尖牙」的超越对象：持有古旧尖牙时，
/// 「绘」会被转化为先古卡「墨染江山」。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
[RegisterCharacterStarterCard(typeof(WanJieRuLinCharacter), 2)]
[RegisterArchaicToothTranscendence(typeof(MoRanJiangShan))]
public sealed class Hui : WanJieRuLinCardModel
{
    public Hui() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        // 0 能量 + X 鬼气。鬼气即这张牌的"费用"。
        SetGhostQiCostX();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        GhostQiGainVarOf(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // X = 本张牌实际消耗掉的鬼气（已在支付阶段扣除）。
        var x = GhostQiXValue(cardPlay);
        if (x <= 0)
        {
            return;
        }

        await ApplySelf<HuiTempStrengthPower>(choiceContext, x);
        await ApplySelf<HuiTempDexterityPower>(choiceContext, x);
    }

    protected override void OnUpgrade()
    {
        // 绘之名，绘尽山河——升级后不再担心抽到就废：获得保留。
        AddKeyword(CardKeyword.Retain);
    }
}
