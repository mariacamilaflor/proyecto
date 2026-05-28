# Manual del Sistema: Usuario, Técnico e Instalación

Repositorio: EduDirectory3

Este documento reúne el Manual Técnico, el Manual de Usuario y el Manual de Instalación. Está orientado a una aplicación web .NET (Razor Pages / MVC) y debe adaptarse a detalles concretos del proyecto.

## Índice

1. Manual Técnico
2. Manual de Usuario
3. Manual de Instalación

---

# 1. Manual Técnico

Dirigido a desarrolladores. Describe cómo está construido el sistema.

## 1.1 Arquitectura del sistema

- Patrón: MVC/Razor Pages con separación en capas convencionales:
  - Presentación: Pages/Views y componentes UI (Razor Pages o Views).
  - Lógica de negocio: Controllers / Services / Business layer.
  - Persistencia: Data / Repositories / DbContext (EF Core).
  - Cross-cutting: Logging, configuración, autenticación/autorization.

## 1.2 Tecnologías usadas

- Plataforma: ASP.NET Core (.NET 10)
- UI: Razor Pages / ASP.NET Core MVC
- ORM: Entity Framework Core
- Base de datos: SQL Server (o LocalDB para desarrollo)
- APIs externas: Google Maps API (mapas y geocoding), OpenRouter (servicios de routing), otras APIs posibles (ej. ICFES, servicios de terceros)
- Autenticación/Autorización: ASP.NET Core Identity

## 1.3 Diagrama de base de datos (tablas y relaciones)

Nota: sin acceso directo al modelado real se presenta un esquema ejemplo. Ajustar según el proyecto.

- Instituciones (Institution)
  - Id (PK), Nombre, Tipo, Dirección, Ciudad, Teléfono, Web
- Sedes (Campus)
  - Id (PK), InstitutionId (FK -> Institutions), Nombre, Dirección, Coordenadas
- Usuarios (AspNetUsers por Identity)
  - Id, Email, PasswordHash, Rol
- Evaluaciones (Evaluation)
  - Id, InstitutionId (FK), UsuarioId (FK), Puntuación, Comentario, Fecha
- Rectores (Rector)
  - Id, InstitutionId (FK), Nombre, Email, PeriodoInicio, PeriodoFin
- ICFES / Indicadores (IcfesRecord)
  - Id, InstitutionId (FK), Año, PuntajePromedio, Observaciones

Relaciones principales:
- Institution 1:N Sedes
- Institution 1:N Evaluaciones
- Institution 1:N Rectores
- Institution 1:N IcfesRecord

Para generar un diagrama físico: usar herramientas como SQL Server Management Studio, EF Core Power Tools o draw.io con exportación desde el modelo.

## 1.4 Descripción de los módulos principales

- Controllers / Pages: reciben peticiones HTTP, validan entrada y devuelven vistas o JSON.
- Services: lógica de negocio reutilizable (p. ej. InstitutionService, MapService, EvaluationService).
- Repositories / Data: abstracción sobre DbContext para consultas y persistencia.
- ViewModels / DTOs: objetos que se pasan a las vistas o a la API para evitar acoplar entidades.
- Components / Partial Views: bloques reutilizables de UI (p. ej. tarjeta de institución, modal de reseña).

Estructura de ejemplo:
- WebApplication3/Controllers
- WebApplication3/Pages o Views/
- WebApplication3/Services
- WebApplication3/Data (ApplicationDbContext, Migrations)
- WebApplication3/Models (Entidades y ViewModels)

## 1.5 Diagrama de flujo del chatbot

Flujo general (texto):
1. Usuario envía mensaje desde la UI (chatbox).
2. Frontend envía petición al endpoint /api/chatbot o a Controller Chatbot.
3. Controller valida entrada y llama a ChatbotService.
4. ChatbotService preprocesa, consulta contextos (base de datos o memoria de sesión), y llama a la API externa (OpenRouter/IA o motor local).
5. Respuesta del motor procesada por ChatbotService (formateo, búsqueda de links o sugerencias).
6. Respuesta devuelta al cliente y renderizada en la UI.

Incluir manejo de errores, logging y límite de tasa para llamadas a APIs externas.

## 1.6 Seguridad y roles

- Autenticación: ASP.NET Core Identity (registro, login, recuperación de contraseña).
- Autorización: atributos [Authorize(Roles = "Admin,Institution")], políticas basadas en Claims.
- Roles típicos: Admin, InstitutionAdmin, User (ciudadano), Reviewer.
- Protección CSRF: habilitada por defecto en formularios Razor.
- Validación y sanitización de entradas: evitar inyección SQL (usar EF Core parametrizado) y XSS (HTML encode en vistas).

## 1.7 APIs externas y cómo se integran

- Google Maps API: usadas en el frontend para mostrar mapas y marcadores; backend puede usar Geocoding API para obtener coordenadas.
  - Integración: insertar script de Google Maps en la vista, usar API Key en appsettings.json o variable de entorno.
- OpenRouter / Routing APIs: llamadas desde el backend o frontend para calcular rutas y distancias.
  - Integración: configurar API Key, implementar MapService que llame a la API con HttpClient y cache de respuestas.
