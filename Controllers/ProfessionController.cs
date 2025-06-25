using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessionController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<Profession>>> GetProfessions()
        {
            return Ok(await _context.Profession.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Country>> CreateProfession([FromBody] CreateProfessionDto newProfessionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProfession = await _context.Profession
                .FirstOrDefaultAsync(c => c.Name.ToLower() == newProfessionDto.Name.ToLower());

            if (existingProfession != null)
            {
                return Conflict(new { message = $"{newProfessionDto.Name} already exists." });
            }

            var newProfession = _mapper.Map<Profession>(newProfessionDto);

            _context.Profession.Add(newProfession);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{newProfession.Name} added with id {newProfession.Id}." });
        }

        [HttpDelete] 
        public async Task<IActionResult> DeleteProfession([FromBody] Profession professionToDelete)
        {
            if (professionToDelete == null || professionToDelete.Id == 0 || string.IsNullOrWhiteSpace(professionToDelete.Name))
            {
                return BadRequest(new { message = "Invalid Id or Name." });
            }

            var profession = await _context.Profession
                .FirstOrDefaultAsync(c => c.Id == professionToDelete.Id && c.Name.ToLower() == professionToDelete.Name.ToLower());

            if (profession == null)
            {
                return NotFound(new { message = $"No profession with Id {professionToDelete.Id} and Name '{professionToDelete.Name}'." });
            }

            var personWithProfession = await _context.Person
                .FirstOrDefaultAsync(p => p.ProfessionId == professionToDelete.Id);

            if (personWithProfession != null)
            {
                return Conflict(new { message = $"Cannot delete the profession '{professionToDelete.Name}' because there is at least one person associated with it." });
            }

            _context.Profession.Remove(profession);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{profession.Name} deleted successfully." });
        }


    }
}
