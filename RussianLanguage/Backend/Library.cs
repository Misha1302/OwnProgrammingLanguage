namespace RussianLanguage.Backend;

public struct Library
{
    public string Version;
    public string FullName;

    public Library(string fullName, string version)
    {
        FullName = fullName;
        Version = version;
    }
}