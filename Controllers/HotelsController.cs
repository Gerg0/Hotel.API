using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hotel.API.Contracts;
using Hotel.API.Dtos.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
         private readonly IMapper _mapper;
        private readonly IHotelsRepository _hotelsRepository;

        public HotelsController(IHotelsRepository hotelsRepository, IMapper mapper)
        {
            _mapper = mapper;
            _hotelsRepository = hotelsRepository;
        }
        // GET: api/Hotels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
        {
            var countries = await _hotelsRepository.GetAllAsync();
            var result = _mapper.Map<List<HotelDto>>(countries);
            return result;
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            var hotel = await _hotelsRepository.GetAsync(id);
            if(hotel == null) return NotFound();
            var result = _mapper.Map<HotelDto>(hotel);
            return result;
        }

        // PUT: api/Hotels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, HotelDto updateHotelDto)
        {
            if (id != updateHotelDto.Id) return BadRequest("Invalid Record Id");
            
            var hotel = await _hotelsRepository.GetAsync(id);
            if(hotel == null) return NotFound();

            _mapper.Map(updateHotelDto, hotel);
            
            try
            {
                await _hotelsRepository.UpdateAsync(hotel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!await HotelExists(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // POST: api/Hotels
        [HttpPost]
        public async Task<ActionResult<Hotel.API.Data.Hotel>> PostHotel(CreateHotelDto createHotel)
        {
            var hotel = _mapper.Map<Hotel.API.Data.Hotel>(createHotel);

            await _hotelsRepository.AddAsync(hotel);

            return CreatedAtAction("GetHotel", new{id=hotel.Id},hotel);
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id){
            var hotel = await _hotelsRepository.GetAsync(id);
            if(hotel == null) return NotFound();

            await _hotelsRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> HotelExists(int id){
            return await _hotelsRepository.Exists(id);
        }
    }
}