namespace Droch1;

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using ClassLibrary;

public class PipeClient
{
    private static List<Task> clients;
    public static void Main(string[] args)
    {
        Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");

        var possibleLoginPairs = GenerateLoginInfos();

        Console.WriteLine("Spawning client threads...\n");
        
        CreateRunningClientThreads(possibleLoginPairs);
        
        Task.WaitAll(clients.ToArray());
        
        Console.WriteLine("\nClient processes finished, exiting.");
    }
    
    private static void CreateRunningClientThreads(List<LoginInfo> infosToCheck)
    {
        clients = new List<Task>();
        foreach (var info in infosToCheck)
        {
            var taskToAdd = Task.Run(() => ClientThread(info));
            clients.Add(taskToAdd);
        }
    }
    
    private static void ClientThread(LoginInfo infoToCheck)
    {
        var pipeClient =
            new NamedPipeClientStream(".", "testpipe",
                PipeDirection.InOut, PipeOptions.None,
                TokenImpersonationLevel.Impersonation);

        Console.WriteLine("Connecting to server...\n");

        TryToConnect(pipeClient);

        if (CheckLoginInfoCorrect(infoToCheck, pipeClient))
            Console.WriteLine("Correct INFO!!!");
        else
            Console.WriteLine("Wrong INFO((");

        pipeClient.Close();

    }
    
    private static void TryToConnect(NamedPipeClientStream server)
    {
        try
        {
            server.Connect();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("CAUGHT FILENOTFOUND ERROR TRYING TO RECONNECT !!!!\n \n \n");
            Thread.Sleep(250);
            TryToConnect(server);
        }
    }

    private static bool CheckLoginInfoCorrect(LoginInfo loginInfo, NamedPipeClientStream pipeClientStream)
    {
        var ss = new StreamString(pipeClientStream);
        ss.WriteString(loginInfo.ToString());
        return ss.ReadString() == "1";
    }

    private static List<LoginInfo> GenerateLoginInfos()
    {
        List<LoginInfo> loginInfos = new List<LoginInfo>();

        // populate list with test data
        loginInfos.Add(new LoginInfo("log`n", "password"));
        loginInfos.Add(new LoginInfo("joder", "abc122"));
        loginInfos.Add(new LoginInfo("login", "password"));
        loginInfos.Add(new LoginInfo("joder", "abc122"));
        loginInfos.Add(new LoginInfo("login", "password"));
        return loginInfos;
    }
}