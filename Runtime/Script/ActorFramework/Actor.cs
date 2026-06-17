#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Ayla
{
    public class Actor : MonoBehaviour
    {
        private readonly List<ActorComponent> m_ActorComponents = new();

        public T AddActorComponent<T>() where T : ActorComponent
        {
            return (T)AddActorComponent(typeof(T));
        }

        public ActorComponent AddActorComponent(Type componentType)
        {
            ValidateActorComponentCreationType(componentType);

            var component = CreateActorComponent(componentType);
            m_ActorComponents.Add(component);
            return component;
        }

        public T? GetActorComponent<T>() where T : ActorComponent
        {
            foreach (var component in m_ActorComponents)
            {
                if (component is T typedComponent)
                {
                    return typedComponent;
                }
            }

            return null;
        }

        public ActorComponent? GetActorComponent(Type componentType)
        {
            ValidateActorComponentQueryType(componentType);

            foreach (var component in m_ActorComponents)
            {
                if (componentType.IsInstanceOfType(component))
                {
                    return component;
                }
            }

            return null;
        }

        public bool TryGetActorComponent<T>([NotNullWhen(true)] out T? component) where T : ActorComponent
        {
            component = GetActorComponent<T>();
            return component != null;
        }

        public bool TryGetActorComponent(Type componentType, [NotNullWhen(true)] out ActorComponent? component)
        {
            component = GetActorComponent(componentType);
            return component != null;
        }

        public ActorComponent[] GetActorComponents()
        {
            return m_ActorComponents.ToArray();
        }

        public T[] GetActorComponents<T>() where T : ActorComponent
        {
            var components = new List<T>();
            foreach (var component in m_ActorComponents)
            {
                if (component is T typedComponent)
                {
                    components.Add(typedComponent);
                }
            }

            return components.ToArray();
        }

        public ActorComponent[] GetActorComponents(Type componentType)
        {
            ValidateActorComponentQueryType(componentType);

            var components = new List<ActorComponent>();
            foreach (var component in m_ActorComponents)
            {
                if (componentType.IsInstanceOfType(component))
                {
                    components.Add(component);
                }
            }

            return components.ToArray();
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            m_ActorComponents.Clear();
        }

        private ActorComponent CreateActorComponent(Type componentType)
        {
            var previousOwner = ActorComponent.Initializer.Setup(this);
            try
            {
                return (ActorComponent)(Activator.CreateInstance(componentType)
                    ?? throw new InvalidOperationException($"Failed to create actor component '{componentType.FullName}'."));
            }
            finally
            {
                ActorComponent.Initializer.Restore(previousOwner);
            }
        }

        private static void ValidateActorComponentCreationType(Type componentType)
        {
            ValidateActorComponentQueryType(componentType);

            if (componentType.IsAbstract)
            {
                throw new ArgumentException($"Actor component type '{componentType.FullName}' must not be abstract.", nameof(componentType));
            }

            if (componentType.ContainsGenericParameters)
            {
                throw new ArgumentException($"Actor component type '{componentType.FullName}' must not contain generic parameters.", nameof(componentType));
            }
        }

        private static void ValidateActorComponentQueryType(Type componentType)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            if (typeof(ActorComponent).IsAssignableFrom(componentType) == false)
            {
                throw new ArgumentException($"Type '{componentType.FullName}' must derive from {nameof(ActorComponent)}.", nameof(componentType));
            }
        }
    }
}
