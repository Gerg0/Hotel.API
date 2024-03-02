using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.API.Contracts;
using Hotel.API.Data;

namespace Hotel.API.Controllers
{
    public interface ICountriesRepository : IGenericRepository<Country>
    {
        Task<Country> GetDetails(int id);
    }
}