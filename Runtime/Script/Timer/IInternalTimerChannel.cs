using System.Collections.Generic;

namespace Ayla
{
    internal interface IInternalTimerChannel
    {
        void TimeUpdate(double parentTimeScale, double deltaTime, List<IInternalTimerChannel> timeScaleChanged);
    }
}
