using System.Text.Json;
using Interfaces;
public class FileRepository <T>: IDisposable where T : IEntity 
{
    private readonly string _filePath;
    private List<T> _items;
    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            SaveChanges();
        }
        _disposed = true;
    }
    public FileRepository(string filePath)
    {
        _filePath = filePath;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            _items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        else
        {
            _items = new List<T>();
        }
    }
    private void SaveChanges()
    {
        string json = JsonSerializer.Serialize(_items);
        File.WriteAllText(_filePath, json);
    }
    public void Add(T item)
    {
        _items.Add(item);
        SaveChanges();
    }
    ~FileRepository()
    {
        Dispose(false);
    }
}