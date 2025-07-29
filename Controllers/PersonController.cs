using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Data;
using Web_API_for_Contacts_2._0.Models;
using AutoMapper.QueryableExtensions;

namespace Web_API_for_Contacts_2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController(ContactsDbContext context, IMapper mapper) : ControllerBase
    {

        private readonly ContactsDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<PersonDto>>> GetPersons()
        {
            var persons = await _context.Person
                .Include(p => p.Country)
                .Include(p => p.Profession)
                .Include(p => p.PersonHobbies)
                    .ThenInclude(ph => ph.Hobby)
                .ToListAsync();
            //var persons = await _context.Person
            //    .ProjectTo<PersonDto>(_mapper.ConfigurationProvider)
            //    .ToListAsync();


            var peopleDto = _mapper.Map<List<PersonDto>>(persons);

            return Ok(peopleDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDto>> GetPersonById(int id)
        {
            var person = await _context.Person
                .Include(p => p.Country)
                .Include(p => p.Profession)
                .Include(p => p.PersonHobbies)
                    .ThenInclude(ph => ph.Hobby)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
                return NotFound($"No person found with ID = {id}");

            var personDto = _mapper.Map<PersonDto>(person);

            return Ok(personDto);
        }

        [HttpGet("by-country-name/{countryName}")]
        public async Task<ActionResult<List<PersonDto>>> GetPersonsByCountryName(string countryName)
        {

            var country = await _context.Country
                .FirstOrDefaultAsync(c => c.Name.ToLower() == countryName.ToLower());

            if (country == null)
            {
                return NotFound(new { message = $"No country {countryName} in our database." });
            }

            var people = await _context.Person
                .Include(p => p.Country)
                .Include(p => p.Profession)
                .Include(p => p.PersonHobbies)
                    .ThenInclude(ph => ph.Hobby)
                .Where(p => p.Country != null && p.Country.Name.ToLower() == countryName.ToLower())
                .ToListAsync();

            if (people.Count == 0)
                return NotFound($"No people found in country '{countryName}'");

            var peopleDto = _mapper.Map<List<PersonDto>>(people);
            return Ok(peopleDto);
        }

        [HttpGet("by-profession-name/{professionName}")]
        public async Task<ActionResult<List<PersonDto>>> GetPersonsByProfessionName(string professionName)
        {

            var country = await _context.Profession
                .FirstOrDefaultAsync(c => c.Name.ToLower() == professionName.ToLower());

            if (country == null)
            {
                return NotFound(new { message = $"No profession {professionName} in our database." });
            }

            var people = await _context.Person
                .Include(p => p.Country)
                .Include(p => p.Profession)
                .Include(p => p.PersonHobbies)
                    .ThenInclude(ph => ph.Hobby)
                .Where(p => p.Profession != null && p.Profession.Name.ToLower() == professionName.ToLower())
                .ToListAsync();

            if (people.Count == 0)
                return NotFound($"No people found with profession '{professionName}'");

            var peopleDto = _mapper.Map<List<PersonDto>>(people);
            return Ok(peopleDto);
        }

        [HttpGet("by-hobby-name/{hobbyName}")]
        public async Task<ActionResult<List<PersonDto>>> GetPersonsByHobbyName(string hobbyName)
        {
            var hobby = await _context.Hobby
                .FirstOrDefaultAsync(h => h.Name.ToLower() == hobbyName.ToLower());

            if (hobby == null)
            {
                return NotFound(new { message = $"No hobby '{hobbyName}' in our database." });
            }

            var people = await _context.Person
                .Include(p => p.Country)
                .Include(p => p.Profession)
                .Include(p => p.PersonHobbies)
                    .ThenInclude(ph => ph.Hobby)
                .Where(p => p.PersonHobbies != null &&
                            p.PersonHobbies.Any(ph => ph.Hobby != null &&
                                                      ph.Hobby.Name.ToLower() == hobbyName.ToLower()))
                .ToListAsync();

            if (people.Count == 0)
                return NotFound($"No people found with hobby '{hobbyName}'");

            var peopleDto = _mapper.Map<List<PersonDto>>(people);
            return Ok(peopleDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson([FromBody] CrUpPersonDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Country? country = null;
            Profession? profession = null;

            if (!string.IsNullOrWhiteSpace(dto.CountryName))
            {
                country = await _context.Country
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.CountryName.ToLower());

                if (country == null)
                {
                    country = new Country { Name = dto.CountryName };
                    _context.Country.Add(country);
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.ProfessionName))
            {
                profession = await _context.Profession
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.ProfessionName.ToLower());

                if (profession == null)
                {
                        profession = new Profession { Name = dto.ProfessionName };
                        _context.Profession.Add(profession);
                        await _context.SaveChangesAsync();
                }
            }

            var person = new Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                CountryId = country?.Id,
                ProfessionId = profession?.Id
            };

            _context.Person.Add(person);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPersonById), new { id = person.Id }, person);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] CrUpPersonDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person.FindAsync(id);
            if (person == null)
                return NotFound(new { message = $"Person with id {id} not found." });

            Country? country = null;
            Profession? profession = null;

            if (!string.IsNullOrWhiteSpace(dto.CountryName))
            {
                country = await _context.Country
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.CountryName.ToLower());

                if (country == null)
                {
                    country = new Country { Name = dto.CountryName };
                    _context.Country.Add(country);
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.ProfessionName))
            {
                profession = await _context.Profession
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.ProfessionName.ToLower());

                if (profession == null)
                {
                    profession = new Profession { Name = dto.ProfessionName };
                    _context.Profession.Add(profession);
                    await _context.SaveChangesAsync();
                }
            }

            person.FirstName = dto.FirstName;
            person.LastName = dto.LastName;
            person.Email = dto.Email;
            person.Phone = dto.Phone;
            person.CountryId = country?.Id;
            person.ProfessionId = profession?.Id;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Person updated successfully." });
        }


        [HttpDelete]
        public async Task<IActionResult> DeletePerson([FromBody] DeletePersonDto deleteDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = await _context.Person
                .FirstOrDefaultAsync(p => p.Id == deleteDto.Id &&
                                          p.FirstName.ToLower() == deleteDto.FirstName.ToLower() &&
                                          p.LastName.ToLower() == deleteDto.LastName.ToLower());

            if (person == null)
            {
                return NotFound(new { message = $"No person found with Id {deleteDto.Id} and name {deleteDto.FirstName} {deleteDto.LastName}." });
            }

            if (person.PersonHobbies.Any())
            {
                _context.PersonHobby.RemoveRange(person.PersonHobbies);
            }

            _context.Person.Remove(person);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"'{person.FirstName} {person.LastName}' deleted successfully." });
        }

    }

}