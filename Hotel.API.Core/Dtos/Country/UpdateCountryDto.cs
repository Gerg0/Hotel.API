using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.API.Dtos.Country
{
    public class UpdateCountryDto : BaseCountryDto
    {
        public int Id { get; set; }
    }
}