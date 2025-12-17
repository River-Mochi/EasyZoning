// File: src/Tools/SyncBlockSystem.cs
// Purpose: applies the preview/committed zoning depth to zone blocks
// respecting settings (RemoveZonedCells / RemoveOccupiedCells). Tool won’t function without it.

namespace EasyZoning.Tools
{
    using System;
    using EasyZoning.Components;
    using Game;
    using Game.Common;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    public partial class SyncBlockSystem : GameSystemBase
    {
        private EntityQuery m_UpdatedBlocksQuery;
        private ModificationBarrier4B m_ModificationBarrier = null!;

#if DEBUG
        private int m_LogTick;
        private int m_LastCount;
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            m_UpdatedBlocksQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<Block, ValidArea>()
                .WithAll<Owner, Updated>()
                .Build(this);

            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4B>();

#if DEBUG
            m_LogTick = 0;
            m_LastCount = -1;
#endif
        }

        protected override void OnUpdate()
        {
            if (m_UpdatedBlocksQuery.IsEmpty)
                return;

#if DEBUG
            int count = m_UpdatedBlocksQuery.CalculateEntityCount();
            m_LogTick++;
            if (count != m_LastCount || (m_LogTick % 30) == 0)
            {
                Mod.s_Log.Info(
                    $"[EZ][SyncBlock] blocks={count} removeOcc={Mod.Settings?.RemoveOccupiedCells == true} removeZoned={Mod.Settings?.RemoveZonedCells == true}");
                m_LastCount = count;
            }
#endif

            var ecb = m_ModificationBarrier.CreateCommandBuffer();
            var updatedBlocks = m_UpdatedBlocksQuery.ToEntityArray(Allocator.TempJob);

            var syncBlockJob = new SyncBlockJob
            {
                ECB = ecb.AsParallelWriter(),
                Entities = updatedBlocks.AsReadOnly(),
                BlockLookup = GetComponentLookup<Block>(true),
                ValidAreaLookup = GetComponentLookup<ValidArea>(true),
                OwnerLookup = GetComponentLookup<Owner>(true),
                CellLookup = GetBufferLookup<Cell>(true),
                ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(true),
                ZoningPreviewLookup = GetComponentLookup<ZoningPreviewComponent>(true),
            }.Schedule(m_UpdatedBlocksQuery.CalculateEntityCount(), 32, this.Dependency);

            updatedBlocks.Dispose(syncBlockJob);
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, syncBlockJob);
            m_ModificationBarrier.AddJobHandleForProducer(this.Dependency);
        }

        public struct SyncBlockJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly Entities;

            [ReadOnly] public ComponentLookup<Block> BlockLookup;
            [ReadOnly] public ComponentLookup<ValidArea> ValidAreaLookup;
            [ReadOnly] public BufferLookup<Cell> CellLookup;
            [ReadOnly] public ComponentLookup<Owner> OwnerLookup;
            [ReadOnly] public ComponentLookup<ZoningDepthComponent> ZoningDepthLookup;
            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;

            public void Execute(int index)
            {
                Entity blockEntity = Entities[index];

                Block block = BlockLookup[blockEntity];
                ValidArea validArea = ValidAreaLookup[blockEntity];

                if (!OwnerLookup.TryGetComponent(blockEntity, out Owner owner))
                    throw new NullReferenceException($"Block {blockEntity} has no owner assigned.");

                Entity roadEntity = owner.m_Owner;

                // NOTE:
                // tool/UI uses int2 where x=LEFT and y=RIGHT (engine convention).
                // The zone Block side detection here was effectively inverted for this mod,
                // so Left-only and Right-only got swapped. Fix = swap the chosen depth.
                bool left = (math.dot(1, block.m_Direction) < 0);

                int depth;
                if (ZoningPreviewLookup.TryGetComponent(roadEntity, out ZoningPreviewComponent zoningPreview))
                {
                    // FIX: swap x/y mapping at application point
                    depth = left ? zoningPreview.Depths.y : zoningPreview.Depths.x;
                }
                else if (ZoningDepthLookup.TryGetComponent(roadEntity, out ZoningDepthComponent data))
                {
                    // FIX: swap x/y mapping at application point
                    depth = left ? data.Depths.y : data.Depths.x;
                }
                else
                {
                    return;
                }

                // Respect settings
                if (Mod.Settings != null)
                {
                    if (Mod.Settings.RemoveOccupiedCells &&
                        IsAnyCellOccupied(CellLookup[blockEntity], block, validArea))
                        return;

                    if (Mod.Settings.RemoveZonedCells &&
                        IsAnyCellZoned(CellLookup[blockEntity], block, validArea))
                        return;
                }

                block.m_Size.y = depth;
                ECB.SetComponent(index, blockEntity, block);

                validArea.m_Area.w = depth;
                ECB.SetComponent(index, blockEntity, validArea);
            }

            private readonly bool IsAnyCellOccupied(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                if (validArea.m_Area.y * validArea.m_Area.w == 0)
                    return false;

                for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
                {
                    for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                    {
                        int idx = z * block.m_Size.x + x;
                        Cell cell = cells[idx];
                        if ((cell.m_State & CellFlags.Occupied) != 0)
                            return true;
                    }
                }
                return false;
            }

            private readonly bool IsAnyCellZoned(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                if (validArea.m_Area.y * validArea.m_Area.w == 0)
                    return false;

                for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
                {
                    for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                    {
                        int idx = z * block.m_Size.x + x;
                        Cell cell = cells[idx];
                        if (cell.m_Zone.m_Index != ZoneType.None.m_Index)
                            return true;
                    }
                }
                return false;
            }
        }
    }
}
