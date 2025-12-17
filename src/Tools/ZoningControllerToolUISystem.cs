// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose UI bindings the React UI reads/writes
//    (ToolZoningMode, RoadZoningMode, IsRoadPrefab, IsPhotoMode, ContourEnabled).
//  • Handle triggers (Change/Flip/Toggle) with null guards.
//  • Show the “Zone Change” section when this tool is active OR a RoadPrefab is active.
//  • Provide a Contour toggle when the EasyZoning tool is active.

namespace EasyZoning.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;   // section visibility flag
        private ValueBinding<bool> m_ContourEnabled = null!; // contour toggle in update panel

        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ZoningTool = null!;
        private PhotoModeRenderSystem m_PhotoModeSystem = null!;

        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;
        public bool ContourEnabled => m_ContourEnabled.value;

        // Shared helper: convert zoning mode bits to left/right depths.
        private static int2 DepthsFromMode(ZoningMode mode)
        {
            // Engine convention: x = LEFT, y = RIGHT
            return new int2(
                (mode & ZoningMode.Left) != 0 ? 6 : 0,
                (mode & ZoningMode.Right) != 0 ? 6 : 0);
        }

        public int2 ToolDepths
        {
            get => DepthsFromMode(ToolZoningMode);
            set
            {
                var mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;
                SetToolZoningMode(mode);
            }
        }

        public int2 RoadDepths
        {
            get => DepthsFromMode(RoadZoningMode);
            set
            {
                var mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;
                ChangeRoadZoningMode((int)mode);
            }
        }

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = Mod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[EZ][UI] " + msg);
            }
            catch { }
        }

        private static string ModeToStr(ZoningMode z) =>
            z == ZoningMode.Both ? "Both"
            : z == ZoningMode.Left ? "Left"
            : z == ZoningMode.Right ? "Right"
            : "None";

        private void LogToolDepths(string tag)
        {
            var mode = ToolZoningMode;
            var d = ToolDepths;
            Dbg($"{tag}: ToolZoningMode={ModeToStr(mode)} ToolDepths=({d.x},{d.y})");
        }
