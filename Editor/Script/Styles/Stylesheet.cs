using UnityEngine;

namespace Ayla
{
    internal static class Stylesheet
    {
        public static readonly Color LinkedReferenceColor = new(0.7f, 0.85f, 1.0f);
#if WITH_ADDRESSABLES
        public static readonly Color SoftReferenceColor = new(0.7f, 1.0f, 0.7f);
#endif
    }
}
