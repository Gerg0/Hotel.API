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
        private readonly HotelDbContext _context;
        private readonly IMapper _mapper;

        public CountriesController(HotelDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            var countries = await _context.Countries.ToListAsync();
            var result = _mapper.Map<List<GetCountryDto>>(countries);
            return result;
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            var country = await _context.Countries.Include(x=>x.Hotels).FirstOrDefaultAsync(x=>x.Id == id);
            if(country == null) return NotFound();
            var result = _mapper.Map<CountryDto>(country);
            return result;
        }

        // PUT: api/Countries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id) return BadRequest("Invalid Record Id");
            
            // _context.Entry(country).State = EntityState.Modified;
            var country = await _context.Countries.FindAsync(id);
            if(country == null) return NotFound();

            _mapper.Map(updateCountryDto, country);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!CountryExists(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // POST: api/Countries
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountry)
        {
            var country = _mapper.Map<Country>(createCountry);

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new{id=country.Id},country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id){
            var country = await _context.Countries.FindAsync(id);
            if(country == null) return NotFound();

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool CountryExists(int id){
            return _context.Countries.Any(e=>e.Id == id);
        }
    }
}