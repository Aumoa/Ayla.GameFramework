using Unity.Entities;

#nullable enable

namespace Ayla
{
    public static class EntityWorldHelper
    {
        public static World GetOrCreateDefaultWorld()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || world.IsCreated == false)
            {
                world = DefaultWorldInitialization.Initialize("Default World", false);
            }

            return world;
        }
    }
}
