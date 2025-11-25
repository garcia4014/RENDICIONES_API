using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models.General
{
    public class GeneralDBContext : DbContext
    {
        public GeneralDBContext(DbContextOptions<GeneralDBContext> options) : base(options) { }

        public DbSet<MVT_EMPPLA> Empleados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MVT_EMPPLA>().ToTable("@MVT_EMPPLA", "dbo");

            modelBuilder.Entity<MVT_EMPPLA>().HasKey(e => new { e.Code });
            base.OnModelCreating(modelBuilder);
        }
    }
}
