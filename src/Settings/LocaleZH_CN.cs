// File: src/Settings/LocaleZH_CN.cs
// Purpose: Simplified Chinese (zh-HANS) strings for Options UI + Panel text.

namespace EasyZoning.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using EasyZoning.Tools;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleZH_CN(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "Easy Zoning [EZ]" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分区选项" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按键绑定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "防止已分区格子被更改" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在预览/应用时不修改已分区的格子。\n<建议启用。>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止占用格子被更改" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "在预览/应用时不修改已占用的格子（如建筑）。\n<建议启用。>" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切换面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "显示 Easy Zoning 面板按钮（默认 Shift+Z）。" },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(EasyZoningMod.kToggleToolActionName), "切换 Easy Zoning 面板按钮" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "为道路选择分区：双侧、左侧、右侧或无。\n右键切换；左键应用。" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模组名称" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "该模组的显示名称。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "当前模组版本。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "打开 Paradox Mods 页面。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "加入模组的 Discord。" },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
