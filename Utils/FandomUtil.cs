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
    }
}
