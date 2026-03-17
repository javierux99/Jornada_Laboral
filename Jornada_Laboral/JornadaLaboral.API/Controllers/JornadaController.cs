// Recibe las peticiones HTTP del frontend y las delega al Service
using JornadaLaboral.API.DTOs;
using JornadaLaboral.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JornadaLaboral.API.Controllers;

[ApiController]
[Route("api/[controller]")]   // Define la ruta base: /api/jornada
[Produces("application/json")]
public class JornadaController : ControllerBase
{
    // El Service se inyecta automáticamente — el Controller no crea objetos directamente
    private readonly IJornadaService _jornadaService;

    public JornadaController(IJornadaService jornadaService)
    {
        _jornadaService = jornadaService;
    }

    // POST/api/jornada/iniciar
    /// <summary>Inicia la jornada laboral de un trabajador.</summary>
    [HttpPost("iniciar")]
    [ProducesResponseType(typeof(ApiResponse<JornadaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<JornadaResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<JornadaResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<JornadaResponse>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IniciarJornada([FromBody] IniciarJornadaRequest request)
    {
        // Validación rápida antes de llamar al Service
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiResponse<JornadaResponse>(false, "El código no puede estar vacío.", null, "E01"));

        // Llama al Service con el código recibido
        var (ok, errorCode, mensaje, data) = await _jornadaService.IniciarJornadaAsync(request.Codigo);

        // Si hubo error, devuelve el código HTTP apropiado según el tipo de error
        if (!ok)
        {
            return errorCode switch
            {
                "E02"           => NotFound(new ApiResponse<JornadaResponse>(false, mensaje, null, errorCode)),   // 404: no existe
                "E03" or "E04"  => Conflict(new ApiResponse<JornadaResponse>(false, mensaje, null, errorCode)),   // 409: conflicto
                _               => BadRequest(new ApiResponse<JornadaResponse>(false, mensaje, null, errorCode))  // 400: error genérico
            };
        }

        // 201 Created: jornada iniciada correctamente
        return CreatedAtAction(nameof(ObtenerRegistros), new ApiResponse<JornadaResponse>(true, mensaje, data, null));
    }

    // POST/api/jornada/terminar
    /// <summary>Termina la jornada laboral de un trabajador.</summary>
    [HttpPost("terminar")]
    [ProducesResponseType(typeof(ApiResponse<JornadaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<JornadaResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminarJornada([FromBody] TerminarJornadaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest(new ApiResponse<JornadaResponse>(false, "El código no puede estar vacío.", null, "E01"));

        var (ok, errorCode, mensaje, data) = await _jornadaService.TerminarJornadaAsync(request.Codigo);

        if (!ok)
        {
            return errorCode switch
            {
                "E02" or "E05"  => NotFound(new ApiResponse<JornadaResponse>(false, mensaje, null, errorCode)),
                _               => BadRequest(new ApiResponse<JornadaResponse>(false, mensaje, null, errorCode))
            };
        }

        // 200 OK: jornada terminada correctamente
        return Ok(new ApiResponse<JornadaResponse>(true, mensaje, data, null));
    }

    // GET/api/jornada/registros?fecha=yyyy-MM-dd
    /// <summary>Devuelve todos los registros del dia. Si no se envia fecha, retorna los de hoy.</summary>
    [HttpGet("registros")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<JornadaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRegistros([FromQuery] string? fecha = null)
    {
        var registros = await _jornadaService.ObtenerRegistrosAsync(fecha);
        return Ok(new ApiResponse<IEnumerable<JornadaResponse>>(true, "Registros obtenidos.", registros, null));
    }
}