- Otras APIs (ICFES): llamadas a endpoints REST para obtener indicadores; implementar adaptadores en Services.

---

# 2. Manual de Usuario

Dirigido al usuario final. Muestra cómo usar la aplicación.

## 2.1 Cómo registrarse e iniciar sesión

1. Acceder a la página de registro (/Identity/Account/Register o /Account/Register según implementación).
2. Completar el formulario: nombre, email, contraseña y aceptar términos.
3. Confirmar cuenta si se usa verificación por email (seguir enlace enviado).
4. Iniciar sesión en /Identity/Account/Login con email y contraseña.

## 2.2 Cómo buscar colegios con filtros

1. Ir a la página de búsqueda (p. ej. /Institutions/Search).
2. Introducir filtros: nombre, ciudad, tipo, puntuación mínima, servicios.
3. Hacer clic en Buscar. Los resultados se muestran en lista y en el mapa.
4. Usar paginación o ordenar por relevancia, distancia o puntuación.

## 2.3 Cómo usar el mapa

- El mapa muestra marcadores para cada sede.
- Hacer clic en un marcador abre un popup con información básica y enlace a la ficha.
- Zoom y arrastre estándar. Usar controles de la derecha para tipo de mapa o capas.

## 2.4 Cómo comparar instituciones

1. En los resultados, marcar las instituciones que quieres comparar (checkbox o botón "Añadir a comparador").
2. Abrir la vista de comparación (p. ej. /Institutions/Compare) con las fichas seleccionadas.
3. Comparar atributos: programas, ICFES, número de sedes, puntuación.

## 2.5 Cómo usar el chatbot

1. Abrir la ventana de chat (p. ej. esquina inferior derecha).
2. Escribir pregunta o frase (ej. "Buscar colegios en Bogotá con bilingüe").
3. El chatbot devuelve sugerencias, enlaces a fichas o pasos para refinar la búsqueda.

## 2.6 Cómo una institución gestiona su perfil, sedes, rectores, ICFES

1. Iniciar sesión como InstitutionAdmin.
2. Navegar a la sección de gestión (/Institution/Profile o panel de control).
3. Editar datos de la institución (nombre, web, descripción).
4. Gestionar sedes: añadir/editar/borrar sedes y sus coordenadas.
5. Gestionar rectores: añadir histórico de rectores y periodos.
6. Subir/editar registros ICFES por año.

## 2.7 Capturas de pantalla

Indicar: incluir capturas de cada pantalla clave: login, búsqueda, mapa, ficha de institución, comparador, panel de gestión, chat. (Agregar imágenes reales en la carpeta /docs/screenshots y referenciarlas en este documento).

---

# 3. Manual de Instalación

Dirigido a quien despliega la aplicación.

## 3.1 Requisitos previos

- Visual Studio 2022/2024/2026 o VS Code
- .NET 10 SDK (dotnet --version -> 10.x)
- SQL Server (express, localdb o servidor remoto)
- Node.js/NPM si hay assets que compilar (opcional)
- Herramientas: dotnet-ef (dotnet tool install --global dotnet-ef)

## 3.2 Cómo clonar/descargar el proyecto

1. Clonar desde el remoto:
   - git clone https://github.com/mariacamilaflor/proyecto.git EduDirectory3
2. Entrar en la carpeta:
   - cd C:\Users\kmila\source\repos\EduDirectory3

## 3.3 Cómo configurar la cadena de conexión en appsettings.json

1. Abrir WebApplication3/appsettings.json (o el proyecto principal).
2. Localizar el bloque "ConnectionStrings" y ajustar DefaultConnection:

```
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EduDirectoryDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

3. Para producción, utilizar variables de entorno o Secret Manager para no almacenar credenciales en texto plano.

## 3.4 Cómo correr las migraciones

1. Restaurar paquetes:
   - dotnet restore
2. Crear migración (si aplica):
   - dotnet ef migrations add Inicial --project WebApplication3 --startup-project WebApplication3
3. Aplicar migraciones:
   - dotnet ef database update --project WebApplication3 --startup-project WebApplication3

Si usas Package Manager Console en Visual Studio:
- Update-Database -Project WebApplication3 -StartupProject WebApplication3

## 3.5 Cómo configurar las API Keys (Google Maps, OpenRouter)

1. Guardar las claves en appsettings.json (Development) o como variables de entorno:

```
"ApiKeys": {
  "GoogleMaps": "TU_GOOGLE_MAPS_KEY",
  "OpenRouter": "TU_OPENROUTER_KEY"
}
```

2. En Program.cs leer las claves por IConfiguration y pasarlas al MapService o cliente HTTP.

## 3.6 Cómo publicar o ejecutar en local

Desde CLI:
- dotnet build
- dotnet run --project WebApplication3

Para publicar:
- dotnet publish WebApplication3 -c Release -o ./publish

Despliegue a IIS o Azure: seguir la guía específica de Microsoft (configurar appsettings, connection strings en el entorno, SSL, y variables de entorno para keys).

---

Notas finales:
- Este manual es plantilla: puedo adaptarlo con contenido real (diagramas, capturas, nombres de tablas y ejemplos concretos) si me autorizas a leer archivos clave del repositorio (appsettings.json, .csproj, Models/).

