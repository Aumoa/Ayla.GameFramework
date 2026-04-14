#nullable enable

using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ayla
{
    [Serializable]
    public partial class SceneReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset? m_Asset;
#endif
        [SerializeField]
        private int m_BuildIndex = -1;

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

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!BuildPipeline.isBuildingPlayer)
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
    }
}
