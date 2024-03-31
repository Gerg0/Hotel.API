using Asp.Versioning;
using AutoMapper;
using Hotel.API.Core.Contracts;
using Hotel.API.Core.Dtos;
using Hotel.API.Core.Dtos.Hotel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
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
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
        {
            var hotels = await _hotelsRepository.GetAllAsync<HotelDto>();
            return Ok(hotels);
        }

        // GET: api/Hotels/?StartIndex=0&PageSize=25&PageNumber=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<HotelDto>>> GetPagedHotels([FromQuery] QueryParameters queryParameters)
        {
            var pagedHotelsResult = await _hotelsRepository.GetAllAsync<HotelDto>(queryParameters);
            return pagedHotelsResult;
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            var hotel = await _hotelsRepository.GetAsync<HotelDto>(id);
            return hotel;
        }

        // PUT: api/Hotels/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, HotelDto hotelDto)
        {
            if (id != hotelDto.Id) return BadRequest("Invalid Record Id");
            
            
            try
            {
                await _hotelsRepository.UpdateAsync(id,hotelDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!await HotelExists(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // POST: api/Hotels
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<HotelDto>> PostHotel(CreateHotelDto createHotel)
        {
            var hotel = await _hotelsRepository.AddAsync<CreateHotelDto, HotelDto>(createHotel);
            return CreatedAtAction(nameof(PostHotel), new{id=hotel.Id},hotel);
        }

        // DELETE: api/Hotels/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id){
            await _hotelsRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> HotelExists(int id){
            return await _hotelsRepository.Exists(id);
        }
    }
}