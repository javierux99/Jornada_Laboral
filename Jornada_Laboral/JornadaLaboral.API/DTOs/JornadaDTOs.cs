// DTOs: objetos que definen qué datos entran y salen de la API
namespace JornadaLaboral.API.DTOs;

// Lo que llega del frontend al iniciar jornada
public record IniciarJornadaRequest(string Codigo);

// Lo que llega del frontend al terminar jornada
public record TerminarJornadaRequest(string Codigo);

// Lo que se envía al frontend con los datos de una jornada
public record JornadaResponse(
    int id,
    string CodigoTrabajador,
    string NombreTrabajador,
    string Cargo,
    string Fecha,
    DateTime HoraEntrada,
    DateTime? HoraSalida, // Vacío si la jornada sigue activa
    string? TiempoTotal   // Vacío hasta que el trabajador termine
    );

// ************* Envoltorio estandar para todas las respuestas de la API********************
// T es genérico: puede envolver una jornada, una lista, o cualquier dato.
public record ApiResponse<T>(
    bool Success,      // true = todo bien, false = hubo un error
    string Message,    // mensaje legible para mostrar al usuario
    T? Data,           // los datos de la respuesta (puede ser null si hay error)
    string? ErrorCode  // código del error (E01, E02...) o null si fue exitoso
);


