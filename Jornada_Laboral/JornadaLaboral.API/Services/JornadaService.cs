// Contiene toda la lógica del negocio: reglas, validaciones y operaciones con la BD
using JornadaLaboral.API.Data;
using JornadaLaboral.API.DTOs;
using JornadaLaboral.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JornadaLaboral.API.Services;

// Contrato que define qué puede hacer el servicio
public interface IJornadaService
{
    Task<(bool ok, string ErrorCode, string Mensaje, JornadaResponse? Data)> IniciarJornadaAsync(string codigo);
    Task<(bool Ok, string ErrorCode, string Mensaje, JornadaResponse? Data)> TerminarJornadaAsync(string codigo);
    Task<IEnumerable<JornadaResponse>> ObtenerRegistrosAsync(string? fecha); 
}

public class JornadaService : IJornadaService
{
    //Referencia a la BD, inyectada autmaticamente por .NET
    private readonly AppDbContext _context;

    public JornadaService(AppDbContext context)
    {
        _context = context;
    }

    // INICIAR JORNADA
    public async Task<(bool ok, string ErrorCode, string Mensaje, JornadaResponse? Data)> IniciarJornadaAsync(string codigo)
    {
        // Elimina espacios y convierte a mayúsculas para evitar errores de tipeo
        codigo = codigo.Trim().ToUpper();

        // E01: El campo llegó vacío desde el frontend
        if (string.IsNullOrWhiteSpace(codigo))
            return (false, "E01", "El código de trabajador no puede estar vacío.", null);

        // Busca el trabajador en la BD por su código
        var trabajador = await _context.Trabajadores
            .FirstOrDefaultAsync(t => t.Codigo == codigo);

        // E02: El código no existe en la tabla Trabajadores
        if (trabajador is null)
            return (false, "E02", $"El código \"{codigo}\" no está registrado en el sistema. Verifique su código e intente de nuevo.", null);

        // Fecha de hoy en formato texto para comparar con los registros
        var hoy = DateTime.Today.ToString("yyyy-MM-dd");

        // Busca si ya tiene una jornada iniciada hoy sin terminar
        var jornadaActiva = await _context.Jornadas
            .FirstOrDefaultAsync(j => j.TrabajadorId == trabajador.Id && j.Fecha == hoy && j.HoraSalida == null);

        // E03: Ya tiene jornada activa — no puede iniciar otra sin cerrar la anterior
        if (jornadaActiva is not null)
            return (false, "E03", $"El trabajador {trabajador.Nombre} ya tiene una jornada iniciada hoy sin cerrar. Termine la jornada anterior antes de iniciar una nueva.", null);

        // Busca si ya completó su jornada hoy (entró y salió)
        var jornadaCompletada = await _context.Jornadas
            .FirstOrDefaultAsync(j => j.TrabajadorId == trabajador.Id && j.Fecha == hoy && j.HoraSalida != null);

        // E04: Ya terminó su jornada hoy — no puede registrar otra
        if (jornadaCompletada is not null)
            return (false, "E04", $"El trabajador {trabajador.Nombre} ya registró y completó una jornada hoy. Contacte a RRHH si necesita corregir el registro.", null);

        // Todo bien: crea el registro de jornada con la hora actual
        var nuevaJornada = new Jornada
        {
            TrabajadorId = trabajador.Id,
            Fecha        = hoy,
            HoraEntrada  = DateTime.Now
        };

        // Agrega el registro y guarda en la BD
        _context.Jornadas.Add(nuevaJornada);
        await _context.SaveChangesAsync();

        return (true, string.Empty, $"Jornada iniciada correctamente para {trabajador.Nombre}.", MapToResponse(nuevaJornada, trabajador));
    }

    // TERMINAR JORNADA
    public async Task<(bool Ok, string ErrorCode, string Mensaje, JornadaResponse? Data)> TerminarJornadaAsync(string codigo)
    {
        // Elimina espacios y convierte a mayúsculas para evitar errores de tipeo
        codigo = codigo.Trim().ToUpper();

        // E01: El campo llegó vacío desde el frontend
        if (string.IsNullOrWhiteSpace(codigo))
            return (false, "E01", "El código de trabajador no puede estar vacío.", null);

        var trabajador = await _context.Trabajadores
            .FirstOrDefaultAsync(t => t.Codigo == codigo);

        // E02: El código no existe en la tabla Trabajadores
        if (trabajador is null)
            return (false, "E02", $"El código \"{codigo}\" no está registrado en el sistema.", null);

        var hoy = DateTime.Today.ToString("yyyy-MM-dd");

        // Busca la jornada activa del trabajador (sin hora de salida)
        var jornada = await _context.Jornadas
            .FirstOrDefaultAsync(j => j.TrabajadorId == trabajador.Id && j.Fecha == hoy && j.HoraSalida == null);

        // E05: No hay jornada activa que cerrar
        if (jornada is null)
            return (false, "E05", "No se encontró una jornada activa para cerrar. Es posible que ya haya sido terminada o nunca fue iniciada.", null);

        // Registra la hora de salida
        jornada.HoraSalida = DateTime.Now;

        // Calcula cuánto tiempo trabajó (salida - entrada)
        var duracion = jornada.HoraSalida.Value - jornada.HoraEntrada;

        // Guarda el tiempo en formato HH:mm:ss
        jornada.TiempoTotal = $"{(int)duracion.TotalHours:D2}:{duracion.Minutes:D2}:{duracion.Seconds:D2}";

        // Guarda los cambios en la BD
        await _context.SaveChangesAsync();

        return (true, string.Empty, $"Jornada terminada correctamente. Tiempo laborado: {jornada.TiempoTotal}.", MapToResponse(jornada, trabajador));
    }

    // OBTENER REGISTROS
    public async Task<IEnumerable<JornadaResponse>> ObtenerRegistrosAsync(string? fecha)
    {
        // Si no llega fecha, usa hoy por defecto
        var filtro = fecha ?? DateTime.Today.ToString("yyyy-MM-dd");

        // Trae todas las jornadas del dia ordenadas de mas reciente a mas antigua
        return await _context.Jornadas
            .Include(j => j.Trabajador)           // Incluye los datos del trabajador en la consulta
            .Where(j => j.Fecha == filtro)         // Solo las del día solicitado
            .OrderByDescending(j => j.HoraEntrada) // Las más recientes primero
            .Select(j => MapToResponse(j, j.Trabajador))
            .ToListAsync();  
    }

    // MAPPER: convierte los modelos de BD al formato de respuesta
    private static JornadaResponse MapToResponse(Jornada j, Trabajador t) => new(
        j.Id,
        t.Codigo,
        t.Nombre,
        t.Cargo,
        j.Fecha,
        j.HoraEntrada,
        j.HoraSalida,
        j.TiempoTotal
    );
}