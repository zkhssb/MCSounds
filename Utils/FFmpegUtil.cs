using ColoredConsole;
using System.Diagnostics;
using System.IO;

namespace MCSounds.Utils
{
    public static class FFmpegUtil
    {
        private static string? _ffmpegPath;
        private static string? _ffplayPath;
        static FFmpegUtil()
        {
            string? pathVar = Environment.GetEnvironmentVariable("PATH");
            foreach (string path in pathVar?.Split(';') ?? Array.Empty<string>())
            {
                string ffmpegPath = Path.Combine(path, "ffmpeg.exe");
                if (File.Exists(ffmpegPath))
                {
                    _ffmpegPath = ffmpegPath;
                }
                string ffplayPath = Path.Combine(path, "ffplay.exe");
                if (File.Exists(ffplayPath))
                {
                    _ffplayPath = ffplayPath;
                }
            }

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                ColorConsole.WriteLine("无法找到FFmpeg,请确保添加路径到PATH环境变量".OnRed().White());
            }
            if (string.IsNullOrEmpty(_ffplayPath))
            {
                ColorConsole.WriteLine("无法找到FFPlay,请确保添加路径到PATH环境变量".OnRed().White());
            }
        }
        public static void PlaySound(MemoryStream ms)
        {
            // 将读取到的音频数据写入本地临时文件
            string tempPath = Path.GetTempFileName();
            try
            {
                using (FileStream fs = new FileStream(tempPath, FileMode.OpenOrCreate))
                {
                    ms.CopyTo(fs);
                }

                if (string.IsNullOrEmpty(_ffplayPath))
                {
                    ErrorUtil.Error("解析音频", "无法找到ffplay 请将其添加到系统PATH环境变量里!");
                }
                else
                {
                    // 修改ffmpeg参数为：输入流指向临时文件，输出为指定格式的音频数据流
                    //string arguments = $"-i \"{tempPath}\" -f {extension} -";
                    string arguments = $"-autoexit {tempPath}";

                    // 启动新进程执行ffmpeg命令
                    ProcessStartInfo psi = new ProcessStartInfo(_ffplayPath, arguments);
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    using (Process? process = Process.Start(psi))
                    {
                        // 将输出流写入标准输出
                        using (Stream output = Console.OpenStandardOutput())
                        {
                            process?.StandardOutput.BaseStream.CopyTo(output);
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempPath);
            }
        }
        /// <summary>
        /// 保存音频
        /// </summary>
        /// <param name="ms">音频流文件</param>
        /// <param name="fullPath">音频完整保存路径(带文件名)</param>
        /// <param name="target">转换格式</param>
        public static bool SaveSound(MemoryStream ms, string fullPath, string? target = null)
        {
            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                ErrorUtil.Error("保存音频", "保存失败,无法找到FFMPEG");
                return false;
            }

            string tempPath = Path.GetTempFileName();
            try
            {
                using (FileStream fs = new FileStream(tempPath, FileMode.OpenOrCreate))
                {
                    // ms.CopyTo fs 貌似无法写出?? (不知道为啥
                    fs.Write(ms.ToArray()); // 写音频文件
                }

                string srcFileName = Path.GetFileNameWithoutExtension(fullPath);
                string targetFileName = string.Format("{0}.{1}", srcFileName, target??Path.GetExtension(fullPath));
                string savePath = Path.Combine(Path.GetDirectoryName(fullPath) ?? "./", targetFileName);
                string arguments = string.Format("-y -nostdin -i \"{0}\" \"{1}\"", tempPath, savePath);

                ProcessStartInfo psi = new ProcessStartInfo(_ffmpegPath, arguments);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                using (Process? process = Process.Start(psi))
                {
                    process?.WaitForExit();
                    return process?.ExitCode == 0;
                }

            }
            finally { File.Delete(tempPath); }
        }
    }
}
