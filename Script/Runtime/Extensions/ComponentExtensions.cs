using System;
using UnityEngine;

namespace Ayla;

public static class ComponentExtensions
{
    public static Transform MakeChild(this Component component, string name)
    {
        return component.transform.MakeChild(name);
    }

    public static Transform MakeChild(this Transform transform, string name, params Type[] componentTypes)
    {
        var gameObject = new GameObject(name, componentTypes);
        var transform1 = gameObject.transform;
        transform1.SetParent(transform, false);
        return transform1;
    }

    public static T MakeChild<T>(this Component component, string name) where T : Component
    {
        return component.transform.MakeChild<T>(name);
    }

    public static T MakeChild<T>(this Transform transform, string name) where T : Component
    {
        var transform1 = transform.MakeChild(name, typeof(T));
        return transform1.GetComponent<T>();
    }
}
