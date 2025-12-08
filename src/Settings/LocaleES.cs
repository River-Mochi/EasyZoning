// File: src/Settings/LocaleES.cs
// Purpose: Spanish (es-ES) strings for Options UI + Panel text.

namespace EasyZoning.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using EasyZoning.Tools;

    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleES(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "Easy Zoning [EZ]" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de zonificación" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Evitar que se eliminen celdas zonificadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "No cambiar celdas ya zonificadas durante la vista previa/aplicación.\n<Se recomienda habilitar.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar que se eliminen celdas ocupadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "No cambiar celdas ocupadas durante la vista previa/aplicación (p. ej., edificios).\n<Se recomienda habilitar.>" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Alternar panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Mostrar el botón del panel de Easy Zoning (por defecto Shift+Z)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(EasyZoningMod.kToggleToolActionName), "Alternar panel de Easy Zoning" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Elige la zonificación de las carreteras: ambos, izquierda, derecha o ninguna.\nClic derecho alterna; clic izquierdo aplica." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nombre del mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nombre visible de este mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Abrir la página de Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Unirse al Discord del mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
