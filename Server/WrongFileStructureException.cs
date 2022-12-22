namespace Server;

public class WrongFileStructureException : Exception
{
    public WrongFileStructureException(string message) : base(message)
    {
        ;
    }
}