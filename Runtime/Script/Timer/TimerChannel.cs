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
        internal ScriptableObject? m_Parent;

        private double m_Time;
        private double m_DeltaTime;

        public ITimerChannel? Parent => m_Parent as ITimerChannel;

        public IReadOnlyList<ITimerChannel> Children => m_Channels;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public double SelfTimeScale => m_TimeScale;

        public double TimeScale => (Parent?.TimeScale ?? 1.0) * SelfTimeScale;

        public double DeltaTime => m_DeltaTime;

        public double Time => m_Time;

        void IInternalTimerChannel.TimeUpdate(double deltaTime)
        {
            m_DeltaTime = deltaTime * m_TimeScale;
            m_Time += m_DeltaTime;

            foreach (var channel in m_Channels)
            {
                ((IInternalTimerChannel)channel).TimeUpdate(m_DeltaTime);
            }
        }
    }
}
