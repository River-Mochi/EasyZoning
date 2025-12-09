// File: src/Settings/LocaleKO.cs
// Purpose: Korean (ko-KR) strings for Options UI + Panel text.

namespace EasyZoning.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using EasyZoning.Tools;

    public sealed class LocaleKO : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleKO(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "Easy Zoning [EZ]" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "동작" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "정보" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "조닝 옵션" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "키 바인딩" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "존이 지정된 칸 보호" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "미리보기/적용 중 이미 존이 지정된 칸을 변경하지 않습니다.\n<사용 권장>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "점유된 칸 보호" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "미리보기/적용 중 점유된 칸(예: 건물)을 변경하지 않습니다.\n<사용 권장>" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "패널 토글" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Easy Zoning 패널 버튼 표시(기본 Shift+Z)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 패널 토글" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "도로 조닝 선택: 양쪽, 왼쪽, 오른쪽 또는 없음.\n우클릭 전환, 좌클릭 적용." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "모드 이름" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "이 모드의 표시 이름." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "버전" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "현재 모드 버전." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Paradox Mods 페이지 열기." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "모드 Discord 참여." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
