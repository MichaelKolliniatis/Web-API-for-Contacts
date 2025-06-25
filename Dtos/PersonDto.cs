namespace Web_API_for_Contacts_2._0.Dtos
{
    public class PersonDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? CountryName { get; set; }
        public string? ProfessionName { get; set; }
        public List<string> Hobbies { get; set; } = new();

    }
}
