// File: src/Settings/LocaleJA.cs
// Purpose: Japanese (ja-JP) strings for Options UI + Panel text.

namespace EasyZoning.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using EasyZoning.Tools;

    public sealed class LocaleJA : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleJA(Setting setting) => m_Settings = setting;

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
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "概要" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "ゾーニング設定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "キー設定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "既存のゾーンセルを変更しない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "プレビュー／適用中に既にゾーン指定されたセルを変更しません。\n〈有効を推奨〉" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "占有セルを変更しない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "プレビュー／適用中に占有セル（建物など）を変更しません。\n〈有効を推奨〉" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "パネル切替" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Easy Zoning のパネルボタンを表示（既定は Shift+Z）。" },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning パネル切替" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "道路のゾーニングを選択：両側／左／右／なし。\n右クリックで切替、左クリックで適用。" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod 名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "この Mod の表示名。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "現在の Mod バージョン。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Paradox Mods を開く。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Mod の Discord に参加。" },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
