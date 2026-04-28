#nullable enable

using UnityEngine;

namespace Ayla
{
    public abstract class TimerMixerComponent : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private TimerChannel? m_Channel;

        protected double TimeScale => ReferenceEquals(m_Channel, null) ? Time.timeScale : m_Channel.TimeScale;

        protected double DeltaTime => ReferenceEquals(m_Channel, null) ? Time.deltaTime : m_Channel.DeltaTime;
    }
}
