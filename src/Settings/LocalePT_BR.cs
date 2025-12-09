// File: src/Settings/LocalePT_BR.cs
// Purpose: Portuguese (Brazil) (pt-BR) strings for Options UI + Panel text.

namespace EasyZoning.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using EasyZoning.Tools;

    public sealed class LocalePT_BR : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocalePT_BR(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "Easy Zoning [EZ]" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Ações" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Sobre" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opções de zoneamento" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atalhos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Evitar remover células zoneadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Não alterar células já zoneadas durante a prévia/aplicação.\n<Ativar recomendado.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar remover células ocupadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Não alterar células ocupadas durante a prévia/aplicação (ex.: edifícios).\n<Ativar recomendado.>" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Alternar painel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Mostrar o botão do painel do Easy Zoning (padrão Shift+Z)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Alternar painel do Easy Zoning" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Escolha o zoneamento das vias: ambos, esquerda, direita ou nenhum.\nClique direito alterna; clique esquerdo aplica." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome do mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome exibido deste mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versão" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versão atual do mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Abrir a página do Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Entrar no Discord do mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
