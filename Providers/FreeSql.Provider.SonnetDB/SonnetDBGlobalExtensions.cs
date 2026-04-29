using FreeSql.SonnetDB;

public static class FreeSqlSonnetDBGlobalExtensions
{
    public static string FormatSonnetDB(this string that, params object[] args) => _sonnetDBAdo.Addslashes(that, args);

    static readonly SonnetDBAdo _sonnetDBAdo = new SonnetDBAdo();
}
