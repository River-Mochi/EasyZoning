// File: src/Systems/PanelBootStrapSystem.cs

// NOTE: PanelBuilder is not used in Phase 1 of Easy Zoning.
// All calls are commented out in Mod.OnLoad; RoadServices button is disabled.
// Keep this file for possible Phase 2 re-enable.

// Purpose:
//   Wait until a RoadsServices donor/anchor exists, then call PanelBuilder.InstantiateTools().
//   Arms only after a game load, then turns itself off.



namespace EasyZoning.Systems
{
    using Colossal.Serialization.Entities; // Purpose, GameMode
    using Game;
    using Game.Prefabs;
    using Unity.Entities;

    public sealed partial class PanelBootStrapSystem : GameSystemBase
    {
        // --- RETRY TUNING ----------------------------------------------------
        private const int MaxTries = 2000;    // Poll up to kMaxTries frames looking for a donor tile.
        private const int LogEvery = 50;      // Log every kLogEvery tries in DEBUG.

        // --- State -----------------------------------------------------------
        private PrefabSystem m_Prefabs = null!;
        private bool m_Armed;
        private bool m_Done;
        private int m_Tries;

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = Mod.s_Log;
            if (log != null)
            {
                try
                {
                    log.Info("[EZ][Bootstrap] " + msg);
                }
                catch { }
            }
        }
#else
        private static void Dbg(string msg)
        {
        }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Prefabs = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
            m_Armed = false;
            m_Done = false;
            m_Tries = 0;

            Enabled = false; // stay off until we’re in a playable map
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            bool realGame =
                mode == GameMode.Game &&
                (purpose == Purpose.LoadGame || purpose == Purpose.NewGame);

            if (!realGame)
            {
                m_Armed = false;
                m_Done = true;
                m_Tries = 0;
                Enabled = false;
#if DEBUG
                Dbg($"OnGameLoadingComplete(mode={mode}, purpose={purpose}) → not gameplay; disarmed.");
#endif
                return;
            }

            m_Armed = true;
            m_Done = false;
            m_Tries = 0;
            Enabled = true;

#if DEBUG
            Dbg("OnGameLoadingComplete → armed; will begin polling for RoadsServices donor …");
#endif
        }

        protected override void OnUpdate()
        {
            if (!m_Armed || m_Done || m_Prefabs == null)
                return;

            if (PanelBuilder.TryResolveDonor(m_Prefabs, out PrefabBase? donor, out UIObject? donorUI))
            {
#if DEBUG
                if (donorUI != null)
                {
                    string groupName = (donorUI.m_Group != null) ? donorUI.m_Group.name : "(null)";
                    Dbg($"Donor found: '{(donor != null ? donor.name : "(null)")}' group='{groupName}' priority={donorUI.m_Priority}");
                }
#endif
                PanelBuilder.InstantiateTools(logIfNoDonor: true);

                m_Done = true;
                Enabled = false;
                return;
            }

            // Still waiting for donor.
            m_Tries++;

#if DEBUG
            if ((m_Tries % LogEvery) == 0)
                Dbg($"Still waiting for RoadsServices donor… tries={m_Tries}");
#endif

            if (m_Tries >= MaxTries)
            {
                Mod.s_Log.Error("[EZ][Bootstrap] Giving up; RoadsServices donor never appeared.");
                m_Armed = false;
                Enabled = false;
            }
        }
    }
}
