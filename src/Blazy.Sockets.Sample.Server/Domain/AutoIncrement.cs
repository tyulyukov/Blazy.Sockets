namespace Blazy.Sockets.Sample.Server.Domain;

public class AutoIncrement
{
    private readonly object _locker = new();
    
    private int _number;
    public int Value
    {
        get
        {
            lock (_locker)
                return _number++;
        }
    }
}