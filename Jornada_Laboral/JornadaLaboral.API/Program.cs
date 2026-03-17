// Punto de entrada de la aplicación: configura y arranca todo el sistema
using JornadaLaboral.API.Data;
using JornadaLaboral.API.Middleware;
using JornadaLaboral.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Registra la BD SQLite usando la cadena de conexión del appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=jornada_laboral.db")); // Valor por defecto si no hay appsettings

// Registra el Service para que .NET lo inytecte automáticamente donde se necesite
builder.Services.AddScoped<IJornadaService, JornadaService>();

// Habilita el uso de Controllers (JornadaController)
builder.Services.AddControllers();

// Permite que el frontend (index.html) se comunique con la API en el navegador
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Configura Swagger: interfaz visual para probar la API en el navegador
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Jornada Laboral API",
        Version     = "v1",
        Description = "API para control de ingreso y egreso en jornada laboral",
        Contact     = new OpenApiContact
        {
            Name  = "Javier Alexander Aguilar Ocampo",
            Email = "javier.aguilar@utp.edu.co"
        }
    });

    // Incluye los comentarios /// del código como documentación en Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Crea la BD y las tablas automáticamente si no existen
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // También inserta los trabajadores del seed
}

// ── Activa el middleware que captura errores inesperados
app.UseMiddleware<GlobalExceptionMiddleware>();

// Activa Swagger solo en modo desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jornada Laboral API v1");
        c.RoutePrefix = "swagger"; // Disponible en http://localhost:5277/swagger
    });
}

// Activa la política CORS registrada arriba
app.UseCors("FrontendPolicy");

app.UseAuthorization();

// Mapea las rutas de los Controllers (GET, POST, etc.)
app.MapControllers();

// Sirve el index.html desde la carpeta wwwroot al abrir http://localhost:5277
app.UseDefaultFiles();
app.UseStaticFiles();

// Arranca el servidor y queda escuchando peticiones
app.Run();