using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Ayla
{
    internal class TimerMixerPreloadBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var timerMixerAsset = AssetDatabase.LoadAssetAtPath<TimerMixer>(TimerMixer.kDefaultAssetPath);
            if (timerMixerAsset == null)
            {
                throw new BuildPlayerWindow.BuildMethodException("TimerMixer asset not found at path: " + TimerMixer.kDefaultAssetPath);
            }

            var preloadAssets = PlayerSettings.GetPreloadedAssets().Append(timerMixerAsset).Distinct();
            PlayerSettings.SetPreloadedAssets(preloadAssets.ToArray());
        }
    }
}
