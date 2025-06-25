using System.Text.Json.Serialization;

namespace Web_API_for_Contacts_2._0.Models

{
    public class Hobby
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<PersonHobby> PersonHobbies { get; set; } = new List<PersonHobby>();
    }
}
