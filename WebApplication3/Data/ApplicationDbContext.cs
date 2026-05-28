using EduDirectory3.Models;
using EduDirectory3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduDirectory3.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public virtual DbSet<InstitucionResultado> InstitucionResultados { get; set; }
        public DbSet<Favorito> Favorito { get; set; }
        public DbSet<Evento> Evento { get; set; }
        public virtual DbSet<ActividadExtracurricularPreferencia> ActividadExtracurricularPreferencia { get; set; }

        public virtual DbSet<ActividadesExtracurriculares> ActividadesExtracurriculares { get; set; }

        public virtual DbSet<Calificacion> Calificacion { get; set; }

        public virtual DbSet<Chatbot> Chatbot { get; set; }

        public virtual DbSet<Coordenada> Coordenada { get; set; }

        public virtual DbSet<Email> Email { get; set; }

        public virtual DbSet<Evaluacion> Evaluacion { get; set; }

        public virtual DbSet<Filtro> Filtro { get; set; }

        public virtual DbSet<FormularioContacto> FormularioContacto { get; set; }

        public virtual DbSet<FormularioPreferencia> FormularioPreferencia { get; set; }

        public virtual DbSet<Institucion> Institucion { get; set; }

        public virtual DbSet<Jornada> Jornada { get; set; }

        public virtual DbSet<JornadaPreferencia> JornadaPreferencia { get; set; }

        public virtual DbSet<Nivel> Nivel { get; set; }

        public virtual DbSet<NivelPreferencia> NivelPreferencia { get; set; }

        public virtual DbSet<Rector> Rector { get; set; }

        public virtual DbSet<ResultadosIcfes> ResultadosIcfes { get; set; }

        public virtual DbSet<Sede> Sede { get; set; }

        public virtual DbSet<Servicio> Servicio { get; set; }

        public virtual DbSet<ServiciosPreferencia> ServiciosPreferencia { get; set; }

        public virtual DbSet<Telefono> Telefono { get; set; }

        public virtual DbSet<Ubicacion> Ubicacion { get; set; }

        public virtual DbSet<Usuario> Usuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InstitucionResultado>().HasNoKey().ToView(null);

            modelBuilder.Entity<ActividadExtracurricularPreferencia>(entity =>
            {
                entity.HasKey(e => e.IdActividadExtracurricularPreferencia).HasName("PK__Activida__1FD12A63972B702B");

                entity.HasOne(d => d.IdFormularioPreferenciaNavigation).WithMany(p => p.ActividadExtracurricularPreferencia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActividadExtracurricularPreferencia_FormularioPreferencia");
            });

            modelBuilder.Entity<ActividadesExtracurriculares>(entity =>
            {
                entity.HasKey(e => e.IdActividadesExtracurriculares).HasName("PK__Activida__4D054355988EB3F5");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.ActividadesExtracurriculares)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActividadesExtracurriculares_Sede");
            });

            modelBuilder.Entity<Calificacion>(entity =>
            {
                entity.HasKey(e => e.IdCalificacion).HasName("PK__Califica__40E4A751755F3CF2");

                entity.HasOne(d => d.IdEvaluacionNavigation).WithMany(p => p.Calificacion)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Calificacion_Evaluacion");
            });

            modelBuilder.Entity<Chatbot>(entity =>
            {
                entity.HasKey(e => e.IdChatbot).HasName("PK__Chatbot__3C18B2DC3D3A8D2B");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Chatbot)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Chatbot_Usuario");
            });

            modelBuilder.Entity<Coordenada>(entity =>
            {
                entity.HasKey(e => e.IdCoordenada).HasName("PK__Coordena__6A279A16F8171065");

                entity.HasOne(d => d.IdUbicacionNavigation).WithMany(p => p.Coordenada)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Coordenada_Ubicacion");
            });

            modelBuilder.Entity<Email>(entity =>
            {
                entity.HasKey(e => e.IdEmail).HasName("PK__Email__E80F8BD4AD379777");

                entity.HasOne(d => d.IdInstitucionNavigation).WithMany(p => p.Email)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Email_Institucion");
            });

            modelBuilder.Entity<Evaluacion>(entity =>
            {
                entity.HasKey(e => e.IdEvaluacion).HasName("PK__Evaluaci__A7EA657C6F2E62AE");

                entity.Property(e => e.Reportado).HasDefaultValue(false);

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Evaluacion)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Evaluacion_Sede");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Evaluacion)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Evaluacion_Usuario");
            });

            modelBuilder.Entity<Filtro>(entity =>
            {
                entity.HasKey(e => e.IdFiltro).HasName("PK__Filtro__0772E7B25D49E454");

                entity.HasOne(d => d.IdChatbotNavigation).WithMany(p => p.Filtro)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Filtro_Chatbot");
            });

            modelBuilder.Entity<FormularioContacto>(entity =>
            {
                entity.HasKey(e => e.IdFormularioContacto).HasName("PK__Formular__EBDCB066ECFFFD80");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.FormularioContacto)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FormularioContacto_Sede");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.FormularioContacto)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FormularioContacto_Usuario");
            });

            modelBuilder.Entity<FormularioPreferencia>(entity =>
            {
                entity.HasKey(e => e.IdFormularioPreferencia).HasName("PK__Formular__8AB0744AE928721C");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.FormularioPreferencia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FormularioPreferencia_Usuario");
            });

            modelBuilder.Entity<Institucion>(entity =>
            {
                entity.HasKey(e => e.IdInstitucion).HasName("PK__Instituc__4231815ADADD810D");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Institucion).HasConstraintName("FK_Institucion_Usuario");
            });

            modelBuilder.Entity<Jornada>(entity =>
            {
                entity.HasKey(e => e.IdJornada).HasName("PK__Jornada__FED2FEE8F46E0BC4");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Jornada)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Jornada_Sede");
            });

            modelBuilder.Entity<JornadaPreferencia>(entity =>
            {
                entity.HasKey(e => e.IdJornadaPreferencia).HasName("PK__JornadaP__DF65943EB2F31AE7");

                entity.HasOne(d => d.IdFormularioPreferenciaNavigation).WithMany(p => p.JornadaPreferencia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_JornadaPreferencia_FormularioPreferencia");
            });

            modelBuilder.Entity<Nivel>(entity =>
            {
                entity.HasKey(e => e.IdNivel).HasName("PK__Nivel__A7F93DEC5CEB3004");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Nivel)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Nivel_Sede");
            });

            modelBuilder.Entity<NivelPreferencia>(entity =>
            {
                entity.HasKey(e => e.IdNivel).HasName("PK__NivelPre__A7F93DECFB8771B2");

                entity.HasOne(d => d.IdFormularioPreferenciaNavigation).WithMany(p => p.NivelPreferencia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NivelPreferencia_FormularioPreferencia");
            });

            modelBuilder.Entity<Rector>(entity =>
            {
                entity.HasKey(e => e.IdRector).HasName("PK__Rector__2BC2872E6C5BA223");

                entity.HasOne(d => d.IdInstitucionNavigation).WithMany(p => p.Rector)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Rector_Institucion");
            });

            modelBuilder.Entity<ResultadosIcfes>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Resultad__3214EC079F84B606");

                entity.HasOne(d => d.IdInstitucionNavigation).WithMany(p => p.ResultadosIcfes)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResultadosIcfes_Institucion");
            });

            modelBuilder.Entity<Sede>(entity =>
            {
                entity.HasKey(e => e.IdSede).HasName("PK__Sede__A7780DFF6824DC6D");

                entity.HasOne(d => d.IdInstitucionNavigation).WithMany(p => p.Sede)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Sede_Institucion");
            });

            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.HasKey(e => e.IdServicio).HasName("PK__Servicio__2DCCF9A2B507655D");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Servicio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Servicio_Sede");
            });

            modelBuilder.Entity<ServiciosPreferencia>(entity =>
            {
                entity.HasKey(e => e.IdServicios).HasName("PK__Servicio__011372997EF7B8B9");

                entity.HasOne(d => d.IdFormularioPreferenciaNavigation).WithMany(p => p.ServiciosPreferencia)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServiciosPreferencia_FormularioPreferencia");
            });

            modelBuilder.Entity<Telefono>(entity =>
            {
                entity.HasKey(e => e.IdTelefono).HasName("PK__Telefono__9B8AC75331B9CB57");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Telefono)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Telefono_Sede");
            });

            modelBuilder.Entity<Ubicacion>(entity =>
            {
                entity.HasKey(e => e.IdUbicacion).HasName("PK__Ubicacio__778CAB1DE79913DD");

                entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Ubicacion)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ubicacion_Sede");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF97754479C4");
            });

        }

    }
}
