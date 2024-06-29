namespace mcy.CmdTools.Infrastructure;

public static class Utility
{
    public static string ReverseString(string value)
    {
        char[] charArray = value.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}