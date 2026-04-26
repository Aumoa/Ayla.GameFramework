#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Ayla
{
    public class TimerMixer : ScriptableObject, ITimerChannel, IInternalTimerChannel
    {
        public const string kDefaultAssetPath = "Assets/Settings/TimerMixer.asset";

        private static TimerMixer? s_Mixer;

        [SerializeField]
        internal TimerChannel[] m_Channels = Array.Empty<TimerChannel>();

        private double m_Time;
        private double m_DeltaTime;

        public ITimerChannel? Parent => null;

        public IReadOnlyList<ITimerChannel> Children => m_Channels;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public double TimeScale => Application.isPlaying ? UnityEngine.Time.timeScale : 1;

        public double SelfTimeScale => TimeScale;

        public double DeltaTime => m_DeltaTime;

        public double Time => m_Time;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Asserts.Equals(s_Mixer, null);
                s_Mixer = this;
                Register();
            }
        }

        void IInternalTimerChannel.TimeUpdate(double deltaTime)
        {
            m_DeltaTime = deltaTime;
            m_Time += m_DeltaTime;

            foreach (var channel in m_Channels)
            {
                ((IInternalTimerChannel)channel).TimeUpdate(m_DeltaTime);
            }
        }

        private readonly struct TimeUpdateExecutor
        {
            public static void Call()
            {
                ((IInternalTimerChannel)s_Mixer!).TimeUpdate((double)UnityEngine.Time.deltaTime);
            }
        }

        private static void Register()
        {
            var system = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < system.subSystemList.Length; ++i)
            {
                ref var s = ref system.subSystemList[i];
                if (s.type == typeof(TimeUpdate))
                {
                    var oldArray = system.subSystemList;
                    var newArray = new PlayerLoopSystem[oldArray.Length + 1];
                    Array.Copy(oldArray, newArray, oldArray.Length);
                    newArray[oldArray.Length] = new PlayerLoopSystem
                    {
                        type = typeof(TimeUpdateExecutor),
                        updateDelegate = TimeUpdateExecutor.Call
                    };
                    system.subSystemList = newArray;
                    break;
                }
            }
            PlayerLoop.SetPlayerLoop(system);
        }
    }
}
