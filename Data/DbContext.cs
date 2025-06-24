using Microsoft.EntityFrameworkCore;
using Web_API_for_Contacts_2._0.Models;

namespace Web_API_for_Contacts_2._0.Data
{
    public class ContactsDbContext(DbContextOptions<ContactsDbContext> options) : DbContext(options)
    {
        public DbSet<Person> Person { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Profession> Profession { get; set; }
        public DbSet<Hobby> Hobby { get; set; }
        public DbSet<PersonHobby> PersonHobby { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PersonHobby>()
                .HasKey(ph => new { ph.PersonId, ph.HobbyId });

            modelBuilder.Entity<PersonHobby>()
                .HasOne(ph => ph.Person)
                .WithMany(p => p.PersonHobbies)
                .HasForeignKey(ph => ph.PersonId);

            modelBuilder.Entity<PersonHobby>()
                .HasOne(ph => ph.Hobby)
                .WithMany(h => h.PersonHobbies)
                .HasForeignKey(ph => ph.HobbyId);
        }

    }
}
