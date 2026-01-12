using Microsoft.EntityFrameworkCore;
using FbiApi.Models.Entities;

namespace FbiApi.Data;

// Moștenim din DbContext
public class AppDbContext : DbContext
{
    // Constructorul primește opțiunile (connection string, provider) și le dă la părinte
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Aici definim tabelele. Numele proprietății = Numele tabelului în SQL.
    public DbSet<WantedPerson> WantedPersons { get; set; }

    public DbSet<WantedSubject> WantedSubjects { get; set; }

    public DbSet<WantedImage> WantedImages {get; set;}

    public DbSet<WantedFile> WantedFiles {get; set;}

    public DbSet<WantedAlias> WantedAliases { get; set; }
}