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
        LoginInfosFromFileReader loginInfosReader = new LoginInfosFromFileReader();
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
            new NamedPipeServerStream("testpipe", PipeDirection.InOut,MaxAllowedServerInstances);

        int threadId = Thread.CurrentThread.ManagedThreadId;
        pipeServer.WaitForConnection();

        Console.WriteLine("Client connected on thread[{0}].", threadId);
        CreateRunningServerThread();
        try
        {
            StreamString ss = new StreamString(pipeServer);
            
            while (true)
            {
                var infoFromClient = GetLoginInfoFromClient(ss);
                ss.WriteString(CheckIfLoginInfoIsCorrect(infoFromClient) ? "1" : "0");
                if(!pipeServer.IsConnected)
                    ServerThread();
                    
            }
        }
        // Catch the SystemException that is raised if the pipe is broken
        // or disconnected.
        catch (SystemException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
            ServerThread();
        }
    }
    
    private static LoginInfo GetLoginInfoFromClient(StreamString ss)
    {
        string stringFromClient = ss.ReadString();
        string[] loginInfoArray = stringFromClient.Split(" ");
        return  new LoginInfo(loginInfoArray[0], loginInfoArray[1]);
    }

    private static bool CheckIfLoginInfoIsCorrect(LoginInfo info) => _loginInfosFromFile.Contains(info);

}
