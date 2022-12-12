
using System.Reflection;

internal static class ConsoleStyle
{
    private const string Header = @"
 _______  _______        _                        _______  _______  _______                   
(  ___  )/ ___   )      | \    /\|\     /|       (  ____ )(  ____ )(  ___  )|\     /||\     /|
| (   ) |\/   )  |      |  \  / /| )   ( |       | (    )|| (    )|| (   ) |( \   / )( \   / )
| (___) |    /   )_____ |  (_/ / | |   | | _____ | (____)|| (____)|| |   | | \ (_) /  \ (_) / 
|  ___  |   /   /(_____)|   _ (  ( (   ) )(_____)|  _____)|     __)| |   | |  ) _ (    \   /  
| (   ) |  /   /        |  ( \ \  \ \_/ /        | (      | (\ (   | |   | | / ( ) \    ) (   
| )   ( | /   (_/\      |  /  \ \  \   /         | )      | ) \ \__| (___) |( /   \ )   | |   
|/     \|(_______/      |_/    \/   \_/          |/       |/   \__/(_______)|/     \|   \_/   
                                                                                              
";
    public static void WriteHeader(int port = 5616)
    {
        Console.WriteLine(Header);
        Console.WriteLine("##############################################################");
        Console.WriteLine("#");
        Console.WriteLine("# command: az-kv-proxy");
        Console.WriteLine("# version: {0}", Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
        Console.WriteLine("#");
        Console.WriteLine("# Repository: https://github.com/Smartersoft/identity-client-assertion");
        Console.WriteLine("# Documentation: http://localhost:{0}/swagger/index.html", port);
        Console.WriteLine("#");
        Console.WriteLine("# dotnet tool update --global Smartersoft.Identity.Client.Assertion.Proxy");
        Console.WriteLine("#");
        Console.WriteLine("####################      CTRL + C to Exit   #################");
    }
}

