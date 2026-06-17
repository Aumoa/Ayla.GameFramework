#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    public class GameplayWorld : MonoBehaviour
    {
        [SerializeField]
        private Transform? m_Root;

        public Transform Root => m_Root != null ? m_Root : transform;

        public T SpawnActor<T>() where T : Actor
        {
            return (T)SpawnActor(typeof(T));
        }

        public T SpawnActor<T>(string name) where T : Actor
        {
            return (T)SpawnActor(typeof(T), name);
        }

        public T SpawnActor<T>(Vector3 localPosition, Quaternion localRotation) where T : Actor
        {
            return (T)SpawnActor(typeof(T), typeof(T).Name, localPosition, localRotation);
        }

        public T SpawnActor<T>(string name, Vector3 localPosition, Quaternion localRotation) where T : Actor
        {
            return (T)SpawnActor(typeof(T), name, localPosition, localRotation);
        }

        public Actor SpawnActor(Type actorType)
        {
            return SpawnActor(actorType, actorType?.Name ?? nameof(Actor));
        }

        public Actor SpawnActor(Type actorType, string name)
        {
            return SpawnActor(actorType, name, Vector3.zero, Quaternion.identity);
        }

        public Actor SpawnActor(Type actorType, Vector3 localPosition, Quaternion localRotation)
        {
            return SpawnActor(actorType, actorType?.Name ?? nameof(Actor), localPosition, localRotation);
        }

        public Actor SpawnActor(Type actorType, string name, Vector3 localPosition, Quaternion localRotation)
        {
            ValidateActorType(actorType);

            var gameObject = new GameObject(name);
            var actorTransform = gameObject.transform;
            actorTransform.SetParent(Root, false);
            actorTransform.SetLocalPositionAndRotation(localPosition, localRotation);

            return (Actor)gameObject.AddComponent(actorType);
        }

        public T SpawnActor<T>(T prefab) where T : Actor
        {
            return SpawnActor(prefab, Vector3.zero, Quaternion.identity);
        }

        public T SpawnActor<T>(T prefab, Vector3 localPosition, Quaternion localRotation) where T : Actor
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            var actor = Instantiate(prefab, Root);
            actor.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            return actor;
        }

        public Actor SpawnActor(GameObject prefab)
        {
            return SpawnActor(prefab, Vector3.zero, Quaternion.identity);
        }

        public Actor SpawnActor(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            var instance = Instantiate(prefab, Root);
            var actor = instance.GetComponent<Actor>();
            if (actor == null)
            {
                DestroySpawnedInstance(instance);
                throw new InvalidOperationException($"Prefab '{prefab.name}' must contain an {nameof(Actor)} on its root GameObject.");
            }

            instance.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            return actor;
        }

        private static void ValidateActorType(Type actorType)
        {
            if (actorType == null)
            {
                throw new ArgumentNullException(nameof(actorType));
            }

            if (typeof(Actor).IsAssignableFrom(actorType) == false)
            {
                throw new ArgumentException($"Type '{actorType.FullName}' must derive from {nameof(Actor)}.", nameof(actorType));
            }

            if (actorType.IsAbstract)
            {
                throw new ArgumentException($"Actor type '{actorType.FullName}' must not be abstract.", nameof(actorType));
            }

            if (actorType.ContainsGenericParameters)
            {
                throw new ArgumentException($"Actor type '{actorType.FullName}' must not contain generic parameters.", nameof(actorType));
            }
        }

        private static void DestroySpawnedInstance(GameObject instance)
        {
            if (Application.isPlaying)
            {
                Destroy(instance);
                return;
            }

            DestroyImmediate(instance);
        }
    }
}
