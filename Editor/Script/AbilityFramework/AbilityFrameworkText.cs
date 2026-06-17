#nullable enable

using UnityEngine;

namespace Ayla
{
    internal static class AbilityFrameworkText
    {
        public static string NotFoundLabel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "Ability Framework 에셋이 경로에 존재하지 않습니다: " + AbilityFramework.kDefaultAssetPath,
            SystemLanguage.Japanese => "Ability Framework アセットがパスに存在しません: " + AbilityFramework.kDefaultAssetPath,
            _ => "Ability Framework asset not found at path: " + AbilityFramework.kDefaultAssetPath
        };

        public static string Create => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "Ability Framework 에셋 생성",
            SystemLanguage.Japanese => "Ability Framework アセットを作成",
            _ => "Create Ability Framework Asset"
        };

        public static string CurrentAssetLabel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "현재 Ability Framework",
            SystemLanguage.Japanese => "現在の Ability Framework",
            _ => "Current Ability Framework"
        };
    }
}
