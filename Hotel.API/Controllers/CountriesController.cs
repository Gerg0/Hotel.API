using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using AutoMapper;
using Hotel.API.Data;
using Hotel.API.Core.Dtos;
using Hotel.API.Core.Dtos.Country;
using Hotel.API.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Hotel.API.Core.Contracts;

namespace Hotel.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<CountriesController> _logger;
        private readonly ICountriesRepository _countriesRepository;

        public CountriesController(ICountriesRepository countriesRepository, IMapper mapper, ILogger<CountriesController> logger)
        {
            _mapper = mapper;
            _logger = logger;
            _countriesRepository = countriesRepository;
        }

        // GET: api/Countries/
        [HttpGet("GetAll")]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();
            var result = _mapper.Map<List<GetCountryDto>>(countries);
            return result;
        }

        // GET: api/Countries/?StartIndex=0&PageSize=25&PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetCountryDto>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
        {
            var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);
            return pagedCountriesResult;
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);
            if(country == null) {
                throw new NotFoundException(nameof(GetCountry),id);
            }
            var result = _mapper.Map<CountryDto>(country);
            return Ok(result);
        }

        // PUT: api/Countries/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id) return BadRequest("Invalid Record Id");
            
            var country = await _countriesRepository.GetAsync(id);
            if(country == null) throw new NotFoundException(nameof(PutCountry),id);

            _mapper.Map(updateCountryDto, country);
            
            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!await CountryExists(id)) {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        // POST: api/Countries
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountry)
        {
            var country = _mapper.Map<Country>(createCountry);

            await _countriesRepository.AddAsync(country);

            return CreatedAtAction("GetCountry", new{id=country.Id},country);
        }

        // DELETE: api/Countries/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id){
            var country = await _countriesRepository.GetAsync(id);

            if(country == null) throw new NotFoundException(nameof(DeleteCountry),id);

            await _countriesRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> CountryExists(int id){
            return await _countriesRepository.Exists(id);
        }
    }
}