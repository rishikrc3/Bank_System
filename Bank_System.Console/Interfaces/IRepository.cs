using System.ComponentModel;
using Interfaces;
using Models;

public interface IRepository<T> where T: IEntity
{
    void Add(T entity);
    T? GetById(Guid id);
    IEnumerable<T> GetAll();
    void Delete(Guid id);
    Task AddAsync(T entity, CancellationToken ct=default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct=default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct=default);
    Task DeleteAsync(Guid id, CancellationToken ct=default);
}