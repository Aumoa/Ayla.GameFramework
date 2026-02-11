using UnityEditor;
using UnityEngine;

namespace Ayla;

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
}
