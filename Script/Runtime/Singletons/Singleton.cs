using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ayla;

public abstract class Singleton : MonoBehaviour
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

        var dataSingleton = typeof(Singleton<>);
        if (!t.IsImplements(dataSingleton))
        {
            return null;
        }

        while (t.IsGenericType == false || t.GetGenericTypeDefinition() != dataSingleton)
        {
            t = t.BaseType;
        }

        return t.GetGenericArguments()[0];
    }
}

public abstract class Singleton<TData> : Singleton
    where TData : SingletonData
{
    public readonly TData Data;

    public Singleton()
    {
        Data = (TData)ConstructorContext.Args.Value.Data.GetData(this);
    }
}