#else
        private static void Dbg(string msg) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            AddBinding(m_ToolZoningMode =
                new ValueBinding<int>(Mod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode =
                new ValueBinding<int>(Mod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab =
                new ValueBinding<bool>(Mod.ModID, "IsRoadPrefab", false));
            AddBinding(m_ContourEnabled =
                new ValueBinding<bool>(Mod.ModID, "ContourEnabled", false));

            // Photo mode system → drives IsPhotoMode binding used by UI to hide panel/buttons.
            m_PhotoModeSystem = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();
            AddUpdateBinding(new GetterValueBinding<bool>(
                Mod.ModID,
                "IsPhotoMode",
                () => m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled));

            // Triggers from UI
            AddBinding(new TriggerBinding<int>(Mod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(Mod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(Mod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(Mod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(Mod.ModID, "ToggleZoneControllerTool", ToggleTool));
            AddBinding(new TriggerBinding(Mod.ModID, "ToggleContourLines", ToggleContourLines));

            try
            {
                m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
                if (m_MainToolSystem != null)
                {
                    m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged -= OnToolChanged;
                    m_MainToolSystem.EventPrefabChanged += OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged += OnToolChanged;
                }
            }
            catch { }

            try
            {
                m_ZoningTool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            }
            catch { }

            try
            {
                ToolBaseSystem activeTool = null!;
                PrefabBase activePrefab = null!;
                if (m_MainToolSystem != null)
                {
                    activeTool = m_MainToolSystem.activeTool;
                    try
                    {
                        activePrefab = activeTool != null ? activeTool.GetPrefab() : null!;
                    }
                    catch { activePrefab = null!; }
                }
                bool show = ShouldShowFor(activeTool, activePrefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg(
                    $"Init visibility → show={show}, tool={(activeTool != null ? activeTool.GetType().Name : "(null)")}, prefab={(activePrefab != null ? activePrefab.name : "(null)")}");

#endif
            }
            catch { }

#if DEBUG
            Dbg("UISystem created and bindings registered.");
#endif
        }

        protected override void OnDestroy()
        {
            try
            {
                if (m_MainToolSystem != null)
                {
                    m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged -= OnToolChanged;
                }
            }
            catch { }
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                PrefabBase prefab = null!;
                try
                {
                    prefab = tool != null ? tool.GetPrefab() : null!;
                }
                catch { prefab = null!; }
                bool show = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg(
                    $"OnToolChanged: show={show} activeTool={(tool != null ? tool.GetType().Name : "(null)")} prefab={(prefab != null ? prefab.name : "(null)")}");

#endif
            }
            catch { }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            try
            {
                ToolBaseSystem tool = null!;
                try
                {
                    tool = (m_MainToolSystem != null) ? m_MainToolSystem.activeTool : null!;
                }
                catch { tool = null!; }
                bool show = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg(
                    $"OnPrefabChanged: show={show} prefab={(prefab != null ? prefab.name : "(null)")} tool={(tool != null ? tool.GetType().Name : "(null)")}");

#endif
            }
            catch { }
        }

        private void ToggleTool()
        {
            try
            {
                if (m_MainToolSystem == null || m_ZoningTool == null)
                    return;

                bool enable = m_MainToolSystem.activeTool != m_ZoningTool;
                m_ZoningTool.SetToolEnabled(enable);
#if DEBUG
                Dbg($"ToggleTool → enable={enable}");
#endif
            }
            catch { }
        }

        private void FlipToolBothMode()
        {
            try
            {
                var next = (ToolZoningMode == ZoningMode.Both) ? ZoningMode.None : ZoningMode.Both;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"FlipToolBothMode → Tool={ModeToStr(next)}");
                LogToolDepths("FlipToolBothMode");
#endif
            }
            catch { }
        }

        private void FlipRoadBothMode()
        {
            try
            {
                var next = (RoadZoningMode == ZoningMode.Both) ? ZoningMode.None : ZoningMode.Both;
                m_RoadZoningMode.Update((int)next);
#if DEBUG
                Dbg($"FlipRoadBothMode → Road={ModeToStr(next)}");
#endif
            }
            catch { }
        }

        private void ChangeToolZoningMode(int value)
        {
            try
            {
                m_ToolZoningMode.Update(value);
#if DEBUG
                Dbg($"ChangeToolZoningMode → Tool={ModeToStr((ZoningMode)value)} rawValue={value}");
                LogToolDepths("ChangeToolZoningMode");
#endif
            }
            catch { }
        }

        private void ChangeRoadZoningMode(int value)
        {
            try
            {
                m_RoadZoningMode.Update(value);
#if DEBUG
                Dbg($"ChangeRoadZoningMode → Road={ModeToStr((ZoningMode)value)} rawValue={value}");
#endif
            }
            catch { }
        }

        public void SetToolZoningMode(ZoningMode mode)
        {
            try
            {
                m_ToolZoningMode.Update((int)mode);
#if DEBUG
                Dbg($"SetToolZoningMode → Tool={ModeToStr(mode)}");
                LogToolDepths("SetToolZoningMode");
#endif
            }
            catch { }
        }

        public void FlipToolBothOrNone()
        {
            try
            {
                var next = ToolZoningMode == ZoningMode.Both ? ZoningMode.None :
                           ToolZoningMode == ZoningMode.None ? ZoningMode.Both : ToolZoningMode;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"FlipToolBothOrNone → Tool={ModeToStr(next)}");
                LogToolDepths("FlipToolBothOrNone");
#endif
            }
            catch { }
        }

        public void InvertZoningSideOnly()
        {
            try
            {
                var mode = ToolZoningMode;
                var next =
                    mode == ZoningMode.Left ? ZoningMode.Right :
                    mode == ZoningMode.Right ? ZoningMode.Left :
                    ZoningMode.Left;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"InvertZoningSideOnly → Tool={ModeToStr(next)}");
                LogToolDepths("InvertZoningSideOnly");
#endif
            }
            catch { }
        }

        // RMB behaviour in the tool uses this: cycles Left → Right → Both → Left.
        public void CycleToolSideMode()
        {
            try
            {
                var mode = ToolZoningMode;
                ZoningMode next =
                    mode == ZoningMode.Left ? ZoningMode.Right :
                    mode == ZoningMode.Right ? ZoningMode.Both :
                    ZoningMode.Left;

                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"CycleToolSideMode → Tool={ModeToStr(next)}");
                LogToolDepths("CycleToolSideMode");
#endif
            }
            catch { }
        }

        // Legacy; no longer used by the tool. Kept for compatibility.
        public void RmbPreviewToggle()
        {
            try
            {
                if (ToolZoningMode == ZoningMode.Left || ToolZoningMode == ZoningMode.Right)
                    InvertZoningSideOnly();
                else
                    FlipToolBothOrNone();
            }
            catch { }
        }

        /// <summary>
        /// Toggle terrain contour lines while the zone update tool is active.
        /// If selectedSnap cannot be accessed, this becomes a no-op.
        /// </summary>
        private void ToggleContourLines()
        {
            try
            {
                // Flip the UI binding so the React button can show state.
                bool next = !ContourEnabled;
                m_ContourEnabled.Update(next);

                var toolSystem = m_MainToolSystem;
                if (toolSystem == null)
                    return;

                var active = toolSystem.activeTool;
                if (active == null)
                    return;

                try
                {
                    // ToolBaseSystem exposes selectedSnap; manipulate the ContourLines bit.
                    Snap snap = active.selectedSnap;

                    if (next)
                        snap |= Snap.ContourLines;
                    else
                        snap &= ~Snap.ContourLines;

                    active.selectedSnap = snap;

#if DEBUG
                    Dbg($"ToggleContourLines → {(next ? "ON" : "OFF")}  selectedSnap={snap}");
#endif
                }
                catch
                {
                    // If ToolBaseSystem.selectedSnap changes in a future patch, ignore and avoid crashes.
                }
            }
            catch
            {
            }
        }

        private static bool ShouldShowFor(ToolBaseSystem? tool, PrefabBase? prefab)
        {
            try
            {
                if (tool is ZoningControllerToolSystem)
                    return true;
                if (prefab is RoadPrefab)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
