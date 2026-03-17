// Representa la tabla "Trabajadores" en la base de datos
namespace JornadaLaboral.API.Models;

public class Trabajador
{
    // Identificador unico generado automaticamente por la BD
    public int Id { get; set; }

    // Código que el trabajador escribe en la pantalla (ej: EMP-001)
    public string Codigo { get; set; }

    // Nombre completo del trabajador
    public string Nombre { get; set; }

    // Cargo o rol dentro de la empresa
    public string Cargo { get; set; }

    // Un trabajador puede tener muchas jornadas registradas
    public ICollection<Jornada> Jornadas {get; set; } = new List<Jornada>();
}