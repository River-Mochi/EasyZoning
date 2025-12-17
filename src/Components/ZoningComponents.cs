// File: src/Components/ZoningComponents.cs
// Purpose: Holds temporary preview depths (left/right) while hovering / previewing.
// Preview = temporary overlay, Depth = real stored setting.

namespace EasyZoning.Components
{
    using System;
    using Colossal.Serialization.Entities;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// Live preview depths (hover/flip) for a road entity.
    /// Depths.x = left, Depths.y = right (cells).
    /// </summary>
    public struct ZoningPreviewComponent : IComponentData
    {
        public int2 Depths; // x = left, y = right
    }

    /// <summary>
    /// Committed/desired zoning depths for a road entity.
    /// Depths.x = left, Depths.y = right (cells).
    /// </summary>
    public struct ZoningDepthComponent :
        IComponentData,
        IEquatable<ZoningDepthComponent>,
        ISerializable
    {
        public int depthLeft;
        public int depthRight;

        public int2 Depths
        {
            get => new int2(depthLeft, depthRight);
            set
            {
                depthLeft = value.x;
                depthRight = value.y;
            }
        }

        public bool Equals(ZoningDepthComponent other) =>
            other.depthLeft == depthLeft && other.depthRight == depthRight;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(depthLeft);
            writer.Write(depthRight);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out depthLeft);
            reader.Read(out depthRight);
        }
    }
}
