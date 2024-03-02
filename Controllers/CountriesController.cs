using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hotel.API.Data;
using Hotel.API.Dtos.Country;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;

        public CountriesController(ICountriesRepository countriesRepository, IMapper mapper)
        {
            _mapper = mapper;
            _countriesRepository = countriesRepository;
        }
        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();
            var result = _mapper.Map<List<GetCountryDto>>(countries);
            return result;
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);
            if(country == null) return NotFound();
            var result = _mapper.Map<CountryDto>(country);
            return result;
        }

        // PUT: api/Countries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id) return BadRequest("Invalid Record Id");
            
            var country = await _countriesRepository.GetAsync(id);
            if(country == null) return NotFound();

            _mapper.Map(updateCountryDto, country);
            
            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!await CountryExists(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // POST: api/Countries
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountry)
        {
            var country = _mapper.Map<Country>(createCountry);

            await _countriesRepository.AddAsync(country);

            return CreatedAtAction("GetCountry", new{id=country.Id},country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id){
            var country = await _countriesRepository.GetAsync(id);
            if(country == null) return NotFound();

            await _countriesRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> CountryExists(int id){
            return await _countriesRepository.Exists(id);
        }
    }
}