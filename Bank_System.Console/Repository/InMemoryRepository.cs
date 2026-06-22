using Exceptions;
using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _items = new ();
        public void Add(T entity)
        {
            _items.Add(entity);
        }
        public void Delete(Guid id)
        {
            _items.RemoveAll(x => x.Id == id);
        }
        public IEnumerable<T> GetAll()
        {
            return _items;
        }
        public T? GetById(Guid id)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }
        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await Task.Delay(100);
            _items.Add(entity);
        }
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            await Task.Delay(100);
            return _items.FirstOrDefault(x => x.Id == id);
        }
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            await Task.Delay(100);
            return _items;
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await Task.Delay(100);
            _items.RemoveAll(x => x.Id == id);
        }

        public void Update(T entity)
        {
           var item = _items.FirstOrDefault(x => x.Id == entity.Id);
           var index = _items.IndexOf(item);
           _items[index] = entity; 
        }

        public async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            await Task.Delay(100);
            var item = _items.FirstOrDefault(x => x.Id == entity.Id);
            var index = _items.IndexOf(item);
            _items[index] = entity;
        }
    }
}
