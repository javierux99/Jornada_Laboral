# 🕐 Control de Jornada Laboral

Sistema completo de registro de ingreso y egreso en jornada laboral.  
Desarrollado como prueba técnica con stack C# / .NET 8.

---

## Stack Tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Backend | ASP.NET Core Web API | .NET 8.0 |
| ORM | Entity Framework Core | 8.0.0 |
| Base de datos | SQLite (vía EF Core) | 8.0.0 |
| Documentación API | Swagger / OpenAPI | 6.5.0 |
| Frontend | HTML5 + CSS3 + JS vanilla | — |
| Fuente | IBM Plex Mono / Sans | Google Fonts |

---

## Arquitectura del Proyecto

```
JornadaLaboral/
├── JornadaLaboral.sln
└── JornadaLaboral.API/
    ├── Controllers/
    │   └── JornadaController.cs     # Endpoints REST
    ├── Data/
    │   └── AppDbContext.cs          # EF Core + seed data
    ├── DTOs/
    │   └── JornadaDTOs.cs           # Request / Response models
    ├── Middleware/
    │   └── GlobalExceptionMiddleware.cs
    ├── Models/
    │   ├── Trabajador.cs
    │   └── Jornada.cs
    ├── Services/
    │   └── JornadaService.cs        # Lógica de negocio
    ├── wwwroot/
    │   └── index.html               # Frontend estático
    ├── Program.cs
    └── appsettings.json
```

---

## Cómo ejecutar

### Prerrequisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/javierux99/Jornada_Laboral.git
cd Jornada_Laboral

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la API
cd JornadaLaboral.API
dotnet run

# La app estará disponible en:
# http://localhost:5277         → Frontend
# http://localhost:5277/swagger → Documentación Swagger
```

La base de datos SQLite (`jornada_laboral.db`) se crea automáticamente con datos semilla.

---

## Endpoints API

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/jornada/iniciar` | Inicia la jornada de un trabajador |
| `POST` | `/api/jornada/terminar` | Termina la jornada activa |
| `GET`  | `/api/jornada/registros?fecha=yyyy-MM-dd` | Lista registros del día |

### Ejemplo — Iniciar jornada
```json
POST /api/jornada/iniciar
{ "codigo": "T001" }

// Respuesta exitosa (201)
{
  "success": true,
  "message": "Jornada iniciada correctamente para Ana García.",
  "data": {
    "id": 1,
    "codigoTrabajador": "T001",
    "nombreTrabajador": "María Gómez",
    "cargo": "Diseñadora",
    "fecha": "2025-01-15",
    "horaEntrada": "2025-01-15T08:00:00",
    "horaSalida": null,
    "tiempoTotal": null
  },
  "errorCode": null
}
```

---

## Escenarios de Error Manejados

| Código | Escenario | Justificación | HTTP |
|--------|-----------|---------------|------|
| **E01** | Código vacío | El trabajador envía el formulario sin ingresar código | 400 |
| **E02** | Código no registrado | Código inexistente o error tipográfico | 404 |
| **E03** | Jornada activa sin cerrar | El trabajador recargó o abrió otra sesión sin cerrar la anterior | 409 |
| **E04** | Jornada ya completada hoy | Evita doble registro en el mismo día | 409 |
| **E05** | No existe jornada activa para cerrar | Petición maliciosa o desincronización cliente-servidor | 404 |
| **E00** | Error interno del servidor | Capturado por el middleware global de excepciones | 500 |

---

## Trabajadores de prueba (seed)

| Código | Nombre | Cargo |
|--------|--------|-------|
| T001 | María Gómez | Diseñadora |
| T002 | Pepito Perez | Analista |
| T003 | Ana Martínez | Gerente |
| T004 | Luis Rodríguez | Soporte |
| T005 | Sofía Fernández | Recursos Humanos |
| ADMIN01 | Javier Aguilar | Desarrollador |

---

## Autor

**Javier Alexander Aguilar Ocampo**  
Ingeniero de Sistemas y Computación  
javier.aguilar@utp.edu.co | Roldanillo, Valle del Cauca
