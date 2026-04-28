#nullable enable

using UnityEngine;

namespace Ayla
{
    public abstract class TimerMixerComponent : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private TimerChannel? m_Channel;

        public ITimerChannel Channel => (ITimerChannel?)m_Channel ?? TimerMixer.Instance;

        public double TimeScale => ReferenceEquals(m_Channel, null) ? Time.timeScale : m_Channel.TimeScale;

        public double DeltaTime => ReferenceEquals(m_Channel, null) ? Time.deltaTime : m_Channel.DeltaTime;

        private void OnEnable()
        {
            Channel.TimeScaleChanged += OnTimeScaleChanged;
            OnTimeScaleChanged();
        }

        private void OnDisable()
        {
            if (Channel != null)
            {
                Channel.TimeScaleChanged -= OnTimeScaleChanged;
            }
        }

        protected virtual void OnTimeScaleChanged()
        {
        }
    }
}
