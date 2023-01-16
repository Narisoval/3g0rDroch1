using System.IO.Pipes;
using Core;

namespace Server;

public class PipeServer
{
    private static List<LoginInfo>? _loginInfosFromFile;
    
    private static List<Task>? _servers;

    private const int MaxAllowedServerInstances = 2;
    public static void Main()
    {
        GetLoginInfos();
        _servers = new List<Task>();
        RunServers();
    }

    private static void GetLoginInfos()
    {
        LoginInfosFromFileReader loginInfosReader = new LoginInfosFromFileReader("Choose a file with password login pairs");
        _loginInfosFromFile = loginInfosReader.GetPasswordsFromFileDialog();
    }

    private static void RunServers()
    {
        CreateRunningServerThread(); 
        WaitTillServersDie(); 
    }
    
    private static void CreateRunningServerThread()
    {
        var serverThread = new Task(ServerThread);
        serverThread.Start();
        _servers!.Add(serverThread);
    }
    
    private static void WaitTillServersDie()
    {
        Task.WaitAll(_servers!.ToArray());
        _servers = Enumerable.Empty<Task>().ToList();
    }

    private static void ServerThread()
    {
        NamedPipeServerStream pipeServer =
            new NamedPipeServerStream(ServerConstants.PipeName, PipeDirection.InOut,MaxAllowedServerInstances);

        pipeServer.WaitForConnection();
        CreateRunningServerThread();
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine($"Client connected on thread[{threadId}].");
        try
        {
            StreamString ss = new StreamString(pipeServer);
            
            while (pipeServer.IsConnected)
            {
                var infoFromClient = GetLoginInfoFromClient(ss);
                ss.WriteString(CheckIfLoginInfoIsCorrect(infoFromClient) ? "1" : "0");
            }

            RestartServerThread(pipeServer);
        }
        // Catch the SystemException that is raised if the pipe is broken
        // or disconnected.
        catch (SystemException e)
        {
            Console.WriteLine($"ERROR: {e.Message}. Restarting server on thread {threadId}");
            RestartServerThread(pipeServer);
        }
    }
    
    private static LoginInfo GetLoginInfoFromClient(StreamString ss)
    {
        string stringFromClient = ss.ReadString();
        string[] loginInfoArray = stringFromClient.Split(" ");
        return new LoginInfo(loginInfoArray[0], loginInfoArray[1]);
    }

    private static void RestartServerThread(NamedPipeServerStream server)
    {
        server.Close();
        ServerThread();
    }
    private static bool CheckIfLoginInfoIsCorrect(LoginInfo info) => _loginInfosFromFile.Contains(info);

}
