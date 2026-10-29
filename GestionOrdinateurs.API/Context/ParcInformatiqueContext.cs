using GestionOrdinateurs.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Context;

public class ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) : DbContext(options)
{
    public DbSet<Ordinateur> Ordinateurs { get; set; }
    public DbSet<Reparation> Reparations { get; set; }
    public DbSet<User> Users { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=ParcInformatique.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // option 1 pour configurer qu'ordinateurId soit bien une clé primaire de l'entité Ordinateur et clé étragère de l'entité Reparation.
        modelBuilder.Entity<Reparation>()
            .HasOne(x => x.UnOrdinateur)
            .WithMany(y => y.Reparations)
            .HasForeignKey(x => x.OrdinateurId)
            .HasPrincipalKey(o => o.Id);

        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(o => o.Caracteristiques);

        modelBuilder.Entity<Ordinateur>()
            .HasMany(o => o.Reparations)
            .WithOne(r => r.UnOrdinateur);

        modelBuilder.Entity<Ordinateur>().Property(o => o.Stock).HasDefaultValue(10);
        modelBuilder.Entity<Ordinateur>().Property(o => o.Prix).HasDefaultValue(500);

        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
    }
}