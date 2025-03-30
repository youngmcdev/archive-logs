namespace mcy.CmdTools.Infrastructure;

public interface IUtility
{
    string ReverseString(string value);
}

public class Utility : IUtility
{
    public string ReverseString(string value)
    {
        char[] charArray = value.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}