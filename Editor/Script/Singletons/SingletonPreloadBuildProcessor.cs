using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Ayla
{
    internal class SingletonPreloadBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var singletonDefaultAsset = AssetDatabase.LoadAssetAtPath<SingletonDefault>(SingletonDefault.kDefaultAssetPath);
            if (singletonDefaultAsset == null)
            {
                throw new BuildPlayerWindow.BuildMethodException("SingletonDefault asset not found at path: " + SingletonDefault.kDefaultAssetPath);
            }

            var preloadAssets = PlayerSettings.GetPreloadedAssets().Append(singletonDefaultAsset).Distinct();
            PlayerSettings.SetPreloadedAssets(preloadAssets.ToArray());
        }
    }
}
