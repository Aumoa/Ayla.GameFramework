using System;
using UnityEngine;

namespace Ayla
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class RequiredComponentTypeAttribute : PropertyAttribute
    {
        public readonly Type[] ComponentTypes;

        public RequiredComponentTypeAttribute(params Type[] componentTypes)
        {
            ComponentTypes = componentTypes;
        }
    }
}
