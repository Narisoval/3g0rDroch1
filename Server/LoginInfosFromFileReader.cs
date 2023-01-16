using Core;

namespace Server;

public class LoginInfosFromFileReader
{
    public int MaxPasswordLength { get; set; }
    public int MaxLoginLength { get; set; }

    private OpenFileDialog _fileDialog;
    private List<LoginInfo>? _loginInfosFromFile;

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            _fileDialog.Title = _title;
        }
    }

    public LoginInfosFromFileReader(string title)
    {
        SetUpFileDialog();
        Title = title;
        _loginInfosFromFile = new List<LoginInfo>();
    }

    private void SetUpFileDialog()
    {
        _fileDialog = new OpenFileDialog();
        _fileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        _fileDialog.RestoreDirectory = true;
    }

    public List<LoginInfo>? GetPasswordsFromFileDialog()
    {
        PopulateLoginInfoList();
        return _loginInfosFromFile;
    }

    private void PopulateLoginInfoList()
    {
        using (_fileDialog)
        {
            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read the contents of the file into a stream
                Stream fileStream = _fileDialog.OpenFile();
                TryReadFileInformation(fileStream);
            }
        }
    }
    
    private void TryReadFileInformation(Stream fileStream)
    {
        using StreamReader reader = new StreamReader(fileStream);
        try
        {
            // First line of file should be max login length and max password length
            // separated by space like so "13 25"
            SetMaxLengths(reader); 
            ReadLoginInfosFromStream(reader);
        }
        catch (Exception e)
        {
            if (e is FormatException or NullReferenceException)
            {
                throw new WrongFileStructureException("The file structure is inappropriate");
            }
            throw;
        }
    }

    private void SetMaxLengths( StreamReader reader)
    {
        string? lengthsString = reader.ReadLine();
        var maxLoginAndPasswordLengths = lengthsString.Split(" ");
        MaxLoginLength = Int32.Parse(maxLoginAndPasswordLengths[0]);
        MaxPasswordLength = Int32.Parse(maxLoginAndPasswordLengths[1]);
    }

    private void ReadLoginInfosFromStream(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            string[]? loginPasswordLine = reader.ReadLine().Split(" ");
            _loginInfosFromFile.Add(new LoginInfo(loginPasswordLine[0], loginPasswordLine[1]));
        }
    }
}