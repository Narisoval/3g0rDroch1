using System.IO.Pipes;
using ClassLibrary;

namespace WinFormsApp1;

public class PipeServer
{
    private static int numThreads = 10;

    private static List<LoginInfo> _loginInfosFromFile;
    
    private static int maxPasswordLength;
    private static int maxLoginLength;
    
    public static void Main()
    {
        LoginInfosFromFileReader loginInfosReader = new LoginInfosFromFileReader();
        _loginInfosFromFile = loginInfosReader.GetPasswordsFromFileDialog();
        maxLoginLength = loginInfosReader.MaxLoginLength;
        maxPasswordLength = loginInfosReader.MaxPasswordLength;
        
        int i;
        Thread[] servers = new Thread[numThreads];

        Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
        Console.WriteLine("Waiting for client connect...\n");
        for (i = 0; i < numThreads; i++)
        {
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }
        Thread.Sleep(250);
        while (i > 0)
        {
            for (int j = 0; j < numThreads; j++)
            {
                if (servers[j] != null)
                {
                    if (servers[j].Join(250))
                    {
                        Console.WriteLine("Server thread[{0}] finished.", servers[j].ManagedThreadId);
                        servers[j] = null;
                        i--;    // decrement the thread watch count
                    }
                }
            }
        }
        Console.WriteLine("\nServer threads exhausted, exiting.");
    }

    private static void ServerThread(object data)
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
