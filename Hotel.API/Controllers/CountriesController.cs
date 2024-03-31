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
            var countries = await _countriesRepository.GetAllAsync<GetCountryDto>();
            return Ok(countries);
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

            return Ok(country);
        }

        // PUT: api/Countries/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id) return BadRequest("Invalid Record Id");
            
          
            try
            {
                await _countriesRepository.UpdateAsync(id,updateCountryDto);
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
        public async Task<ActionResult<CountryDto>> PostCountry(CreateCountryDto createCountry)
        {
            var country = await _countriesRepository.AddAsync<CreateCountryDto, GetCountryDto>(createCountry);      
            return CreatedAtAction("GetCountry", new{id=country.Id},country);
        }

        // DELETE: api/Countries/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id){
            await _countriesRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> CountryExists(int id){
            return await _countriesRepository.Exists(id);
        }
    }
}