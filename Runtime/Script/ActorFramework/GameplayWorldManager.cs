#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    public class GameplayWorldManager : Singleton<GameplayWorldManager>
    {
        [SerializeField, HideInInspector]
        private List<GameplayWorld> m_Worlds = new();

        public IReadOnlyList<GameplayWorld> Worlds => m_Worlds;

        public override ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            RegisterLoadedWorlds();
            return default;
        }

        public GameplayWorld CreateWorld()
        {
            return CreateWorld(typeof(GameplayWorld));
        }

        public GameplayWorld CreateWorld(string? name)
        {
            return CreateWorld(typeof(GameplayWorld), name);
        }

        public T CreateWorld<T>() where T : GameplayWorld
        {
            return (T)CreateWorld(typeof(T));
        }

        public T CreateWorld<T>(string? name) where T : GameplayWorld
        {
            return (T)CreateWorld(typeof(T), name);
        }

        public GameplayWorld CreateWorld(Type worldType)
        {
            return CreateWorld(worldType, worldType?.Name);
        }

        public GameplayWorld CreateWorld(Type worldType, string? name)
        {
            ValidateWorldType(worldType);

            var gameObject = new GameObject(string.IsNullOrEmpty(name) ? worldType.Name : name);
            try
            {
                gameObject.transform.SetParent(Manager.transform, false);
                var world = (GameplayWorld)gameObject.AddComponent(worldType);
                RegisterWorld(world);
                return world;
            }
            catch
            {
                DestroyWorldGameObject(gameObject);
                throw;
            }
        }

        public GameplayWorld[] GetWorlds()
        {
            return m_Worlds.ToArray();
        }

        public bool ContainsWorld(GameplayWorld world)
        {
            ValidateWorld(world);
            return m_Worlds.Contains(world);
        }

        public void RegisterWorld(GameplayWorld world)
        {
            ValidateWorld(world);

            if (m_Worlds.Contains(world))
            {
                return;
            }

            m_Worlds.Add(world);
        }

        public bool UnregisterWorld(GameplayWorld world)
        {
            ValidateWorld(world);
            return m_Worlds.Remove(world);
        }

        public bool DestroyWorld(GameplayWorld world)
        {
            ValidateWorld(world);

            var wasRegistered = UnregisterWorld(world);
            DestroyWorldGameObject(world.gameObject);
            return wasRegistered;
        }

        private void RegisterLoadedWorlds()
        {
#if UNITY_2023_1_OR_NEWER
            var worlds = Object.FindObjectsByType<GameplayWorld>(FindObjectsSortMode.None);
#else
#pragma warning disable CS0618
            var worlds = Object.FindObjectsOfType<GameplayWorld>();
#pragma warning restore CS0618
#endif
            foreach (var world in worlds)
            {
                RegisterWorld(world);
            }
        }

        private static void ValidateWorld(GameplayWorld world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }
        }

        private static void ValidateWorldType(Type worldType)
        {
            if (worldType == null)
            {
                throw new ArgumentNullException(nameof(worldType));
            }

            if (typeof(GameplayWorld).IsAssignableFrom(worldType) == false)
            {
                throw new ArgumentException($"Type '{worldType.FullName}' must derive from {nameof(GameplayWorld)}.", nameof(worldType));
            }

            if (worldType.IsAbstract)
            {
                throw new ArgumentException($"Gameplay world type '{worldType.FullName}' must not be abstract.", nameof(worldType));
            }

            if (worldType.ContainsGenericParameters)
            {
                throw new ArgumentException($"Gameplay world type '{worldType.FullName}' must not contain generic parameters.", nameof(worldType));
            }
        }

        private static void DestroyWorldGameObject(GameObject gameObject)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
                return;
            }

            DestroyImmediate(gameObject);
        }
    }
}
