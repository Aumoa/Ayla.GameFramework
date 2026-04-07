using UnityEditor;
using UnityEngine;

namespace Ayla
{
    internal static class TimerMixerText
    {
        private static SystemLanguage s_Language = SystemLanguage.English;

        [InitializeOnLoadMethod]
        private static void StaticAwake()
        {
            s_Language = Application.systemLanguage;
        }

        public static string NotFoundLabel => s_Language switch
        {
            SystemLanguage.Korean => "Timer Mixer 에셋이 경로에 존재하지 않습니다: " + TimerMixer.kDefaultAssetPath,
            SystemLanguage.Japanese => "Timer Mixer アセットがパスに存在しません: " + TimerMixer.kDefaultAssetPath,
            _ => "Timer Mixer asset not found at path: " + TimerMixer.kDefaultAssetPath
        };

        public static string Create => s_Language switch
        {
            SystemLanguage.Korean => "Timer Mixer 에셋 생성",
            SystemLanguage.Japanese => "Timer Mixer アセットを作成",
            _ => "Create Timer Mixer Asset"
        };

        public static string CurrentAssetLabel => s_Language switch
        {
            SystemLanguage.Korean => "현재 Timer Mixer",
            SystemLanguage.Japanese => "現在の Timer Mixer",
            _ => "Current Timer Mixer"
        };
    }
}
