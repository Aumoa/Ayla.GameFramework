using System;

namespace Ayla;

[AttributeUsage(AttributeTargets.Class)]
internal class GameFrameworkCategoryAttribute : CategoryAttribute
{
    public GameFrameworkCategoryAttribute() : base("GameFramework")
    {
    }
}
