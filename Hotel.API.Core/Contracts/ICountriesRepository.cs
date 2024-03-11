using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.API.Core.Contracts;
using Hotel.API.Data;

namespace Hotel.API.Core.Contracts
{
    public interface ICountriesRepository : IGenericRepository<Country>
    {
        Task<Country> GetDetails(int id);
    }
}