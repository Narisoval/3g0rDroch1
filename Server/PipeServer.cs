using System.IO.Pipes;
using ClassLibrary;

namespace Server;

public class PipeServer
{
    private static int numThreads = 6;

    private static List<LoginInfo> _loginInfosFromFile;
    
    private static int maxPasswordLength;
    private static int maxLoginLength;
    private static List<Task> servers;
    public static void Main()
    {
        LoginInfosFromFileReader loginInfosReader = new LoginInfosFromFileReader();
        _loginInfosFromFile = loginInfosReader.GetPasswordsFromFileDialog();
        
        maxLoginLength = loginInfosReader.MaxLoginLength;
        maxPasswordLength = loginInfosReader.MaxPasswordLength;

        servers = new List<Task>();

        Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
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

            // get login and password from user
            string[] loginInfoArrayFromClient = ss.ReadString().Split(" ");

            try
            {
                LoginInfo loginInfoFromClient = new LoginInfo(loginInfoArrayFromClient[0],
                    loginInfoArrayFromClient[1]);
                ss.WriteString(_loginInfosFromFile.Contains(loginInfoFromClient) ? "1" : "0");
            }
            catch (Exception e)
            {
                if(e is IndexOutOfRangeException)
                    ss.WriteString("0");
                else
                {
                    throw;
                }
            }

        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
        pipeServer.Close();
    }
}
