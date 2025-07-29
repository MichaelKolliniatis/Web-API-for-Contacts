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

        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountryById(int id)
        {
            var country = await _context.Country.FindAsync(id);

            if (country == null)
                return NotFound(new { message = $"There is no country with id {id}"});

            return Ok(country);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCountry([FromBody] CreateCountryDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCountry = await _context.Country
                .FirstOrDefaultAsync(c => c.Name.ToLower() == input.Name.ToLower());

            if (existingCountry != null)
            {
                return Conflict(new { message = $"'{input.Name}' already exists." });
            }

            var newCountry = _mapper.Map<Country>(input);

            _context.Country.Add(newCountry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCountryById), new { id = newCountry.Id }, newCountry);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCountry([FromBody] Country countryToDelete)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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