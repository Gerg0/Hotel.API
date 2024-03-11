using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.API.Core.Dtos.Hotel;

namespace Hotel.API.Core.Dtos.Country
{
    public class GetCountryDto : BaseCountryDto
    {
        public int Id { get; set; }
    }
}