using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Ayla
{
    public class UnityTimerComponentConnector : MonoBehaviour, ITimerPlayableComponent
    {
        [SerializeField]
        [RequiredComponentType(typeof(Animator), typeof(PlayableDirector), typeof(ParticleSystem))]
        private List<Component> m_Targets = new();

#if UNITY_EDITOR
        private bool m_ExitingPlayMode = false;
#endif

#if UNITY_EDITOR
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                m_ExitingPlayMode = true;
            }
        }
#endif

        public void AddTarget(PlayableDirector playableDirector)
        {
            InternalAddTarget(playableDirector);
        }

        public void AddTarget(Animator animator)
        {
            InternalAddTarget(animator);
        }

        public void AddTarget(ParticleSystem particleSystem)
        {
            InternalAddTarget(particleSystem);
        }

        public void RemoveTarget(Component target)
        {
            m_Targets.Remove(target);
        }

        private void InternalAddTarget(Component component)
        {
            Asserts.True(component is PlayableDirector);
            Asserts.True(component is Animator);
            Asserts.True(component is ParticleSystem);
            m_Targets.Add(component);
        }

        public double TimeScale
        {
            set
            {
                foreach (var target in m_Targets)
                {
                    switch (target)
                    {
                        case PlayableDirector pd:
#if UNITY_EDITOR
                            if (!m_ExitingPlayMode)
#endif
                            {
                                pd.playableGraph.GetRootPlayable(0).SetSpeed(value);
                            }
                            break;
                        case Animator a:
                            a.speed = (float)value;
                            break;
                        case ParticleSystem ps:
                            var main = ps.main;
                            main.simulationSpeed = (float)value;
                            break;
                    }
                }

            }
        }
    }
}
