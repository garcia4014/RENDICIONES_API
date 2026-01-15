using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models.Access
{
    public class AccessDBContext : DbContext
    {
        public AccessDBContext(DbContextOptions<AccessDBContext> options) : base(options) { }

        public DbSet<Perfil> Perfil { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<Perfil_Usuario> Perfil_Usuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Personal>().HasKey(p => p.IdDocumento);

            modelBuilder.Entity<Perfil_Usuario>()
                .HasOne(pu => pu.Perfil) //Relacion hacia perfil;
                .WithMany(p => p.Perfil_Usuarios) //Relacion inversa
                .HasForeignKey(pu => pu.idPerfil); // Especificar clave foránea


            base.OnModelCreating(modelBuilder);
        }
    }
}
