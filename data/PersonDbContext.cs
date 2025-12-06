using Microsoft.EntityFrameworkCore;
using assecor_assesment_api.Models;

namespace assecor_assesment_api.Data
{
    public class PersonDbContext : DbContext
    {
        public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Persons");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);
                
                entity.Property(e => e.LastName)
                    .HasMaxLength(100);
                
                entity.Property(e => e.Address)
                    .HasMaxLength(255);
                
                entity.Property(e => e.Color);
            });

            // Seed some initial data for testing
            modelBuilder.Entity<Person>().HasData(
                new Person { Id = 1, FirstName = "Hans", LastName = "Müller", Address = "67742 Lauterecken", Color = 1 },
                new Person { Id = 2, FirstName = "Peter", LastName = "Petersen", Address = "18439 Stralsund", Color = 2 },
                new Person { Id = 3, FirstName = "Johnny", LastName = "Johnson", Address = "88888 made up", Color = 3 }
            );
        }
    }
}
