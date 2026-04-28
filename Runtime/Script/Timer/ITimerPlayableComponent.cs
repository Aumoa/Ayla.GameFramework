#nullable enable

namespace Ayla
{
    public interface ITimerPlayableComponent
    {
        double TimeScale { set; }

        void OnTimeScaleUpdated(double timeScale);
    }
}
