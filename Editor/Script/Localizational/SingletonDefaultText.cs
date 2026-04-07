#nullable enable

using UnityEditor;
using UnityEngine;

namespace Ayla
{
    internal static class SingletonDefaultText
    {
        private static SystemLanguage s_Language = SystemLanguage.English;

        [InitializeOnLoadMethod]
        private static void StaticAwake()
        {
            s_Language = Application.systemLanguage;
        }

        public static string Add => s_Language switch
        {
            SystemLanguage.Korean => "추가",
            SystemLanguage.Japanese => "追加",
            _ => "Add"
        };

        public static string NoElementsToAdd => s_Language switch
        {
            SystemLanguage.Korean => "모든 요소가 추가되었습니다.",
            SystemLanguage.Japanese => "すべての要素が追加されました。",
            _ => "All elements have been added."
        };

        public static string MultipleEditCannotSupport => s_Language switch
        {
            SystemLanguage.Korean => "여러 개체 편집은 지원되지 않습니다.",
            SystemLanguage.Japanese => "複数オブジェクトの編集はサポートされていません。",
            _ => "Multiple object editing is not supported."
        };

        public static string Loading => s_Language switch
        {
            SystemLanguage.Korean => "로딩 중...",
            SystemLanguage.Japanese => "読み込み中...",
            _ => "Loading..."
        };

        public static string NotFoundLabel => s_Language switch
        {
            SystemLanguage.Korean => "SingletonDefault 에셋이 경로에 존재하지 않습니다: " + SingletonDefault.kDefaultAssetPath,
            SystemLanguage.Japanese => "SingletonDefault アセットがパスに存在しません: " + SingletonDefault.kDefaultAssetPath,
            _ => "SingletonDefault asset not found at path: " + SingletonDefault.kDefaultAssetPath
        };

        public static string Create => s_Language switch
        {
            SystemLanguage.Korean => "생성",
            SystemLanguage.Japanese => "作成",
            _ => "Create"
        };

        public static string CurrentAssetLabel => s_Language switch
        {
            SystemLanguage.Korean => "현재 에셋",
            SystemLanguage.Japanese => "現在のアセット",
            _ => "Current Asset"
        };

        public static string DataNotFoundLabel => s_Language switch
        {
            SystemLanguage.Korean => "데이터를 찾을 수 없습니다. SceneRootManager에 해당하는 데이터 항목을 생성하세요.",
            SystemLanguage.Japanese => "データが見つかりません。SceneRootManagerに対応するデータ項目を作成してください。",
            _ => "Data not found. Please create a data entry for SceneRootManager."
        };

        public static string InitialSceneLabel => s_Language switch
        {
            SystemLanguage.Korean => "시작 씬",
            SystemLanguage.Japanese => "開始シーン",
            _ => "Start Scene"
        };

        public static string EditorOverrideSceneLabel => s_Language switch
        {
            SystemLanguage.Korean => "에디터용 시작 씬",
            SystemLanguage.Japanese => "エディタ用開始シーン",
            _ => "Editor Start Scene"
        };
    }
}
