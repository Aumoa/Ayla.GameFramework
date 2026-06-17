#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    public class ActorComponent : MonoBehaviour
    {
        internal static class Initializer
        {
            public static Actor? s_Owner;

            public static void Setup(Actor owner)
            {
                s_Owner = owner;
            }
        }

        private readonly Actor? m_Owner = Initializer.s_Owner;

        public Actor Owner => m_Owner ?? throw new InvalidOperationException("ActorComponent must be initialized with an Actor.");
    }
}
