// File: src/Tools/ToolDefinition.cs
// Purpose:
//   Describes one EasyZoning tool so PanelBuilder can create a clickable tile in RoadsServices.
//   PanelBuilder reads this struct to make One ToolDefinition
//      = one Panel (clone donor, set custom icon/ID, hook to ToolBaseSystem,
//        apply placement flags (underground, etc.).

namespace EasyZoning.Tools
{
    using System;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;

    // --- Data type ------------------------------------------------------------
    public readonly struct ToolDefinition : IEquatable<ToolDefinition>
    {
        // C# system that implements the tool
        public readonly Type Type;      // Must inherit ToolBaseSystem

        // Unique ID; also becomes the cloned prefab name and must match UI’s tool.activeTool$.id.
        public readonly string ToolID;

        // Icon and visual data for the tile.
        public readonly UI Ui;

        // Placement behavior (copied into PlaceableNetData).
        public readonly bool Underground;
        public readonly PlacementFlags PlacementFlags;
        public readonly CompositionFlags SetFlags;
        public readonly CompositionFlags UnsetFlags;

        // ---- Constructor -----------------------------------------------------
        public ToolDefinition(
            Type toolSystemType,
            string toolId,
            UI ui,
            bool underground = false,
            PlacementFlags placementFlags = default,
            CompositionFlags setFlags = default,
            CompositionFlags unsetFlags = default)
        {
            if (toolSystemType == null)
                throw new ArgumentNullException(nameof(toolSystemType));
            if (!typeof(ToolBaseSystem).IsAssignableFrom(toolSystemType))
                throw new ArgumentException("Type must inherit ToolBaseSystem.", nameof(toolSystemType));

            if (string.IsNullOrWhiteSpace(toolId))
                throw new ArgumentException("ToolID must be non-empty.", nameof(toolId));

            Type = toolSystemType;
            ToolID = toolId;
            Ui = ui;

            Underground = underground;
            PlacementFlags = placementFlags;
            SetFlags = setFlags;
            UnsetFlags = unsetFlags;
        }

        // ---- Nested UI descriptor ---------------------------------------------
        public readonly struct UI
        {
            public readonly string ImagePath; // e.g., "coui://ui-mods/images/tool-icon01.png"

            public UI(string imagePath)
            {
                ImagePath = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
            }
        }

        // ---- Equality ----------------------------------------------------------
        public bool Equals(ToolDefinition other) =>
            string.Equals(ToolID, other.ToolID, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is ToolDefinition other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.Ordinal.GetHashCode(ToolID);
    }
}
