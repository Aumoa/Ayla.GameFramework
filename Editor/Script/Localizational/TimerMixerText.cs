using UnityEngine;

namespace Ayla
{
    internal static class TimerMixerText
    {
        public static string NotFoundLabel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "Timer Mixer 에셋이 경로에 존재하지 않습니다: " + TimerMixer.kDefaultAssetPath,
            SystemLanguage.Japanese => "Timer Mixer アセットがパスに存在しません: " + TimerMixer.kDefaultAssetPath,
            _ => "Timer Mixer asset not found at path: " + TimerMixer.kDefaultAssetPath
        };

        public static string Create => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "Timer Mixer 에셋 생성",
            SystemLanguage.Japanese => "Timer Mixer アセットを作成",
            _ => "Create Timer Mixer Asset"
        };

        public static string CurrentAssetLabel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "현재 Timer Mixer",
            SystemLanguage.Japanese => "現在の Timer Mixer",
            _ => "Current Timer Mixer"
        };

        public static string OpenEditor => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "Timer Mixer 에디터 열기",
            SystemLanguage.Japanese => "Timer Mixer エディタを開く",
            _ => "Open Timer Mixer Editor"
        };

        public static string NoAssetText => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "Timer Mixer 에셋이 존재하지 않습니다. 생성 버튼을 눌러 생성하세요.",
            SystemLanguage.Japanese => "Timer Mixer アセットが存在しません。作成ボタンを押して作成してください。",
            _ => "No Timer Mixer asset exists. Please click the create button to create one."
        };

        public static string AddChild => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "자식 채널 추가",
            SystemLanguage.Japanese => "子チャンネルを追加",
            _ => "Add Child Channel"
        };

        public static string RenameChannel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "채널 이름 변경",
            SystemLanguage.Japanese => "チャンネル名を変更",
            _ => "Rename Channel"
        };

        public static string DeleteChannel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "채널 삭제",
            SystemLanguage.Japanese => "チャンネルを削除",
            _ => "Delete Channel"
        };

        public static string TimeScaleLabel => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "타임 스케일",
            SystemLanguage.Japanese => "タイムスケール",
            _ => "Time Scale"
        };

        public static string FinalHeader => ApplicationMisc.EditorLanguage switch
        {
            SystemLanguage.Korean => "최종",
            SystemLanguage.Japanese => "最終",
            _ => "Final"
        };
    }
}
