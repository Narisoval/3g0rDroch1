namespace Droch1;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using ClassLibrary;

public class PipeClient
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
                var pipeClient =
                    new NamedPipeClientStream(".", "testpipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);

                Console.WriteLine("Connecting to server...\n");
                pipeClient.Connect();

                string currentInfoString = args.Aggregate((s1,s2) => s1 + " " + s2);
                Console.WriteLine(currentInfoString);
                
                if(CheckLoginInfoCorrect(currentInfoString,pipeClient))
                    Console.WriteLine("Correct INFO!!!");
                else
                    Console.WriteLine("Wrong INFO((");
                
                pipeClient.Close();
                
                // Give the client process some time to display results before exiting.
                Thread.Sleep(4000);
        }
        else
        {
            Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
            StartClients();
        }
    }

    // Helper function to create pipe client processes
    private static void StartClients()
    {
        string currentProcessName = CleanUpProcessName(Environment.CommandLine);

        var possibleLoginPairs = GenerateLoginInfos();
        int possiblePairsCount = possibleLoginPairs.Count;
        
        Process[] plist = new Process[possiblePairsCount];
        Console.WriteLine("Spawning client processes...\n");
        
        int i;
        for (i = 0; i < possiblePairsCount; i++)
        {
            // Start 'this' program but spawn a named pipe client and pass login info as argument.
            plist[i] = Process.Start(currentProcessName, possibleLoginPairs[i].ToString());
        }
        while (i > 0)
        {
            for (int j = i; j < possiblePairsCount; j++)
            {
                if (plist[j].HasExited)
                {
                    Console.WriteLine($"Client process[{plist[j].Id}] has exited.");
                    plist[j] = null;
                    i--;    // decrement the process watch count
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
        }
        Console.WriteLine("\nClient processes finished, exiting.");
    }

    private static bool CheckLoginInfoCorrect(LoginInfo loginInfo, NamedPipeClientStream pipeClientStream)
    {
        var ss = new StreamString(pipeClientStream);
        ss.WriteString(loginInfo.ToString());
        return  ss.ReadString() == "1";
    }
    
    private static bool CheckLoginInfoCorrect(string loginInfoString, NamedPipeClientStream pipeClientStream)
    {
        var ss = new StreamString(pipeClientStream);
        ss.WriteString(loginInfoString);
        return  ss.ReadString() == "1";
    }

    private static string CleanUpProcessName(string processName)
    {
        // Remove extra characters when launched from Visual Studio
        processName = processName.Trim('"', ' ');

        processName = Path.ChangeExtension(processName, ".exe");

        if (processName.Contains(Environment.CurrentDirectory))
        {
            processName = processName.Replace(Environment.CurrentDirectory, String.Empty);
        }

        // Remove extra characters when launched from Visual Studio
        processName = processName.Replace("\\", String.Empty);
        processName = processName.Replace("\"", String.Empty);

        return processName;
    }

    private static List<LoginInfo> GenerateLoginInfos()
    {
        List<LoginInfo> loginInfos = new List<LoginInfo>();
        
        // populate list with test data
        loginInfos.Add(new LoginInfo("login","password"));
        loginInfos.Add(new LoginInfo("logigsahc","passw0rd"));
        loginInfos.Add(new LoginInfo("log`n","password"));
        loginInfos.Add(new LoginInfo("joder", "abc122"));
        
        return loginInfos;
    }
}
