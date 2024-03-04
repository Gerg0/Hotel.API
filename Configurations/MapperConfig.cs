using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hotel.API.Data;
using Hotel.API.Dtos.Country;
using Hotel.API.Dtos.Hotel;

namespace Hotel.API.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Country, CreateCountryDto>().ReverseMap();
            CreateMap<Country, GetCountryDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Country, UpdateCountryDto>().ReverseMap();

            CreateMap<Hotel.API.Data.Hotel, HotelDto>().ReverseMap();
            CreateMap<Hotel.API.Data.Hotel, CreateHotelDto>().ReverseMap();
        }
    }
}