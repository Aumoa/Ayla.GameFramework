#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    public class TimerPlayableUpdateComponent : TimerMixerComponent
    {
        [RequiredInterface(typeof(ITimerPlayableComponent))]
        public Component[] Targets = Array.Empty<Component>();

        protected override void OnTimeScaleChanged()
        {
            var timeScale = TimeScale;
            foreach (var target in Targets)
            {
                ((ITimerPlayableComponent)target).OnTimeScaleUpdated(timeScale);
            }
        }
    }
}
