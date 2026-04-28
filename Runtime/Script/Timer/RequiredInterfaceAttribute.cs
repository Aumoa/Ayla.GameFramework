using System;
using UnityEngine;

namespace Ayla
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class RequiredInterfaceAttribute : PropertyAttribute
    {
        public readonly Type InterfaceType;

        public RequiredInterfaceAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}
