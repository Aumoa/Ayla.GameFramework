#nullable enable

using UnityEngine;

namespace Ayla
{
    internal static class AssetReferenceStylesheet
    {
        public static readonly Color LinkedReferenceColor = new(0.7f, 0.85f, 1.0f);
#if WITH_ADDRESSABLES
        public static readonly Color SoftReferenceColor = new(0.7f, 1.0f, 0.7f);
#endif

        public static readonly Color ToolbarColor = new(0.24f, 0.24f, 0.24f);
        public static readonly Color BorderColor = new(0.14f, 0.14f, 0.14f);
        public static readonly Color MixerGroupPanelColor = new(0.19f, 0.19f, 0.19f);
        public static readonly Color MixerContentPanelColor = new(0.18f, 0.18f, 0.18f);
    }
}
