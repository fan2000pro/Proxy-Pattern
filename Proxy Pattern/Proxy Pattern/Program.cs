using System;
using System.Collections.Generic;

public interface Subject
{
    string Request(string request);
}

public class RealSubject : Subject
{
    public string Request(string request)
    {
        Console.WriteLine($"RealSubject: Обработка запроса \"{request}\".");
        return $"Ответ для \"{request}\" от RealSubject.";
    }
}

public class Proxy : Subject
{
    private readonly RealSubject _realSubject = new();
    private readonly Dictionary<string, (string response, DateTime timestamp)> _cache = new();
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromSeconds(10); 

    public string Request(string request)
    {
        Console.WriteLine("Proxy: Проверка прав доступа...");
        if (!CheckAccess())
        {
            throw new UnauthorizedAccessException("Proxy: Доступ запрещён!");
        }

        Console.WriteLine("Proxy: Проверка кэша...");
        if (_cache.TryGetValue(request, out var cacheEntry))
        {
            if (DateTime.Now - cacheEntry.timestamp < _cacheLifetime)
            {
                Console.WriteLine("Proxy: Использование кэшированного ответа.");
                return cacheEntry.response;
            }
            else
            {
                Console.WriteLine("Proxy: Кэш устарел. Удаление старой записи.");
                _cache.Remove(request);
            }
        }

        Console.WriteLine("Proxy: Кэш не найден. Обращение к RealSubject.");
        var response = _realSubject.Request(request);
        _cache[request] = (response, DateTime.Now);

        return response;
    }

    private bool CheckAccess()
    {
        return DateTime.Now.Second % 2 == 0;
    }
}

public class Program
{
    public static void Main()
    {
        Subject proxy = new Proxy();

        try
        {
            Console.WriteLine(proxy.Request("Запрос 1"));
            Console.WriteLine(proxy.Request("Запрос 2"));

            Console.WriteLine(proxy.Request("Запрос 1"));

            Console.WriteLine("Ожидание 11 секунд...");
            System.Threading.Thread.Sleep(11000);

            Console.WriteLine(proxy.Request("Запрос 1"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
