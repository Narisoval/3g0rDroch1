namespace ClassLibrary ;

public class LoginInfo : IEquatable<LoginInfo>
{
    public String Password { get; set; }
    public String Login { get; set; }

    public LoginInfo(string login, string password)
    {
        Login = login;
        Password = password;
    }

    public bool Equals(LoginInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Password == other.Password && Login == other.Login;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((LoginInfo)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Password, Login);
    }

    public static bool operator ==(LoginInfo? left, LoginInfo? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(LoginInfo? left, LoginInfo? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{Login} {Password}";
    }
}