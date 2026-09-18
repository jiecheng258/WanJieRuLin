using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 黑暗壁障 —— 技能牌 1 鬼气：本回合获得 3 点临时敏捷，获得 4 点格挡。
/// 升级后本回合获得 4 点临时敏捷，获得 6 点格挡。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
public sealed class HeiAnBiZhang : WanJieRuLinCardModel
{
    private const int GhostQiCost = 1;
    private const int TemporaryDexterity = 3;
    private const int BlockAmount = 4;

    /// <summary>
    /// 临时敏捷对应的卡面变量名（同时也是本地化占位符名）。
    ///
    /// ★ 命名规则（实测，不要凭直觉改）：<c>ModCardVars.Power&lt;TPower&gt;</c>
    /// 内部就是 <c>typeof(TPower).Name</c>，**不会**把结尾的 "Power" 去掉。
    /// 所以 <c>Power&lt;HeiAnBiZhangTempDexterityPower&gt;</c> 生成的变量名是
    /// <c>HeiAnBiZhangTempDexterityPower</c>（带 Power）。
    ///
    /// 写错会同时炸两处：卡面显示裸占位符（事件/商店日志报
    /// "No source extension could handle the selector named ..."），
    /// 以及 OnUpgrade 里读变量抛 KeyNotFoundException。
    /// </summary>
    private const string DexterityVar = "HeiAnBiZhangTempDexterityPower";

    public HeiAnBiZhang() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        SetGhostQiCost(GhostQiCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 鬼气费用（卡面用 {GhostQiCost:secondaryResourceIcons()} 渲染成图标+数字）。
        GhostQiCostVarOf(1),
        ModCardVars.Power<HeiAnBiZhangTempDexterityPower>(TemporaryDexterity),
        new BlockVar(BlockAmount, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 用本模组的临时敏捷包装能力（回合结束自动撤销），来源写成本卡自己。
        await ApplySelf<HeiAnBiZhangTempDexterityPower>(
            choiceContext,
            DynamicVars.GetIntOrDefault(DexterityVar, TemporaryDexterity));

        await GainBlock(choiceContext, DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars[DexterityVar].UpgradeValueBy(1);
    }
}
