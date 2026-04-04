using UnityEditor;
using UnityEngine;

namespace Ayla;

internal static class SceneManagementToolText
{
    private static SystemLanguage s_Language = SystemLanguage.English;

    [InitializeOnLoadMethod]
    private static void StaticAwake()
    {
        s_Language = Application.systemLanguage;
    }

    public static string HotReloadSceneLabel => s_Language switch
    {
        SystemLanguage.Korean => "HotReload 씬",
        SystemLanguage.Japanese => "HotReload シーン",
        _ => "HotReload Scene"
    };

    public static string HotReloadButton => s_Language switch
    {
        SystemLanguage.Korean => "HotReload 씬 로드",
        SystemLanguage.Japanese => "HotReload シーンをロード",
        _ => "Load HotReload Scene"
    };
}
