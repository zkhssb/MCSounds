using ColoredConsole;

namespace MCSounds.Utils
{
    public static class ErrorUtil
    {
        public static void Error(string func, string msg)
        {
            Console.Clear();
            ColorConsole.WriteLine();
            ColorConsole.WriteLine(func.Red().OnWhite(), "时发生了错误".Red().OnWhite());
            ColorConsole.WriteLine();
            ColorConsole.WriteLine(msg.Red());
            ColorConsole.WriteLine();
            ColorConsole.Write("按下任意按键继续...".Red());
            Console.ReadKey();
        }
    }
}
