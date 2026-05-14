using UnityEngine;

public static class Utility
{
    /// <summary>
    /// 초 단위 시간을 분:초로 변환.
    /// </summary>
    /// <param name="timeSeconds">초 단위 시간</param>
    /// <returns></returns>
    public static string FormatTime(float timeSeconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        string formattedTime = minutes.ToString("00") + ":" + seconds.ToString("00");
        return formattedTime;
    }
}
