using System;
using UnityEngine;

namespace Ayla
{
    public class TimerPlayableUpdateComponent : TimerMixerComponent
    {
        [RequiredInterface(typeof(ITimerPlayableComponent))]
        public Component[] Targets = Array.Empty<Component>();

        protected void Update()
        {
            foreach (var target in Targets)
            {
                ((ITimerPlayableComponent)target).TimeScale = TimeScale;
            }
        }
    }
}
