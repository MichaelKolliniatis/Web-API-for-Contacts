using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonHobbyController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<PersonHobbyDisplayDto>>> GetPersonHobbies()
        {
            var personHobbies = await _context.PersonHobby
                .Include(ph => ph.Person)
                .Include(ph => ph.Hobby)
                .ToListAsync();

            var result = _mapper.Map<List<PersonHobbyDisplayDto>>(personHobbies);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<PersonHobbyDisplayDto>>> GetPersonHobbyByPersonId(int id)
        {
            var personHobbies = await _context.PersonHobby
                .Include(ph => ph.Person)
                .Include(ph => ph.Hobby)
                .Where(ph => ph.PersonId == id)
                .ToListAsync();

            if (personHobbies == null || personHobbies.Count == 0)
            {
                return NotFound($"No hobbies found for person with ID {id}.");
            }

            var result = _mapper.Map<List<PersonHobbyDisplayDto>>(personHobbies);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddPersonHobby([FromBody] CrDePersonHobbyDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person.FindAsync(request.PersonId);
            if (person == null || person.FirstName != request.FirstName || person.LastName != request.LastName)
            {
                return NotFound($"No person with id {request.PersonId} and name {request.FirstName} {request.LastName}.");
            }

            var hobby = await _context.Hobby.FirstOrDefaultAsync(h => h.Name.ToLower() == request.HobbyName.ToLower());
            if (hobby == null)
            {
                hobby = new Hobby { Name = request.HobbyName };
                _context.Hobby.Add(hobby);
                await _context.SaveChangesAsync();
            }

            var existing = await _context.PersonHobby
                .AnyAsync(ph => ph.PersonId == person.Id && ph.HobbyId == hobby.Id);

            if (existing)
            {
                return Conflict($"{person.FirstName} {person.LastName} already likes {request.HobbyName}");
            }

            var personHobby = new PersonHobby
            {
                PersonId = person.Id,
                HobbyId = hobby.Id
            };

            _context.PersonHobby.Add(personHobby);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPersonHobbyByPersonId), new { id = person.Id }, $"{person.FirstName} {person.LastName} likes {request.HobbyName}");
        }

        [HttpDelete]
        public async Task<ActionResult> DeletePersonHobby([FromBody] CrDePersonHobbyDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person.FindAsync(request.PersonId);
            if (person == null || person.FirstName != request.FirstName || person.LastName != request.LastName)
            {
                return NotFound($"No person with id {request.PersonId} and name {request.FirstName} {request.LastName}.");
            }

            var hobby = await _context.Hobby.FirstOrDefaultAsync(h => h.Name.ToLower() == request.HobbyName.ToLower());
            if (hobby == null)
            {
                return NotFound("No such hobby found.");
            }

            var personHobby = await _context.PersonHobby
                .FirstOrDefaultAsync(ph => ph.PersonId == person.Id && ph.HobbyId == hobby.Id);

            if (personHobby == null)
            {
                return NotFound($"{person.FirstName} {person.LastName} doesn't like {hobby.Name}.");
            }

            _context.PersonHobby.Remove(personHobby);
            await _context.SaveChangesAsync();

            return Ok($"Done. {person.FirstName} {person.LastName} doesn't like {request.HobbyName} anymore");
        }

    }
}
