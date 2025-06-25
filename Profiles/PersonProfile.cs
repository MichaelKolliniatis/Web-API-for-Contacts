using AutoMapper;
using Web_API_for_Contacts_2._0.Models;
using Web_API_for_Contacts_2._0.Dtos;

namespace Web_API_for_Contacts_2._0.Profiles
{
    public class PersonProfile : Profile
    {
        public PersonProfile()
        {
            CreateMap<Person, PersonDto>()
            .ForMember(dest => dest.CountryName, 
                opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : "Unknown"))
            .ForMember(dest => dest.ProfessionName, 
                opt => opt.MapFrom(src => src.Profession != null ? src.Profession.Name : "Unknown"))
            .ForMember(dest => dest.Hobbies, 
                opt => opt.MapFrom(src =>
                    src.PersonHobbies != null
                        ? src.PersonHobbies
                            .Where(ph => ph.Hobby != null)
                            .Select(ph => ph.Hobby!.Name)
                            .ToList()
                    : new List<string>()))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Email) ? src.Email : "Unknown"))
            .ForMember(dest => dest.Phone,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Phone) ? src.Phone : "Unknown"));

        }
    }
}
