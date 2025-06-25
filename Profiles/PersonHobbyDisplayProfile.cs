using AutoMapper;
using Web_API_for_Contacts_2._0.Dtos;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Profiles
{
    public class PersonHobbyDisplayProfile : Profile
    {
        public PersonHobbyDisplayProfile()
        {
            CreateMap<PersonHobby, PersonHobbyDisplayDto>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                    $"{src.Person!.FirstName} {src.Person.LastName} likes {src.Hobby!.Name}"
                ));
        }
    }
}
