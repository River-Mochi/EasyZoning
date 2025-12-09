// File: src/Settings/LocaleFR.cs
// Purpose: French (fr-FR) strings for Options UI + Panel text.

namespace EasyZoning.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using EasyZoning.Tools;

    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleFR(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "Easy Zoning [EZ]" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "À propos" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Options de zonage" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Raccourcis clavier" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Empêcher la suppression des cellules zonées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Ne pas modifier les cellules déjà zonées lors de l’aperçu/l’application.\n<Activation recommandée.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Empêcher la suppression des cellules occupées" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Ne pas modifier les cellules occupées lors de l’aperçu/l’application (p. ex. bâtiments).\n<Activation recommandée.>" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Basculer le panneau" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Afficher le bouton du panneau Easy Zoning (Maj+Z par défaut)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Basculer le panneau Easy Zoning" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Choisissez le zonage des routes : les deux côtés, gauche, droite ou aucun.\nClic droit bascule ; clic gauche applique." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nom du mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nom affiché de ce mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Version actuelle du mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Ouvrir la page Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Rejoindre le Discord du mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
