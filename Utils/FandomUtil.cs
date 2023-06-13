using System.Text.RegularExpressions;

namespace MCSounds.Utils
{
    public static class FandomUtil
    {
        /// <summary>
        /// 使用正则表达式获取Fandom中Url里的文件名
        /// </summary>
        public static string GetUrlFileName(string url)
        {
            string fileName = string.Empty;
            Match match = Regex.Match(url, @"\/([^\/]+?\.[^\/]+)\/revision");
            if (match.Success)
            {
                fileName = match.Groups[1].Value;
            }
            return fileName;
        }
        /// <summary>
        /// 通过URL获取音频名字
        /// </summary>
        /// <param name="url">音频路径</param>
        /// <returns>
        /// 输入: .../images/b/bc/Villager_idle2.ogg/revision...
        /// <br/>
        /// 输出: Villager_idle
        /// </returns>
        public static string GetSoundName(string url)
        {
            string input = Path.GetFileNameWithoutExtension(GetUrlFileName(url));
            return Regex.Replace(input, @"\d*\b", "");
        }
    }
}
