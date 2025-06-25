using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Models;
using System.Diagnostics.Metrics;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<Country>>> GetCountries()
        {
            return Ok(await _context.Country.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Country>> CreateCountry([FromBody] CreateCountryDto newCountryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCountry = await _context.Country
                .FirstOrDefaultAsync(c => c.Name.ToLower() == newCountryDto.Name.ToLower());

            if (existingCountry != null)
            {
                return Conflict(new { message = $"'{newCountryDto.Name}' already exists." });
            }

            var newCountry = _mapper.Map<Country>(newCountryDto);

            _context.Country.Add(newCountry);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{newCountry.Name} added with id {newCountry.Id}." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCountry([FromBody] Country countryToDelete)
        {
            if (countryToDelete == null || countryToDelete.Id == 0 || string.IsNullOrWhiteSpace(countryToDelete.Name))
            {
                return BadRequest(new { message = "Invalid Id or Name." });
            }

            var country = await _context.Country
                .FirstOrDefaultAsync(c => c.Id == countryToDelete.Id && c.Name.ToLower() == countryToDelete.Name.ToLower());

            if (country == null)
            {
                return NotFound(new { message = $"No country with Id {countryToDelete.Id} and Name '{countryToDelete.Name}'." });
            }

            var personWithCountry = await _context.Person
                .FirstOrDefaultAsync(p => p.CountryId == countryToDelete.Id);

            if (personWithCountry != null)
            {
                return Conflict(new { message = $"Cannot delete the country '{countryToDelete.Name}' because there is at least one person associated with it." });
            }

            _context.Country.Remove(country);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"'{country.Name}' deleted successfully." });
        }


    }
}
