#nullable enable

using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Ayla
{
    [RequireComponent(typeof(Character))]
    [BurstCompile]
    public class CharacterMovement : MonoBehaviour
    {
        [BurstCompile]
        public struct ComponentData : IComponentData
        {
            [BurstCompile]
            public readonly void Update()
            {
            }
        }

        private EntityManager m_EntityManager;
        private Entity m_Entity;

        protected virtual void OnEnable()
        {
            var character = GetComponent<Character>();

            m_EntityManager = EntityWorldHelper.GetOrCreateDefaultWorld().EntityManager;
            m_Entity = character.GetEntity();

            m_EntityManager.AddComponentData(m_Entity, new ComponentData());
        }

        protected virtual void OnDisable()
        {
            if (ApplicationMisc.IsInTearingDown() == false && m_Entity != Entity.Null)
            {
                m_EntityManager.RemoveComponent<ComponentData>(m_Entity);
                m_Entity = Entity.Null;
            }
        }
    }
}
