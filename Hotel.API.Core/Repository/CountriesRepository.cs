using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hotel.API.Core.Contracts;
using Hotel.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.API.Core.Repository
{
    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        private readonly HotelDbContext _context;

        public CountriesRepository(HotelDbContext context, IMapper mapper) : base(context,mapper)
        {
            _context = context;
        }

        public async Task<Country> GetDetails(int id)
        {
            return await _context.Countries.Include(x=>x.Hotels).FirstOrDefaultAsync(x=>x.Id == id);
        }
    }
}