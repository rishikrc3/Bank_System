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
    }
}
