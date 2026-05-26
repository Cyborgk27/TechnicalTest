# Sistema de Gestión y Distribución Automatizada de Ítems de Trabajo

Este repositorio contiene una solución backend basada en microservicios desarrollada con .NET 8 y C#. Su objetivo es gestionar ítems de trabajo y distribuirlos automáticamente entre usuarios siguiendo reglas de negocio que equilibran carga, relevancia y urgencia.

**Contenido**
- **Microservicio Users.API:** gestión de usuarios.
- **Microservicio WorkItems.API:** núcleo del negocio, algoritmo de distribución y API pública para crear y asignar ítems.

**Tecnologías principales:** .NET 8, C#, SQL Server (LocalDB), Bogus (seed data), IHttpClientFactory.

---

**Arquitectura (resumen)**

- Servicios independientes con bases de datos separadas (SQL Server).
- Comunicación vía HTTP/REST entre microservicios.
- `WorkItemApplicationService` implementa las reglas de negocio; los controladores son delgados (Thin Controllers).
- `Users.API` expone la lista de usuarios consumida por `WorkItems.API` mediante `IHttpClientFactory` y configuraciones en `appsettings.json`.

---

**Algoritmo de distribución — reglas clave**

El algoritmo aplica las reglas en el siguiente orden para elegir el colaborador óptimo:

1. Saturación de carga: un usuario con más de 3 ítems de relevancia Alta pendientes se considera saturado y queda excluido de nuevas asignaciones.
2. Urgencia (dueDate < 3 días): si el ítem vence en menos de 3 días, se asigna al usuario con menos ítems pendientes totales (prioridad por desahogo de carga).
3. Relevancia: los ítems de relevancia Alta se procesan antes y se asignan a los usuarios con menor carga actual.
4. Orden de pendientes: tras cada asignación, la lista de pendientes por usuario se devuelve ordenada por fecha de entrega y, secundariamente, por relevancia.

Estos criterios buscan equilibrar la carga, minimizar riesgos por vencimiento y atender primero los ítems más relevantes.

---

**Estructura del repositorio (resumen relevante)**

- Users.Api/: microservicio de usuarios (DbContext, seed con Bogus, controladores).
- WorkItems.Api/: microservicio de ítems (Application Service, controllers, DTOs, seed scenarios).

Vea los proyectos para más detalles en sus carpetas respectivas.

---

**Requisitos previos**

- .NET 8 SDK
- Visual Studio 2022 (recomendado) o `dotnet` CLI
- SQL Server LocalDB (instalado con Visual Studio)

---

**Instrucciones rápidas para ejecutar (Visual Studio)**

1. Abra la solución `TechnicalTest.sln` en Visual Studio.
2. En las propiedades de la solución, configure proyectos de inicio múltiples: marque `Users.Api` y `WorkItems.Api` como Iniciar.
3. Inicie la solución (F5). Se abrirán las interfaces Swagger de ambos servicios.

Alternativa (dotnet CLI)

1. From solution root, build:

```powershell
dotnet build
```

2. Ejecutar cada proyecto desde su carpeta:

```powershell
cd Users.Api
dotnet run

cd ..\WorkItems.Api
dotnet run
```

---

**Seed data y escenarios de prueba**

- Al iniciar, `Users.API` usa Bogus para generar usuarios de ejemplo (por ejemplo: juan.perez, maria.gomez, pedro.vaca, ana.silva).
- El proyecto incluye un escenario intencional de saturación: se crean 4 ítems de relevancia Alta para `juan.perez` para validar la regla de exclusión por saturación.

Pruebas sugeridas:

- POST `/api/WorkItems/allocate` con un ítem cualquiera: observará que `juan.perez` no recibe más asignaciones cuando está saturado.
- Crear un ítem con `dueDate` a menos de 3 días: el sistema asignará al usuario con menos carga total.

---

**Endpoints útiles**

- Users.API: endpoints de gestión de usuarios y listado (consumido por WorkItems.API).
- WorkItems.API: endpoints para crear, listar y asignar ítems (`/api/WorkItems/*`).

---
