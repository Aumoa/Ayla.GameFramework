#nullable enable

using System;

namespace Ayla
{
    public class ActorComponent
    {
        private readonly Actor m_Owner = Initializer.GetOwner();

        public Actor Owner => m_Owner;

        internal static class Initializer
        {
            private static Actor? s_Owner;

            public static Actor? Setup(Actor owner)
            {
                var previousOwner = s_Owner;
                s_Owner = owner;
                return previousOwner;
            }

            public static void Restore(Actor? owner)
            {
                s_Owner = owner;
            }

            public static Actor GetOwner()
            {
                return s_Owner ?? throw new InvalidOperationException(
                    "ActorComponent must be created through Actor.AddActorComponent.");
            }
        }
    }
}
