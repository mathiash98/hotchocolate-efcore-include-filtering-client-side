using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication1.Models;

namespace WebApplication1;

public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(Console.WriteLine);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonAction>()
                .Property(pa => pa.Action)
                .HasConversion(new EnumToStringConverter<PersonActionTypeDto>());
        }

        public DbSet<PersonAction> PersonActions { get; set; }
        public DbSet<Person> Persons { get; set; }


        /// <summary>
        /// Migrates the database and seeds
        /// </summary>
        public void MigrateAndSeed()
        {
            Migrate();
            Seed();
        }

        private void Migrate()
        {
            Database.Migrate();
        }

        private void Seed()
        {
            const int numPersons = 100;
            const int numPersonActions = 1000;

            if (Persons.Any())
            {
                return;
            }
            
            var persons = Enumerable.Range(0, numPersons).Select(i => new Person()
            {
                FullName = $"Person {i}",
                Actions = Enumerable.Range(0, numPersonActions).Select(j => new PersonAction()
                {
                    Action = PersonActionTypeDto.Disembark,
                    DateTimeUtc = DateTime.UtcNow,
                }).ToList(),
            });
            Persons.AddRange(persons);

            SaveChanges();
        }
    }