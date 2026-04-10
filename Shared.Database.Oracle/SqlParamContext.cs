namespace Shared.Database.Oracle;

public class SqlParamContext
{
    private int _index = 0;

    public string Next(string prefix)
    {
        return $"{prefix}_{_index++}";
    }
}
