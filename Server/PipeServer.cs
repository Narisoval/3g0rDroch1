using System.IO.Pipes;
using ClassLibrary;
using static ClassLibrary.CommunicationCodes;

namespace Server;

public class PipeServer
{
    private static int numThreads = 3;

    private static List<LoginInfo> _loginInfosFromFile;
    
    private static List<Task> servers;
    public static void Main()
    {
        LoginInfosFromFileReader loginInfosReader = new LoginInfosFromFileReader();
        _loginInfosFromFile = loginInfosReader.GetPasswordsFromFileDialog();
        
        servers = new List<Task>();

        RunServers();
    }

    private static void RunServers()
    {
        Console.WriteLine("Waiting for client connect...\n");
        CreateRunningServerThreads();
        WaitTillServersDie(); 
        Console.WriteLine("\nServer threads exhausted, creating new ones.");
        RunServers();
    }
    
    private static void CreateRunningServerThreads()
    {
        for (int i = 0; i < numThreads; i++)
        {
            servers.Add(new Task(ServerThread));
            servers[i].Start();
        }
    }
    
    private static void WaitTillServersDie()
    {
        Task.WaitAll(servers.ToArray());
        servers = Enumerable.Empty<Task>().ToList();
    }

    private static void ServerThread()
    {
        NamedPipeServerStream pipeServer =
            new NamedPipeServerStream("testpipe", PipeDirection.InOut, numThreads);

        int threadId = Thread.CurrentThread.ManagedThreadId;
        // Wait for a client to connect
        pipeServer.WaitForConnection();

        Console.WriteLine("Client connected on thread[{0}].", threadId);
        try
        {
            // Read the request from the client. Once the client has
            // written to the pipe its security token will be available.
            StreamString ss = new StreamString(pipeServer);

            // get login and password from useri
            string stringFromClient = ss.ReadString();
            while (stringFromClient != CLIENT_DISCONNECTED)
            {
                string[] loginInfoArray = stringFromClient.Split(" ");
                LoginInfo infoFromClient = new LoginInfo(loginInfoArray[0], loginInfoArray[1]);
                
                ss.WriteString(CheckIfLoginInfoIsCorrect(infoFromClient) ? "1" : "0");

                stringFromClient = ss.ReadString();
            }
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
        
        Console.WriteLine("\nClient disconnected on thread[{0}].", threadId);
        pipeServer.Close();
        }

    private static bool CheckIfLoginInfoIsCorrect(LoginInfo info) => _loginInfosFromFile.Contains(info);

}
