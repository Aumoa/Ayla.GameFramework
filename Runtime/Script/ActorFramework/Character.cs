using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

#nullable enable

namespace Ayla
{
    [BurstCompile]
    public class Character : Actor
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

        protected override void OnEnable()
        {
            base.OnEnable();
            m_EntityManager = EntityWorldHelper.GetOrCreateDefaultWorld().EntityManager;

            m_Entity = m_EntityManager.CreateEntity(typeof(ComponentData), typeof(LocalTransform), typeof(LocalToWorld));
            m_EntityManager.SetComponentData(m_Entity, LocalTransform.Identity);
            m_EntityManager.SetName(m_Entity, new FixedString64Bytes(name));

            var collider = SphereCollider.Create(new SphereGeometry
            {
                Center = float3.zero,
                Radius = 0.5f
            });

            m_EntityManager.AddComponentData(m_Entity, new PhysicsCollider { Value = collider });

            m_EntityManager.AddComponentData(m_Entity, PhysicsMass.CreateDynamic(collider.Value.MassProperties, 1f));
            m_EntityManager.AddComponentData(m_Entity, PhysicsVelocity.Zero);
            m_EntityManager.AddComponentData(m_Entity, new PhysicsDamping
            {
                Linear = 0.1f,
                Angular = 0.05f
            });
            m_EntityManager.AddComponentData(m_Entity, new PhysicsGravityFactor { Value = 1f });
            m_EntityManager.AddSharedComponent(m_Entity, new PhysicsWorldIndex { Value = 0 });
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (ApplicationMisc.IsInTearingDown() == false && m_Entity != Entity.Null)
            {
                m_EntityManager.DestroyEntity(m_Entity);
                m_Entity = Entity.Null;
            }
        }

        private void LateUpdate()
        {
            var lt = m_EntityManager.GetComponentData<LocalTransform>(m_Entity);
            transform.SetLocalPositionAndRotation(lt.Position, lt.Rotation);
        }

        public Entity GetEntity() => m_Entity;
    }
}
