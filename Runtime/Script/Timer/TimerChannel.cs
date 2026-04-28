#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ayla
{
    public class TimerChannel : ScriptableObject, ITimerChannel, IInternalTimerChannel
    {
        [SerializeField]
        internal TimerChannel[] m_Channels = Array.Empty<TimerChannel>();
        [SerializeField]
        private double m_TimeScale = 1;
        [SerializeField]
        internal ScriptableObject m_Parent = null!;

        private double m_Time;
        private double m_DeltaTime;
        private double m_RuntimeTimeScale = 1;

        public event Action? TimeScaleChanged;

        public ITimerChannel Parent => (ITimerChannel)m_Parent;

        public IReadOnlyList<ITimerChannel> Children => m_Channels;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public double SelfTimeScale => m_TimeScale;

        public double TimeScale =>
#if UNITY_EDITOR
            Application.isPlaying ?
#endif
            m_RuntimeTimeScale
#if UNITY_EDITOR
            : Parent.TimeScale * SelfTimeScale
#endif
            ;

        public double DeltaTime => m_DeltaTime;

        public double Time => m_Time;

        void IInternalTimerChannel.TimeUpdate(double parentTimeScale, double deltaTime, List<IInternalTimerChannel> timeScaleChanged)
        {
            m_DeltaTime = deltaTime * m_TimeScale;
            m_Time += m_DeltaTime;

            var timeScale = parentTimeScale * m_TimeScale;
            if (timeScale != m_RuntimeTimeScale)
            {
                m_RuntimeTimeScale = timeScale;
                timeScaleChanged.Add(this);
            }

            foreach (var channel in m_Channels)
            {
                ((IInternalTimerChannel)channel).TimeUpdate(m_RuntimeTimeScale, m_DeltaTime, timeScaleChanged);
            }
        }

        internal void InvokeTimeScaleChanged()
        {
            TimeScaleChanged?.Invoke();
        }
    }
}
