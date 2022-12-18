namespace WinFormsApp1;

public class WrongFileStructureException : Exception
{
    public WrongFileStructureException(string message) : base(message)
    {
        
    }
}