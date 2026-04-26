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
            if (AssetDatabase.AssetPathExists(assetPath) == false)
            {
                throw new ArgumentException();
            }

            Undo.RecordObject(asset, "Add Child");
            var newChannelAsset = ScriptableObject.CreateInstance<TimerChannel>();

            try
            {
                newChannelAsset.name = nameToAdd;
                AssetDatabase.AddObjectToAsset(newChannelAsset, asset);

                if (channel is TimerMixer mixer)
                {
                    var newArray = mixer.m_Channels.Append(newChannelAsset);
                    mixer.m_Channels = newArray.ToArray();
                }
                else
                {
                    var channelAsset = (TimerChannel)channel;
                    var newArray = channelAsset.m_Channels.Append(newChannelAsset);
                    channelAsset.m_Channels = newArray.ToArray();
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
    }
}
