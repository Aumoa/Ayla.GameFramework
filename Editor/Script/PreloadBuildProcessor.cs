using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Ayla
{
    internal class PreloadBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var singletonDefaultAsset = AssetDatabase.LoadAssetAtPath<SingletonDefault>(SingletonDefault.kDefaultAssetPath);
            if (singletonDefaultAsset == null)
            {
                throw new BuildPlayerWindow.BuildMethodException("SingletonDefault asset not found at path: " + SingletonDefault.kDefaultAssetPath);
            }

            var timerMixerAsset = AssetDatabase.LoadAssetAtPath<TimerMixer>(TimerMixer.kDefaultAssetPath);
            if (timerMixerAsset == null)
            {
                throw new BuildPlayerWindow.BuildMethodException("TimerMixer asset not found at path: " + TimerMixer.kDefaultAssetPath);
            }

            var preloadAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadAssets.Add(singletonDefaultAsset);
            preloadAssets.Add(timerMixerAsset);
            PlayerSettings.SetPreloadedAssets(preloadAssets.ToArray());
        }
    }
}
