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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // obrigatório: configura as tabelas do Identity

        builder.Entity<Cliente>()
            .HasOne(c => c.Usuario).WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId);

        builder.Entity<Profissional>()
            .HasOne(p => p.Usuario).WithOne()
            .HasForeignKey<Profissional>(p => p.UsuarioId);
    }
}