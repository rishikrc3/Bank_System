using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Interfaces;
public class FileRepository <T>: IDisposable, IAsyncDisposable, IRepository<T> where T : IEntity 
{
    private readonly string _filePath;
    private List<T> _items;
    private bool _disposed = false;
    private IAsyncDisposable? _example;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await SaveChangesAsync().ConfigureAwait(false);
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
            using FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader reader = new StreamReader(fs);
            string json = reader.ReadToEnd();
            _items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        else
        {
            _items = new List<T>();
        }
    }
    private async Task SaveChangesAsync()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
       await using FileStream fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
       await using StreamWriter writer = new StreamWriter(fs);
       string json = JsonSerializer.Serialize(_items, options);
       await writer.WriteAsync(json);
    }
    private void SaveChanges()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        using FileStream fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new StreamWriter(fs);
        string json = JsonSerializer.Serialize(_items,options);
        writer.Write(json);
    }
    public void Add(T item)
    {
        _items.Add(item);
        SaveChanges();
    }//
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
        SaveChanges();
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        _items.Add(entity);
        await SaveChangesAsync();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
       var item = _items.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_items.AsEnumerable());
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _items.RemoveAll(x => x.Id == id);
        await SaveChangesAsync();
    }

    ~FileRepository()
    {
        Dispose(false);
    }
}