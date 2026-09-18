using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

namespace WanJieRuLin;

/// <summary>
/// 万界如林的核心资源：鬼气。
///
/// 设计要点（与「星辉」同构）：
/// - 鬼气在<b>一场战斗内跨回合保留</b>，但<b>不会带到下一场战斗</b>。
///   每进入新战斗时回到 <see cref="GhostQiCombatStartAmount"/>（1 点）。
/// - 回合开始时不做自动充能（None）；鬼气完全由遗物 / 卡牌提供，
///   避免「每回合白给」导致 X 费牌失衡。
/// - 软上限由 baseMaxAmount 给出一个宽松值，超出部分靠卡牌自肃。
/// </summary>
public static class ModResources
{
    /// <summary>
    /// 鬼气的<b>本地 ID</b>（注册时用的短名）。
    /// 全局 ID 由 RitsuLib 拼成 <c>{MODID}_SECONDARY_RESOURCE_{LOCALID}</c>，
    /// 实测为 <c>WAN_JIE_RU_LIN_SECONDARY_RESOURCE_GHOST_QI</c>。
    ///
    /// 需要引用鬼气的<b>变量/占位符</b>时优先用这个 + <see cref="Entry.ModId"/>，
    /// 走 <c>SecondaryResourceVars.ForLocal</c>，这样不依赖注册顺序。
    /// </summary>
    public const string GhostQiLocalId = "ghost_qi";

    public static SecondaryResourceDefinition GhostQiDefinition { get; private set; } = null!;

    /// <summary>鬼气的全局 ID，注册后为 "WAN_JIE_RU_LIN_SECONDARY_RESOURCE_GHOST_QI"。</summary>
    public static string GhostQiId { get; private set; } = string.Empty;

    /// <summary>鬼气默认软上限（显示与提示用）。</summary>
    public const int GhostQiSoftCap = 20;

    /// <summary>鬼气硬上限：防止任何循环无限堆叠导致数值溢出。</summary>
    public const int GhostQiHardCap = 99;

    /// <summary>每场战斗开始时鬼气的起始值（星辉式：每场战斗重置后从 1 开始）。</summary>
    public const int GhostQiCombatStartAmount = 1;

    // 视觉基调：墨黑底色 + 幽紫高光。
    private static readonly Color GhostPurple = new(0.66f, 0.45f, 0.95f);

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        GhostQiDefinition = registry.Register(GhostQiLocalId, new SecondaryResourceDefinition(
            defaultAmount: GhostQiCombatStartAmount,
            baseMaxAmount: GhostQiSoftCap,
            minAmount: 0,
            hardMaxAmount: GhostQiHardCap,
            // 回合开始不自动增长：增长由「鬼墨」遗物和卡牌负责。
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            // 星辉式：只在本场战斗中保留，进入下一场战斗时重新计算，不会跨战斗叠加。
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            // 悬浮提示默认读 base 表 static_hover_tips；
            // 对应的 localization/{lang}/static_hover_tips.json 由 gen_loc.py 生成。
            locTable: SecondaryResourceDefinition.DefaultLocTable,
            titleKey: "WAN_JIE_RU_LIN_GHOST_QI_TITLE",
            descriptionKey: "WAN_JIE_RU_LIN_GHOST_QI_DESC",
            smallIconPath: $"{Entry.ResPath}/images/resources/ghost_qi_small.png",
            largeIconPath: $"{Entry.ResPath}/images/resources/ghost_qi_large.png"));

        GhostQiId = GhostQiDefinition.Id;

        // 战斗中的鬼气计数盘：常驻显示在能量盘旁边。
        registry.RegisterCombatUi(
            "ghost_qi_combat_counter",
            (NCombatUi parent) =>
            {
                var counter = NSecondaryResourceCounter.Create(GhostQiDefinition, new SecondaryResourceCounterStyle
                {
                    FontSize = 30,
                    PositiveColor = GhostPurple,
                    IconSize = new Vector2(64, 64),
                });

                var energyCounter = parent.GetNodeOrNull<Control>("%EnergyCounterContainer");
                counter.Position = energyCounter != null
                    ? energyCounter.Position + new Vector2(118, -108)
                    : new Vector2(0, -108);
                return counter;
            },
            ctx => ctx.Node.Bind(ctx.Player, true),
            new NodeAttachmentOptions());

        // 卡面上的鬼气费用图标。
        registry.RegisterCardUi(
            "ghost_qi_card_ui",
            (NCard parent) =>
            {
                var ui = NSecondaryResourceCardCostUi.Create(GhostQiDefinition,
                    new SecondaryResourceCardCostUiStyle
                    {
                        IconSize = new Vector2(44, 44),
                        FontSize = 22,
                    });

                var energyIcon = parent.GetNodeOrNull<TextureRect>("%EnergyIcon");
                ui.Position = energyIcon != null
                    ? energyIcon.Position + new Vector2(0, 76)
                    : new Vector2(0, 76);
                return ui;
            },
            ctx => ctx.Node.Refresh(ctx.Card, ctx.Plan),
            new NodeAttachmentOptions());

        // 战斗界面常显鬼气盘。
        registry.AlwaysShowInCombatUi(GhostQiDefinition.LocalId);

        // 自查：ID 必须已经拼好，否则卡牌构造函数里挂的费用会落到空 ID 上，
        // 表现就是「牌面显示要花鬼气，但打出后鬼气根本没扣」。
        if (string.IsNullOrEmpty(GhostQiId))
        {
            throw new InvalidOperationException(
                "鬼气注册失败：GhostQiId 为空，卡牌费用会挂到无效资源上。");
        }
    }
}
