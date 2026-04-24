#nullable enable

using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ayla
{
    [Serializable]
    public partial class SceneReference : ISerializationCallbackReceiver, IEquatable<SceneReference>
#if UNITY_EDITOR
        , IEquatable<SceneAsset>
#endif
    {
#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset? m_Asset;
#endif
        [SerializeField]
        private int m_BuildIndex = -1;

        public SceneReference()
        {
        }

        public bool IsValid
        {
            get
            {
                return
#if WITH_ADDRESSABLES
                    !string.IsNullOrWhiteSpace(m_AssetGUID) ||
#endif
                    m_BuildIndex != -1;
            }
        }

        public AssetReferenceType ReferenceType
        {
            get
            {
#if WITH_ADDRESSABLES
                if (!string.IsNullOrWhiteSpace(m_AssetGUID))
                {
                    return AssetReferenceType.SoftReference;
                }
#endif

                return m_BuildIndex != -1 ? AssetReferenceType.Reference : AssetReferenceType.None;
            }
        }

#if UNITY_EDITOR
        public SceneAsset? EditorAsset
        {
            get
            {
#if WITH_ADDRESSABLES
                if (!string.IsNullOrWhiteSpace(m_AssetGUID))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                }
#endif

                return m_Asset;
            }
        }
#endif

        public override bool Equals(object obj)
        {
            if (obj is SceneReference sr)
            {
                return Equals(sr);
            }

#if UNITY_EDITOR
            if (obj is SceneAsset sa)
            {
                return Equals(sa);
            }
#endif

            return false;
        }

        public override int GetHashCode()
        {
            return
#if WITH_ADDRESSABLES
                (m_AssetGUID?.GetHashCode() ?? 0)
#else
                0
#endif
                ^ m_BuildIndex.GetHashCode();
        }

        public bool Equals(SceneReference? sr)
        {
            if (sr == null)
            {
                return false;
            }

            var rt = ReferenceType;
            if (rt != sr.ReferenceType)
            {
                return false;
            }

            switch (rt)
            {
                case AssetReferenceType.None:
                    return true;
                case AssetReferenceType.Reference:
#if UNITY_EDITOR
                    return m_Asset == sr.m_Asset;
#else
                    return m_BuildIndex == sr.m_BuildIndex;
#endif
#if WITH_ADDRESSABLES
                case AssetReferenceType.SoftReference:
                    return m_AssetGUID == sr.m_AssetGUID;
#endif
                default:
                    throw new InvalidOperationException("Invalid enum value");
            }
        }

#if UNITY_EDITOR
        public bool Equals(SceneAsset? sa)
        {
            return EditorAsset == sa;
        }
#endif

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!BuildPipeline.isBuildingPlayer || Application.isPlaying)
            {
                return;
            }

#if WITH_ADDRESSABLES
            if (string.IsNullOrWhiteSpace(m_AssetGUID) == false)
            {
                return;
            }
#endif

            if (m_Asset == null)
            {
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(m_Asset);
            var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            GUID.TryParse(assetGUID, out var guid);

            var scenes = EditorBuildSettings.scenes;
            int buildIndex = -1;
            for (int i = 0; i < scenes.Length; ++i)
            {
                if (scenes[i].enabled && scenes[i].guid == guid)
                {
                    buildIndex = i;
                    break;
                }
            }

            if (m_BuildIndex != buildIndex)
            {
                if (buildIndex == -1)
                {
                    Debug.LogErrorFormat(m_Asset,
                        "[SceneReference] The scene asset '{0}' is not included in the build settings. " +
                        "It will be assigned a BuildIndex of -1. Please add it to the build settings.",
                        m_Asset.name);
                }
                else
                {
                    Debug.LogWarningFormat(m_Asset,
                        "[SceneReference] Scene '{0}' build index mismatch! (Expected: {1}, Current: {2}). " +
                        "The index has been automatically synchronized to {1}.",
                        m_Asset.name, buildIndex, m_BuildIndex);
                }

                m_BuildIndex = buildIndex;
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }

        public SceneReferenceAsyncContext LoadSceneAsync(LoadSceneParameters parameters, bool activateOnLoad, int priority, CancellationToken cancellationToken = default)
        {
#if WITH_ADDRESSABLES
            if (TryLoadSceneByAddressables(parameters, activateOnLoad, priority, cancellationToken, out var context))
            {
                return context;
            }
#endif

            if (m_BuildIndex != -1)
            {
                var asyncOp = SceneManager.LoadSceneAsync(m_BuildIndex, parameters);
                return new SceneReferenceAsyncContext(asyncOp, m_BuildIndex, cancellationToken);
            }

            throw new InvalidOperationException("The scene reference does not contain a valid scene asset. Please check IsValid before attempting to load the scene.");
        }

        public SceneReferenceAsyncContext LoadSceneAsync(LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100, CancellationToken cancellationToken = default)
            => LoadSceneAsync(new LoadSceneParameters(loadSceneMode), activateOnLoad, priority, cancellationToken);

        public static bool operator ==(SceneReference? lhs, SceneReference? rhs)
        {
            return ReferenceEquals(lhs, rhs) || (lhs?.Equals(rhs) == true);
        }

        public static bool operator !=(SceneReference? lhs, SceneReference? rhs)
        {
            return !(lhs == rhs);
        }

#if UNITY_EDITOR
        public static SceneReference FromBuiltInAsset(SceneAsset sceneAsset, int buildIndex)
        {
            return new SceneReference
            {
                m_Asset = sceneAsset,
                m_BuildIndex = buildIndex
            };
        }
#endif
    }
}
