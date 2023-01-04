using static ClassLibrary.CommunicationCodes;

namespace Droch1;

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using ClassLibrary;
public class PipeClient
{
    //private static int passwordMaxLength;
    public static void Main(string[] args)
    {
        Console.WriteLine("\nChoose mode:\n1)Connection test;\n2)Hack mode;");

        SelectMode();
        
        Console.WriteLine("Spawning client threads...\n");
        Console.WriteLine("\nClient processes finished, exiting.");
    }

    private static void SelectMode()
    {
        string inputFromUser = Console.ReadLine();

        if (inputFromUser == "1")
            RunAsTest();
        else if (inputFromUser == "2")
            RunHackMode();
        else
        {
            Console.WriteLine("Only 1 and 2 are sufficient options, please, refrain from using anything else");
            SelectMode();
        }
    }

    //
    
    private static void RunHackMode()
    {
        Console.WriteLine("Enter login");
        string login = Console.ReadLine();
        //SetPasswordMaxLengthFromUser();
        BruteForcer bruteForcer = new BruteForcer(login);
        var pipeClient =
            new NamedPipeClientStream(".", "testpipe",
                PipeDirection.InOut, PipeOptions.None);

        Console.WriteLine("Connecting to server...\n");

        pipeClient.Connect();
        StreamString ss = new StreamString(pipeClient);
        foreach (LoginInfo currentLoginInfo in bruteForcer.BruteForce())
        {
            Console.WriteLine($"Attempted password: {currentLoginInfo.Password}");
            bool result = CheckLoginInfoCorrect(currentLoginInfo, ss);
            if (result)
            {
                Console.WriteLine($"Hack successful, password: {currentLoginInfo.Password}");
                break;                
            }
        }
        ss.WriteString(CLIENT_DISCONNECTED);
        pipeClient.Close();
    }

    private static void SetPasswordMaxLengthFromUser()
    {
        Console.WriteLine("Enter the maximum password length");
        int passwordMaxLengthFromUser;
        bool passwordLengthSuccessfullyParsed = int.TryParse(Console.ReadLine(), out passwordMaxLengthFromUser);
        if (passwordLengthSuccessfullyParsed)
        {
    //        passwordMaxLength = passwordMaxLengthFromUser;
            return;
        }
        Console.WriteLine("Password length is wrong, please, try again");
        SetPasswordMaxLengthFromUser();
    }
    private static void RunAsTest()
    {
        Console.WriteLine("Enter login");
        string login = Console.ReadLine();
        Console.WriteLine("Enter password");
        string password = Console.ReadLine();
        LoginInfo infoFromUser = new LoginInfo(login, password);
        ClientThread(infoFromUser);
    }
    
    private static bool ClientThread(LoginInfo infoToCheck)
    {
        var pipeClient =
            new NamedPipeClientStream(".", "testpipe",
                PipeDirection.InOut, PipeOptions.None,
                TokenImpersonationLevel.Impersonation);

        Console.WriteLine("Connecting to server...\n");

        pipeClient.Connect();
        StreamString ss = new StreamString(pipeClient);

        bool result = CheckLoginInfoCorrect(infoToCheck, ss);
        ss.WriteString(CLIENT_DISCONNECTED);
        pipeClient.Close();
        return result;

    }
    

    private static bool CheckLoginInfoCorrect(LoginInfo loginInfo, StreamString streamString )
    {
        streamString.WriteString(loginInfo.ToString());
        return streamString.ReadString() == "1";
    }
    
}