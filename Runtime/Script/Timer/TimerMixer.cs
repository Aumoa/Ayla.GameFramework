#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Pool;

namespace Ayla
{
    public class TimerMixer : ScriptableObject, ITimerChannel, IInternalTimerChannel
    {
        public const string kDefaultAssetPath = "Assets/Settings/TimerMixer.asset";

        private static TimerMixer? s_Mixer;

        [SerializeField]
        internal TimerChannel[] m_Channels = Array.Empty<TimerChannel>();

#if !UNITY_EDITOR
        private static TimerMixer? s_PreloadedAsset;

        private static TimerMixer PreloadedAsset => s_PreloadedAsset != null ? s_PreloadedAsset : throw new InvalidOperationException("TimerMixer asset is not preloaded.");
#endif

        public static TimerMixer Instance
        {
            get
            {
#if UNITY_EDITOR
                return AssetDatabase.LoadAssetAtPath<TimerMixer>(kDefaultAssetPath);
#else
                return PreloadedAsset;
#endif
            }
        }

        private double m_Time;
        private double m_DeltaTime;
        private double m_RuntimeTimeScale = 1;

        public event Action? TimeScaleChanged;

        public ITimerChannel? Parent => null;

        public IReadOnlyList<ITimerChannel> Children => m_Channels;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public double SelfTimeScale => Application.isPlaying ? m_RuntimeTimeScale : 1.0;

        public double TimeScale => SelfTimeScale;

        public double DeltaTime => m_DeltaTime;

        public double Time => m_Time;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode)
#endif
            {
#if !UNITY_EDITOR
                s_PreloadedAsset = this;
#endif

                Asserts.Equals(s_Mixer, null);
                s_Mixer = this;
                Register();
            }
        }

        void IInternalTimerChannel.TimeUpdate(double parentTimeScale, double deltaTime, List<IInternalTimerChannel> timeScaleChanged)
        {
            m_DeltaTime = deltaTime;
            m_Time += m_DeltaTime;
            m_RuntimeTimeScale = UnityEngine.Time.timeScale;

            if (m_RuntimeTimeScale != parentTimeScale)
            {
                timeScaleChanged.Add(this);
            }

            foreach (var channel in m_Channels)
            {
                ((IInternalTimerChannel)channel).TimeUpdate(m_RuntimeTimeScale, m_DeltaTime, timeScaleChanged);
            }

            foreach (var target in timeScaleChanged)
            {
                switch (target)
                {
                    case TimerMixer tm:
                        tm.TimeScaleChanged?.Invoke();
                        break;
                    case TimerChannel tc:
                        tc.InvokeTimeScaleChanged();
                        break;
                }
            }
        }

        private readonly struct TimeUpdateExecutor
        {
            public static void Call()
            {
                using var scope1 = ListPool<IInternalTimerChannel>.Get(out var timeScaleChanged);
                ((IInternalTimerChannel)s_Mixer!).TimeUpdate(1.0, (double)UnityEngine.Time.deltaTime, timeScaleChanged);
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
