using AngleSharp.Dom;
using ColoredConsole;
using MCSounds.Fandom;
using MCSounds.Utils;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MCSounds
{
    internal class Program
    {
        private static FandomClient client = new(FandomLanguage.Zh);

        private static void PrintInfo()
        {
            ColorConsole.WriteLine("                   ".White().OnRed());
            ColorConsole.WriteLine(" * MCSounds        ".White().OnRed());
            ColorConsole.WriteLine(" * By 野生的矿泉水 ".White().OnRed());
            ColorConsole.WriteLine(" * Fandom语言:".OnRed().White(), client.Language.ToString().OnRed().White(), "   ".OnRed().White());
            ColorConsole.WriteLine("                   ".White().OnRed());
            ColorConsole.WriteLine();
        }

        private static async Task Wait(string waitWhat, Task task, bool clear = true)
        {
            if (clear)
                Console.Clear();
            Console.Write("{0}中", waitWhat);
            while (!task.IsCompleted)
            {
                Console.Write(".");
                await Task.Delay(500);
            }
        }

        private static void SaveSounds(IEnumerable<string> urls)
        {

        }

        private static MemoryStream GetSound(string url)
        {
            MemoryStream? ms = null;
            Task.WaitAll(Task.Run(async () =>
            {
                Task<MemoryStream> task = client.GetStreamAsync(url);
                await Wait("获取音频", task, false);
                ms = task.Result;
            }));
            return ms!;
        }

        private static bool ShowSound(SoundInfo sound)
        {


            bool done = false;
            while (!done)
            {
                Console.Clear();
                ColorConsole.WriteLine();
                ColorConsole.WriteLine(sound.Text.Green(), " ", "中有".Green(), sound.Urls.Count.ToString().Green(), "个音频".Green());
                ColorConsole.WriteLine();
                for (int i = 0; i < sound.Urls.Count; i++)
                {
                    ColorConsole.WriteLine(" ", i.ToString().Green(), ":\t", FandomUtil.GetUrlFileName(sound.Urls[i]));
                }

                Console.WriteLine();
                Console.WriteLine("输入序号播放");
                Console.WriteLine("输入s 序号保存 (可多个序号,空格分割,例: s 0 1 2 3)");
                Console.WriteLine("输入空的文本可以返回");
                string? cmd = Console.ReadLine();
                if (!string.IsNullOrEmpty(cmd))
                {
                    string[] commands = cmd.Split(' ');
                    if (commands.Length > 0)
                    {
                        if (commands[0].ToLower() == "s")
                        {
                            List<string> downloadList = new();
                            foreach (string idString in commands.Skip(1))
                            {
                                if (int.TryParse(idString, out var id))
                                {
                                    if (id >= 0 && id < sound.Urls.Count)
                                    {
                                        downloadList.Add(sound.Urls[id]);
                                    }
                                    else
                                    {
                                        ErrorUtil.Error("解析音频", string.Format("解析音频ID失败,ID越界 {0}", id));
                                    }
                                }
                                else
                                {
                                    ErrorUtil.Error("解析音频", string.Format("解析音频ID失败,输入了错误的ID {0}", id));
                                }
                            }
                            Console.Clear();
                            Console.WriteLine("开始下载{0}个文件!", downloadList.Count);
                            List<string> failUrlList = new();
                            Dictionary<string, MemoryStream> downDir = new();
                            foreach (var url in downloadList)
                            {
                                ColorConsole.Write(string.Format(" * {0} ", FandomUtil.GetUrlFileName(url)).Green());
                                using (MemoryStream ms = GetSound(url))
                                {
                                    if (ms.Length > 0)
                                    {
                                        MemoryStream memoryStream = new();
                                        ms.CopyTo(memoryStream);
                                        downDir.Add(url, memoryStream);
                                    }
                                    else
                                    {
                                        failUrlList.Add(url);
                                        ColorConsole.Write("失败".Red());
                                    }
                                }
                                Console.WriteLine();
                            }

                            ColorConsole.WriteLine(" -".Green(), " 获取音频 成功", downDir.Count.ToString().Green(), "个", " 失败", failUrlList.Count.ToString().Red(), "个!");

                            if (downDir.Count == 0)
                            {
                                Console.WriteLine("下载任意文件失败,按下任意按键退出...");
                                Console.ReadKey();
                                break;
                            }

                            Console.WriteLine();
                            Console.WriteLine("开始保存音频");

                            string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            string soundPath = Path.Combine(musicPath, FandomUtil.GetSoundName(downDir.Keys.First()));

                            Directory.CreateDirectory(soundPath);

                            foreach (var downSound in downDir)
                            {
                                try
                                {
                                    string soundFileName = FandomUtil.GetUrlFileName(downSound.Key);
                                    ColorConsole.Write(string.Format("* {0}", soundFileName).Green());
                                    string savePath = Path.Combine(soundPath, soundFileName);
                                    // mp3可以替换为string.Empty 如果为空则是默认后缀输出
                                    if (FFmpegUtil.SaveSound(downSound.Value, savePath, "mp3"))
                                        ColorConsole.WriteLine(" done!".Green());
                                    else
                                        ColorConsole.WriteLine(" error!".Red());
                                }
                                finally
                                {
                                    downSound.Value.Dispose();
                                }
                            }



                            Console.WriteLine();
                            Console.WriteLine("保存到 {0}", soundPath);
                            Console.Write("按下任意按键返回上级...");
                            Console.ReadKey(false);
                        }
                        else // 播放音频
                        {
                            if (int.TryParse(commands[0], out var id))
                            {
                                if (id >= 0 && id < sound.Urls.Count)
                                {
                                    using (MemoryStream soundMemoryStream = GetSound(sound.Urls[id]))
                                    {
                                        if (soundMemoryStream.Length != 0)
                                        {
                                            FFmpegUtil.PlaySound(soundMemoryStream);
                                        }
                                    }
                                }
                                else
                                {
                                    ErrorUtil.Error("解析音频", string.Format("解析音频ID失败,ID越界 {0}", id));
                                }
                            }
                            else
                            {
                                ErrorUtil.Error("解析音频", string.Format("解析音频ID失败,输入了错误的ID {0}", id));
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static bool SelectSound(List<SoundInfo> sounds)
        {
            bool done = false;
            int index = 0;
            Console.Clear();
            while (!done)
            {
                for (int i = 0; i < sounds.Count; i++)
                {
                    if (index == i)
                    {
                        ColorConsole.Write("> ".Blue());
                        ColorConsole.WriteLine(string.Format("[{0}]{1}({2})  ", sounds[i].Type, sounds[i].Text, sounds[i].Description).Blue());
                    }
                    else
                    {
                        Console.WriteLine("[{0}]{1}({2})  ", sounds[i].Type, sounds[i].Text, sounds[i].Description);
                    }
                }
                Console.WriteLine();
                Console.SetCursorPosition(0, index);
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        return false;
                    case ConsoleKey.Enter:
                        done = ShowSound(sounds[index]);
                        Console.Clear();
                        break;
                    case ConsoleKey.UpArrow:
                        if (-1 < index - 1)
                        {
                            index--;
                        }
                        else
                        {
                            index = sounds.Count - 1;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (sounds.Count - 1 > index)
                        {
                            index++;
                        }
                        else
                        {
                            index = 0;
                        }
                        break;
                }
                Console.SetCursorPosition(0, 0);
            }
            return done;
        }

        private static async Task<bool> SetResult(QueryResult result)
        {
            Task<List<SoundInfo>> task = client.Sound(result);
            await Wait(string.Format("访问{0}", result.Title), task);
            List<SoundInfo> infos = task.Result;
            if (infos.Count > 0)
            {
                return SelectSound(infos);
            }
            else
            {
                ErrorUtil.Error(string.Format("{0}页面", result.Title), string.Format("无法找到音频,可能这个页面没有音频\n请尝试手动访问:{0}", result.Url));
            }
            return false;
        }

        private static async ValueTask Find(string key)
        {
            static void logResult(int index, QueryResult result)
            {
                ColorConsole.Write(" ", index.ToString().Green(), ":\t");
                Console.WriteLine(result.Title);
            }
            Task<List<QueryResult>> task = client.Query(key);
            await Wait(string.Format("搜索{0}", key), task);
            List<QueryResult> results = task.Result;
            if (results.Count > 0)
            {
                bool done = false;
                while (!done)
                {
                    Console.Clear();
                    Console.WriteLine();
                    ColorConsole.WriteLine("搜寻到".OnRed().White(), results.Count.ToString().OnRed().White(), "个结果".OnRed().White());
                    Console.WriteLine();
                    for (int i = 0; i < results.Count; i++)
                    {
                        logResult(i, results[i]);
                    }
                    Console.WriteLine();
                    ColorConsole.WriteLine("输入\".\"退出".OnRed().White());
                    ColorConsole.Write("请输入ID:".OnRed().White());
                    string? idString = Console.ReadLine();
                    if (idString == ".")
                        return;
                    if (int.TryParse(idString, out int id))
                    {
                        if (id < 0 || id > results.Count - 1)
                        {
                            ErrorUtil.Error("搜索", "ID超出找到的范围");
                        }
                        else
                        {
                            done = await SetResult(results[id]);
                        }
                    }
                    else
                    {
                        ErrorUtil.Error("搜索", "解析ID失败,请输入正确的整数!");
                    }
                }
            }
            else
            {
                ErrorUtil.Error("搜索", "无法找到结果");
            }
        }

        private static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                PrintInfo();
                ColorConsole.Write("请输入关键词:".OnGray().Black());
                string? key = Console.ReadLine();
                if (!string.IsNullOrEmpty(key))
                {
                    Console.Clear();
                    Task.WaitAll(Task.Run(async () =>
                    {
                        try
                        {
                            await Find(key);
                        }
                        catch (Exception ex)
                        {
                            ErrorUtil.Error("未知区域", ex.ToString());
                        }
                    }));
                }
            }

        }
    }
}