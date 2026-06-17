using System.Text.Json;
using Interfaces;
public class FileRepository <T>: IDisposable, IRepository<T> where T : IEntity 
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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(_items, options);
        File.WriteAllText(_filePath, json);
    }
    public void Add(T item)
    {
        _items.Add(item);
        SaveChanges();
    }

    public T? GetById(Guid id)
    {
        return _items.FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<T> GetAll()
    {
        return _items;
    }

    public void Delete(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
    }
    ~FileRepository()
    {
        Dispose(false);
    }
}