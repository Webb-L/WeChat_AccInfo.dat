namespace WeChat_AccInfo.dat;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage:\nWeChat_AccInfo.dat.exe \"C:\\Users\\{YourUserName}\\Documents\\WeChat Files\"");
            return;
        }

        Console.WriteLine(String.Join("\n===========================\n", new WeChatUserInfo(args[0]).GetUserInfos()));
    }
}