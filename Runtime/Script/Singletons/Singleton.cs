#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla
{
    public abstract class Singleton : ScriptableObject
    {
        internal readonly struct ConstructorArguments
        {
            public readonly SingletonManager Owner;
            public readonly SingletonDefault Data;

            public ConstructorArguments(SingletonManager owner, SingletonDefault data)
            {
                Owner = owner;
                Data = data;
            }
        }

        internal static class ConstructorContext
        {
            public static ThreadLocal<ConstructorArguments> Args = new();

            public static void Begin(in ConstructorArguments args)
            {
                Args.Value = args;
            }

            public static void End()
            {
                Args.Value = default;
            }
        }

        public readonly SingletonManager Manager = ConstructorContext.Args.Value.Owner;

        public virtual ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

        public virtual ValueTask PostInitializeAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

        public virtual ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

        public virtual ValueTask OnEvent(int eventId, CancellationToken cancellationToken = default)
        {
            return default;
        }

        public static Type? GetDataType(Type t)
        {
            if (!t.IsAssignableTo(typeof(Singleton)))
            {
                return null;
            }

            var dataSingleton = typeof(Singleton<,>);
            if (!t.IsImplements(dataSingleton))
            {
                return null;
            }

            while (t.IsGenericType == false || t.GetGenericTypeDefinition() != dataSingleton)
            {
                t = t.BaseType;
            }

            return t.GetGenericArguments()[1];
        }
    }

    public abstract class Singleton<TSingleton> : Singleton, IDisposable
        where TSingleton : Singleton
    {
        private static TSingleton? s_Instance;

        public static TSingleton Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    throw new InvalidOperationException($"Singleton instance of type {typeof(TSingleton)} is not initialized.");
                }

                return s_Instance;
            }
        }

        public static bool TryGetInstance([NotNullWhen(true)] out TSingleton? instance)
        {
            instance = s_Instance;
            return s_Instance != null;
        }

        ~Singleton()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected virtual void OnEnable()
        {
            Asserts.True(s_Instance == null || s_Instance == this, $"Singleton instance of type {typeof(TSingleton)} is already initialized.");
            s_Instance = (TSingleton)(object)this;
        }

        protected virtual void OnDisable()
        {
        }
    }

    public abstract class Singleton<TSingleton, TData> : Singleton<TSingleton>
        where TSingleton : Singleton
        where TData : SingletonData
    {
        [SerializeField]
        private TData? m_Data;

        public TData Data => m_Data
            ?? throw new InvalidOperationException($"Singleton data of type {typeof(TData)} is not initialized.");

        public Singleton()
        {
#if UNITY_EDITOR
            try
            {
#endif
                m_Data = (TData?)ConstructorContext.Args.Value.Data.GetData(this)
                    ?? throw new InvalidOperationException($"Singleton data of type {typeof(TData)} is not found in SingletonDefault.");
#if UNITY_EDITOR
            }
            catch (NullReferenceException)
            {
            }
#endif
        }
    }
}