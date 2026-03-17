// Representa la tabla "Jornadas" en la base de datos
namespace JornadaLaboral.API.Models;

public class Jornada
{
    // Identificador unico generado automaticamente por la BD
    public int Id { get; set; }
    // Llave foránea: indica a qué trabajador pertenece esta jornada
    public int TrabajadorId { get; set; }
     // Fecha y hora exacta en que el trabajador inició su jornada
    public DateTime HoraEntrada { get; set; }
    // Fecha y hora de salida — el "?" significa que puede estar vacío (jornada activa)
    public DateTime? HoraSalida { get; set; }
    // Tiempo total laborado en formato HH:mm:ss — vacío hasta que termine la jornada
    public string? TiempoTotal { get; set; }
    // Fecha en formato texto (yyyy-MM-dd) para filtrar jornadas por día
    public string Fecha { get; set; } = string.Empty;
    // Permite acceder a los datos del trabajador desde una jornada
    public Trabajador Trabajador { get; set; } = null!;
}