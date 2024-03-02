using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.API.Contracts;
using Hotel.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.API.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelDbContext _context;

        public GenericRepository(HotelDbContext context)
        {
            this._context = context;
        }
        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            var result = await _context.Set<T>().ToListAsync();
            return result;
        }

        public async Task<T> GetAsync(int? id)
        {
            if (id is null) return null;

            return await _context.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}