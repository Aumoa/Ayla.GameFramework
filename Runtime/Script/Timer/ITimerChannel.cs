#nullable enable

using System.Collections.Generic;

namespace Ayla
{
    public interface ITimerChannel
    {
        ITimerChannel? Parent { get; }
        IReadOnlyList<ITimerChannel> Children { get; }

        string Name { get; set; }

        double TimeScale { get; }
        double SelfTimeScale { get; }
        double DeltaTime { get; }
        double Time { get;}
    }
}
