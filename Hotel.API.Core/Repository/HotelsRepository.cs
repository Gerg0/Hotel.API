using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hotel.API.Core.Contracts;
using Hotel.API.Data;

namespace Hotel.API.Core.Repository
{
    public class HotelsRepository : GenericRepository<Hotel.API.Data.Hotel>, IHotelsRepository
    {
        public HotelsRepository(HotelDbContext context, IMapper mapper) : base(context,mapper)
        {
        }
    }
}