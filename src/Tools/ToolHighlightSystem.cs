// File: src/Tools/ToolHighlightSystem.cs
// Purpose:
//   Track which road entities are "highlighted" by the zoning tool (hover / multi-select).
//   Adds Updated when highlight state changes so the game can react if it wants.

namespace EasyZoning.Tools
{
    using System.Collections.Generic;
    using Game;
    using Game.Common;
    using Unity.Entities;

    public sealed partial class ToolHighlightSystem : GameSystemBase
    {
        private HashSet<Entity> m_Highlighted = null!;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Highlighted = new HashSet<Entity>();
        }

        protected override void OnDestroy()
        {
            ClearAll();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            // No per-frame work; state is driven by explicit calls.
        }

        public void HighlightEntity(Entity entity, bool enable)
        {
            if (!EntityManager.Exists(entity))
                return;

            if (enable)
            {
                if (m_Highlighted.Add(entity))
                    EntityManager.AddComponent<Updated>(entity);
            }
            else
            {
                if (m_Highlighted.Remove(entity))
                    EntityManager.AddComponent<Updated>(entity);
            }
        }

        public void ClearAll()
        {
            if (m_Highlighted.Count == 0)
                return;

            foreach (var e in m_Highlighted)
            {
                if (EntityManager.Exists(e))
                    EntityManager.AddComponent<Updated>(e);
            }

            m_Highlighted.Clear();
        }
    }
}
