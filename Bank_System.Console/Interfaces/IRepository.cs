using System.ComponentModel;
using Interfaces;
using Models;

public interface IRepository<T> where T: IEntity
{
    void Add(T entity);
    T? GetById(Guid id);
    IEnumerable<T> GetAll();
    void Delete(Guid id);
}