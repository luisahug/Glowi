using Glowi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();
    public DbSet<Servico> Servico => Set<Servico>();
    public DbSet<Disponibilidade> Disponibilidades => Set<Disponibilidade>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>()
            .HasOne(c => c.Usuario).WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId);

        builder.Entity<Profissional>()
            .HasOne(p => p.Usuario).WithOne()
            .HasForeignKey<Profissional>(p => p.UsuarioId);

        builder.Entity<Servico>().Property(s => s.Preco).HasPrecision(10, 2);
    }
}