using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Auth.Entities;
using System.Reflection.Emit;

namespace Auth.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Ativos = Set<Ativo>();
            AtivoxUsers = Set<AtivoxUser>();
            Chamados = Set<Chamado>();
            DataPauses = Set<DataPause>();
            fotoUrls = Set<FotoUrl>();
            UserxUsers = Set<UserxUser>();
            UserAdminRolescontrols = Set<UserAdminRolescontrol>();
        }

        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<AtivoxUser> AtivoxUsers { get; set; }
        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<DataPause> DataPauses { get; set; }
        public DbSet<FotoUrl> fotoUrls { get; set; }
        public DbSet<UserxUser> UserxUsers { get; set; }
        public DbSet<UserAdminRolescontrol> UserAdminRolescontrols { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();

            });   
            
            builder.Entity<Ativo>(entity =>
            {
                entity.HasIndex(e => e.CodigoUnico).IsUnique();

            });

            builder.Entity<Ativo>(entity =>
            {
                entity.HasIndex(e => e.Endereco).IsUnique();

            });


            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(p => p.CodigoUnico)
                    .IsUnique();               
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(l => l.Cpf)
                    .IsUnique();
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(k => k.PhoneNumber)
                    .IsUnique();
            });

            builder.Entity<Chamado>()
                .HasOne(c => c.Ativo)
                .WithMany(a => a.Chamados)
                .HasForeignKey(c => c.Ativo_Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AtivoxUser>()
                .HasOne(au => au.Ativo)
                .WithMany(a => a.AtivoxUsers)
                .HasForeignKey(au => au.Ativo_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DataPause>()
                .HasOne(dp => dp.Chamado)
                .WithMany(c => c.Data_pause)
                .HasForeignKey(dp => dp.Chamado_id)
                .OnDelete(DeleteBehavior.Cascade);           

            builder.Entity<UserAdminRolescontrol>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserAdminRolescontrols)
                .HasForeignKey(ur => ur.RoleId);

            builder.Entity<UserxUser>()
                .HasOne(uu => uu.UserAdmin)
                .WithMany()
                .HasForeignKey(uu => uu.User_Admin_Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserxUser>()
                .HasOne(uu => uu.UserAgregado)
                .WithMany()
                .HasForeignKey(uu => uu.User_Agregado_Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DataPause>()
                .HasOne(dp => dp.Chamado)
                .WithMany()
                .HasForeignKey(uu => uu.Chamado_id)
                .OnDelete(DeleteBehavior.ClientSetNull);

        }
    }
}
