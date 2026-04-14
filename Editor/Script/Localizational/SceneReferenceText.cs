using UnityEditor;
using UnityEngine;

namespace Ayla
{
    internal static class SceneReferenceText
    {
        private static SystemLanguage s_Language = SystemLanguage.English;

        [InitializeOnLoadMethod]
        private static void StaticAwake()
        {
            s_Language = Application.systemLanguage;
        }

        public static string AddToBuildSceneTitle => s_Language switch
        {
            SystemLanguage.Korean => "씬 참조 오류",
            SystemLanguage.Japanese => "シーン参照エラー",
            _ => "Scene Reference Error"
        };

        public static string AddToBuildSceneMessage => s_Language switch
        {
            SystemLanguage.Korean => "씬이 빌드 설정에 추가되어 있지 않습니다.\n빌드 설정에 추가하시겠습니까?",
            SystemLanguage.Japanese => "シーンがビルド設定に追加されていません。\nビルド設定に追加しますか？",
            _ => "The scene is not added to the build settings.\nDo you want to add it to the build settings?"
        };

        public static string Confirm => s_Language switch
        {
            SystemLanguage.Korean => "확인",
            SystemLanguage.Japanese => "確認",
            _ => "Confirm"
        };

        public static string Cancel => s_Language switch
        {
            SystemLanguage.Korean => "취소",
            SystemLanguage.Japanese => "キャンセル",
            _ => "Cancel"
        };
    }
}
