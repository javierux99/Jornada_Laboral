// Puente entre el código C# y la base de datos SQLite
using JornadaLaboral.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JornadaLaboral.API.Data;

public class AppDbContext : DbContext
{
    // Constructor: recibe la configuración de conexión desde Program.cs
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Representa la tabla "Trabajadores" en la BD
    public DbSet<Trabajador> Trabajadores { get; set; }

    // Representa la tabla "Jornadas" en la BD
    public DbSet<Jornada> Jornadas { get; set; }

    // Se ejecuta una sola vez al crear la BD: define reglas y carga datos iniciales
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Evita que un trabajador tenga dos registros abiertos el mismo dia
        modelBuilder.Entity<Jornada>()
            .HasIndex(j => new { j.TrabajadorId, j.Fecha })
            .HasDatabaseName("IX_Jornada_Trabajador_Fecha");

        // Define la relación: un Trabajador tiene muchas Jornadas
        modelBuilder.Entity<Jornada>()
            .HasOne(j => j.Trabajador)
            .WithMany(t => t.Jornadas)
            .HasForeignKey(j => j.TrabajadorId)
            .OnDelete(DeleteBehavior.Cascade); // Si se borra un trabajador, se borran sus jornadas

        // Inserta los 6 trabajadores de prueba la primera vez que arranca la aplicación
        modelBuilder.Entity<Trabajador>().HasData(
            new Trabajador { Id = 1, Codigo = "ADMIN01", Nombre = "Javier Aguilar", Cargo = "Desarrollador" },
            new Trabajador { Id = 2, Codigo = "T001", Nombre = "María Gómez", Cargo = "Diseñadora" },
            new Trabajador { Id = 3, Codigo = "T002", Nombre = "Pepito Perez", Cargo = "Analista" },
            new Trabajador { Id = 4, Codigo = "T003", Nombre = "Ana Martínez", Cargo = "Gerente" },
            new Trabajador { Id = 5, Codigo = "T004", Nombre = "Luis Rodríguez", Cargo = "Soporte" },
            new Trabajador { Id = 6, Codigo = "T005", Nombre = "Sofía Fernández", Cargo = "Recursos Humanos" }
        );
    }
}

