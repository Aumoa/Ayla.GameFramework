#nullable enable

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    public static class TimerMixerUtility
    {
        public static TimerChannel AddChild(ITimerChannel channel, string nameToAdd)
        {
            var asset = (Object)channel;
            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (!AssetDatabase.AssetPathExists(assetPath))
            {
                throw new ArgumentException("Channel asset path does not exist.", nameof(channel));
            }

            Undo.RecordObject(asset, "Add Child");
            var newChannelAsset = ScriptableObject.CreateInstance<TimerChannel>();

            try
            {
                newChannelAsset.name = nameToAdd;
                newChannelAsset.m_Parent = (ScriptableObject)asset;
                AssetDatabase.AddObjectToAsset(newChannelAsset, asset);

                if (channel is TimerMixer mixer)
                {
                    mixer.m_Channels = mixer.m_Channels.Append(newChannelAsset).ToArray();
                }
                else
                {
                    ((TimerChannel)channel).m_Channels = ((TimerChannel)channel).m_Channels.Append(newChannelAsset).ToArray();
                }

                return newChannelAsset;
            }
            catch
            {
                Object.DestroyImmediate(newChannelAsset);
                throw;
            }
            finally
            {
                EditorUtility.SetDirty(asset);
            }
        }

        public static void RemoveChannel(ITimerChannel parent, ITimerChannel channel)
        {
            if (channel is TimerMixer)
            {
                throw new ArgumentException("The root TimerMixer channel cannot be removed.", nameof(channel));
            }

            var channelAsset = (TimerChannel)channel;
            var parentAsset = (Object)parent;

            Undo.RecordObject(parentAsset, "Remove Channel");

            if (parent is TimerMixer mixer)
            {
                mixer.m_Channels = mixer.m_Channels.Where(c => c != channelAsset).ToArray();
            }
            else
            {
                ((TimerChannel)parent).m_Channels = ((TimerChannel)parent).m_Channels.Where(c => c != channelAsset).ToArray();
            }

            EditorUtility.SetDirty(parentAsset);
            DestroySubAssetsRecursive(channelAsset);
            AssetDatabase.SaveAssets();
        }

        private static void DestroySubAssetsRecursive(TimerChannel channel)
        {
            foreach (var child in channel.m_Channels)
            {
                DestroySubAssetsRecursive(child);
            }
            Undo.DestroyObjectImmediate(channel);
        }
    }
}
#endif
