using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using WanJieRuLin.Characters;
using WanJieRuLin.Powers;

namespace WanJieRuLin.Cards;

/// <summary>
/// 作祟 —— ★ 角色核心枢纽牌（Basic）。
///
/// 0 费，耗费 X 鬼气，[消耗]。
/// 打出后：接下来 X 张牌（升级后 X+1 张）按各自能量费获得强化：
///   0–1 费 → 打出时 +1 能量
///   2 费   → 打出时 +1 力量 +1 敏捷，抽 2 张
///   3+ 费  → 打出时把一张 0 费复制（附 [消耗]）放入弃牌堆
///
/// 鬼气是唯一燃料，作祟是唯一引擎，点线面是引擎的三个档位。
/// 作祟牌本身不消耗计数（引擎不是被强化的对象）。
/// </summary>
[RegisterCard(typeof(WanJieRuLinCardPool))]
[RegisterCharacterStarterCard(typeof(WanJieRuLinCharacter), 2)]
public sealed class ZuoSui : WanJieRuLinCardModel
{
    public ZuoSui() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        SetGhostQiCostX();
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            // 万界林（升级）/ 鬼墨真言：作祟不再 [消耗]。
            if (Owner is { } p
                && (p.Creature.GetPowerAmount<WanJieLinPower>() > 0
                    || p.Creature.GetPowerAmount<GuiMoZhenYanPower>() > 0))
            {
                return [];
            }

            return [CardKeyword.Exhaust];
        }
    }

    /// <summary>接下来受影响的张数（X，升级后 X+1）。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Count", 1m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = GhostQiXValue(cardPlay);

        // 施加作祟能力：张数 = X（升级后 X+1）。
        if (Owner is { } player)
        {
            var power = await PowerCmd.Apply<ZuoSuiPower>(
                choiceContext, player.Creature, 1m, player.Creature, this);

            // 单层计数能力：用 Configure 把「张数」写进去（Amount 只用于显示层数）。
            if (power is not null)
            {
                power.Configure(x + (IsUpgraded ? 1 : 0));
            }

            // 打上「本回合已打过作祟」标记，供「墨盾」「墨潮」等条件牌探测。
            await PowerCmd.Apply<ZuoSuiPlayedThisTurnPower>(
                choiceContext, player.Creature, 1m, player.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：作祟多影响 1 张牌（X → X+1）。
    }
}
