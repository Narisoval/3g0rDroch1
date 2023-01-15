namespace Droch1;

using System;
using System.IO.Pipes;
using System.Diagnostics;
using System.Security.Principal;
using Core;
using static Core.ServerConstants;

public class PipeClient
{
    private static NamedPipeClientStream _pipeClientStream;
    private static StreamString _streamString;
    public static void Main(string[] args)
    {
        Console.WriteLine("\nChoose mode:\n1)Connection test;\n2)Hack mode;");

        SelectMode();
    }

    private static void SelectMode()
    {
        string? inputFromUser = Console.ReadLine();

        switch (inputFromUser)
        {
            case "1":
                RunAsTest();
                break;
            case "2":
                RunHackMode();
                break;
            default:
                Console.WriteLine("Only 1 and 2 are sufficient options, please, refrain from using anything else");
                SelectMode();
                break;
        }
    }
    
    private static void RunHackMode()
    {
        Console.WriteLine("Enter login");
        string login = Console.ReadLine();
        
        BruteForcer bruteForcer = new BruteForcer(login);
        ConnectToServer();
        
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start(); 
        foreach (LoginInfo currentLoginInfo in bruteForcer.BruteForce())
        {
            //Console.WriteLine($"Attempted password: {currentLoginInfo.Password}");
            bool isLoginInfoCorrect = CheckLoginInfoCorrect(currentLoginInfo);
            if (isLoginInfoCorrect)
            {
                Console.WriteLine($"Hack successful, password: {currentLoginInfo.Password}");
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime); 
                break;                
            }
        }
        DisconnectFromServer();
    }
    
    private static void RunAsTest()
    {
        var infoFromUser = GetLoginInfoFromUser();

        ConnectToServer();
        bool isLoginInfoCorrect = CheckLoginInfoCorrect(infoFromUser);
        DisconnectFromServer();
        
        if (isLoginInfoCorrect)
        {
            Console.WriteLine("Your password login pair is right!!");    
        }
        else
        {
            Console.WriteLine("Your password login pair is wrong!!");    
        }
    }
    
    private static void ConnectToServer()
    {
        _pipeClientStream =
            new NamedPipeClientStream(ServerName,PipeName,
                PipeDirection.InOut, PipeOptions.None,
                TokenImpersonationLevel.Impersonation);

        Console.WriteLine("Connecting to server...\n");
        _streamString = new StreamString(_pipeClientStream);
        _pipeClientStream.Connect();
    }

    private static void DisconnectFromServer()
    {
        _pipeClientStream.Close();
    }
    
    private static bool CheckLoginInfoCorrect(LoginInfo loginInfo)
    {
        _streamString.WriteString(loginInfo.ToString());
        return _streamString.ReadString() == "1";
    }
    
    private static LoginInfo GetLoginInfoFromUser()
    {
        Console.WriteLine("Enter login: ");
        string login = Console.ReadLine();
        Console.WriteLine("Enter password: ");
        string password = Console.ReadLine();
        LoginInfo infoFromUser;
        try
        {
            infoFromUser = new LoginInfo(login, password);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("Password or login is incorrect format. Please try to reenter them.\n");
            return GetLoginInfoFromUser();
        }
        return infoFromUser;
    }

    
}