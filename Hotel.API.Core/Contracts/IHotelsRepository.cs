using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.API.Core.Contracts
{
    public interface IHotelsRepository : IGenericRepository<Hotel.API.Data.Hotel>
    {
        
    }
}