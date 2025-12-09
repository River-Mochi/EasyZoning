// File: src/Mod.cs
// Purpose: Mod entrypoint; locales + settings + keybindings + tool registration (no Harmony).
// Notes:
//   • Locales install BEFORE Options UI so labels render correctly.
//   • RMB flip stays vanilla (ToolBaseSystem.cancelAction) — no custom binding.
//   • Top-left button & Panel tile point at coui://ui-mods/images/* assets.

namespace EasyZoning
{
    using System.Collections.Generic; // HashSet
    using System.Reflection;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using EasyZoning.Settings;
    using EasyZoning.Tools;       // <-- source of PanelBuilder/ToolDefinition
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;

    public sealed class Mod : IMod
    {
        // ---- PUBLIC CONSTANTS / METADATA ----
        public const string ModName = "Easy Zoning";
        public const string ModID = "EasyZoning";
        public const string ModTag = "[EZ]";

        /// <summary>
        /// Read Version from .csproj (3-part), fallback to "1.0.0".
        /// </summary>
        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";


        // COUI base
        public const string UiCouiRoot = "coui://ui-mods";

        // Top-left floating action button (color)
        public const string MainIconPath = UiCouiRoot + "/images/ico-zones-color02.svg";

        // Not used currently: Road Services panel button (ico-zones-color02.svg under UI/images)
        // public const string PanelIconPath = UiCouiRoot + "/images/ico-zones-color02.svg";

        // Rebindable action IDs exposed in Options UI
        public const string kToggleToolActionName = "ToggleZoneTool";   // default Shift+Z

        /// <summary>
        /// Global settings instance
        /// </summary>
        public static Setting? Settings
        {
            get; private set;
        }
        public static ProxyAction? ToggleToolAction
        {
            get; private set;
        }

        public static readonly ILog s_Log =
            LogManager.GetLogger(ModID).SetShowsErrorsInUI(false);

        // ---- PRIVATE STATE ----
        private static readonly HashSet<string> s_InstalledLocales = new();
        private static bool s_ReapplyingLocale;
        private static bool s_BannerLogged;


        public void OnLoad(UpdateSystem updateSystem)
        {

            // One-time banner.
            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                s_Log.Info($"{ModName} {ModTag} v{ModVersion} OnLoad");
            }

            var settings = new Setting(this);
            Settings = settings;

            // Locales first
            AddLocale("en-US", new LocaleEN(settings));

            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key binding
            try
            {
                settings.RegisterKeyBindings();
                ToggleToolAction = settings.GetAction(kToggleToolActionName);
                if (ToggleToolAction != null)
                    ToggleToolAction.shouldBeEnabled = true;
            }
            catch (System.Exception ex)
            {
                s_Log.Warn($"[EZ] Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // Systems
            // updateSystem.UpdateAt<PanelBootStrapSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate);

            // definition only; prefab created after game load
            // PanelBuilder.Initialize(force: false);

            // NOTE [EZ]:
            // Road Services panel: the icon appears. but clicking it does *not* open
            // zoning side panel currently. For Phase1, disable this to avoid confusing players.
            // GameTopLeft FAB + Shift+Z remain the supported entry points.
            //
            //PanelBuilder.RegisterTool(
            //    new ToolDefinition(
            //        typeof(ZoningControllerToolSystem),
            //        ZoningControllerToolSystem.ToolID,
            //        new ToolDefinition.UI(PanelIconPath)
            //    )
            //);


            var lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }
        }

        public void OnDispose()
        {
            s_Log.Info("[EZ] OnDispose");

            var lm = GameManager.instance?.localizationManager;
            if (lm != null)
                lm.onActiveDictionaryChanged -= OnLocaleChanged;

            if (ToggleToolAction != null)
            {
                ToggleToolAction.shouldBeEnabled = false;
                ToggleToolAction = null;
            }
            Settings?.UnregisterInOptionsUI();
            Settings = null;
        }

        private static void AddLocale(string id, IDictionarySource src)
        {
            var lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn($"[EZ] No LocalizationManager; cannot add locale {id}");
                return;
            }
            if (!s_InstalledLocales.Add(id))
                return;
            lm.AddSource(id, src);
            s_Log.Info($"[EZ] Locale installed: {id}");
        }

        private static void OnLocaleChanged()
        {
            if (s_ReapplyingLocale)
                return;
            s_ReapplyingLocale = true;
            try
            {
                var id = GameManager.instance?.localizationManager?.activeLocaleId ?? "(unknown)";
                s_Log.Info("[EZ] Active locale = " + id);
                Settings?.RegisterInOptionsUI();
            }
            finally { s_ReapplyingLocale = false; }
        }
    }
}
