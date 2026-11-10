namespace MauiEng.Data;

internal static class DbValues
{
    public static int Id(object? value) => value switch
    {
        int id => id,
        long id => checked((int)id),
        _ => throw new InvalidOperationException("База вернула пустой идентификатор.")
    };

    public static int Count(object? value) => value switch
    {
        int count => count,
        long count => checked((int)count),
        null or DBNull => 0,
        _ => throw new InvalidOperationException("База вернула неожиданный счётчик.")
    };
}
