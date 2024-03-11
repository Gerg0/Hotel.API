using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hotel.API.Data;
using Hotel.API.Core.Dtos.Country;
using Hotel.API.Core.Dtos.Hotel;
using Hotel.API.Core.Dtos.User;

namespace Hotel.API.Core.Configurations
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

            CreateMap<ApiUser, ApiUserDto>().ReverseMap();
        }
    }
}