#nullable enable

using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
        private static bool s_Registered;

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
                m_Time = 0;
                m_DeltaTime = 0;
                m_RuntimeTimeScale = UnityEngine.Time.timeScale;
                Register();
            }
        }

        private void OnDisable()
        {
            if (ReferenceEquals(s_Mixer, this))
            {
                Unregister();
                s_Mixer = null;
            }

#if !UNITY_EDITOR
            if (ReferenceEquals(s_PreloadedAsset, this))
            {
                s_PreloadedAsset = null;
            }
#endif
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
                if (s_Mixer == null)
                {
                    return;
                }

                using var scope1 = ListPool<IInternalTimerChannel>.Get(out var timeScaleChanged);
                ((IInternalTimerChannel)s_Mixer).TimeUpdate(1.0, (double)UnityEngine.Time.deltaTime, timeScaleChanged);
            }
        }

        private static void Register()
        {
            var system = PlayerLoop.GetCurrentPlayerLoop();
            RemoveFromPlayerLoop(ref system);

            if (TryAddToTimeUpdate(ref system) == false)
            {
                Debug.LogError("Failed to register TimerMixer update in Unity PlayerLoop.");
                s_Registered = false;
                return;
            }

            PlayerLoop.SetPlayerLoop(system);
            s_Registered = true;
        }

        private static void Unregister()
        {
            if (s_Registered == false)
            {
                return;
            }

            var system = PlayerLoop.GetCurrentPlayerLoop();
            if (RemoveFromPlayerLoop(ref system))
            {
                PlayerLoop.SetPlayerLoop(system);
            }
            s_Registered = false;
        }

        private static bool TryAddToTimeUpdate(ref PlayerLoopSystem system)
        {
            if (system.type == typeof(TimeUpdate))
            {
                var oldArray = system.subSystemList ?? Array.Empty<PlayerLoopSystem>();
                var newArray = new PlayerLoopSystem[oldArray.Length + 1];
                Array.Copy(oldArray, newArray, oldArray.Length);
                newArray[oldArray.Length] = new PlayerLoopSystem
                {
                    type = typeof(TimeUpdateExecutor),
                    updateDelegate = TimeUpdateExecutor.Call
                };
                system.subSystemList = newArray;
                return true;
            }

            if (system.subSystemList == null)
            {
                return false;
            }

            for (int i = 0; i < system.subSystemList.Length; ++i)
            {
                if (TryAddToTimeUpdate(ref system.subSystemList[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RemoveFromPlayerLoop(ref PlayerLoopSystem system)
        {
            if (system.subSystemList == null)
            {
                return false;
            }

            var subSystemList = system.subSystemList;
            bool changed = false;
            int writeIndex = 0;
            for (int readIndex = 0; readIndex < subSystemList.Length; ++readIndex)
            {
                var subSystem = subSystemList[readIndex];
                if (subSystem.type == typeof(TimeUpdateExecutor))
                {
                    changed = true;
                    continue;
                }

                if (RemoveFromPlayerLoop(ref subSystem))
                {
                    changed = true;
                }

                subSystemList[writeIndex++] = subSystem;
            }

            if (changed)
            {
                if (writeIndex != subSystemList.Length)
                {
                    Array.Resize(ref subSystemList, writeIndex);
                }

                system.subSystemList = subSystemList;
            }

            return changed;
        }
    }
}
