# Descripción del Proyecto: RENDICIONES_API

## 📋 Resumen Ejecutivo

**RENDICIONES_API** es una API RESTful desarrollada en **.NET 8.0** (C#) que gestiona el proceso completo de **rendiciones de gastos y viáticos** para una organización. El sistema permite a los empleados registrar, documentar y solicitar aprobación de sus gastos, mientras que los administradores pueden revisar, aprobar o rechazar estas solicitudes de manera eficiente.

---

## 🎯 Objetivo Principal

El proyecto tiene como objetivo **automatizar y digitalizar el proceso de rendición de gastos**, eliminando la necesidad de procesos manuales en papel y proporcionando un sistema centralizado para:

- Registro de solicitudes de viáticos y gastos
- Carga y procesamiento de comprobantes de pago (facturas, boletas)
- Validación de documentos tributarios con SUNAT
- Flujo de aprobación multi-nivel
- Notificaciones automáticas
- Generación de reportes y estadísticas
- Integración con OCR para extracción de datos de comprobantes

---

## 🏗️ Arquitectura del Sistema

### Arquitectura de Capas (3-Tier Architecture)

El proyecto sigue una **arquitectura de capas** bien definida, separando responsabilidades:

```
RENDICIONES_API/
│
├── ContabilidadAPI/                     # 🎮 CAPA DE PRESENTACIÓN
│   ├── Controllers/                     # Endpoints REST API (14 controladores)
│   ├── Filters/                         # Filtros y middleware
│   ├── Models/                          # DTOs de entrada/salida
│   ├── Views/                           # Vistas (si las hay)
│   └── Program.cs                       # Configuración y arranque
│
├── CapaNegocio.ContabilidadAPI/         # 💼 CAPA DE NEGOCIO
│   ├── Repository/                      # Servicios de lógica de negocio (33 servicios)
│   │   ├── Implementation/              # Implementaciones de servicios
│   │   └── Interfaces/                  # Contratos de servicios
│   ├── Models/                          # Modelos de dominio y DTOs
│   ├── Extensions/                      # Extensiones y utilidades
│   └── OCR_README.md                    # Documentación de OCR
│
├── CapaDatos.ContabilidadAPI/           # 🗄️ CAPA DE ACCESO A DATOS
│   ├── DAO/                             # Data Access Objects (22 DAOs)
│   │   ├── Implementation/              # Implementaciones de repositorios
│   │   └── Interfaces/                  # Contratos de repositorios
│   ├── Models/                          # Entidades de base de datos
│   └── SvrendicionesContext.cs          # DbContext de Entity Framework
│
├── ContabilidadAPI.Tests/               # 🧪 PRUEBAS UNITARIAS (API)
│   └── Controllers/                     # Tests de controladores
│
└── CapaNegocio.ContabilidadAPI.Tests/   # 🧪 PRUEBAS UNITARIAS (Servicios)
    └── Services/                        # Tests de servicios (27 pruebas)
```

### Capas del Sistema

#### 1. **Capa de Presentación (ContabilidadAPI)**
- **14 Controladores REST** que exponen endpoints HTTP
- Autenticación JWT para seguridad
- Documentación con Swagger/OpenAPI
- Manejo de errores y validaciones
- Integración con Hangfire para tareas en segundo plano

#### 2. **Capa de Negocio (CapaNegocio.ContabilidadAPI)**
- **33 Servicios** que implementan la lógica de negocio
- Validaciones de reglas de negocio
- Transformación de datos entre capas
- Servicios especializados (OCR, SUNAT, Notificaciones)
- Uso de AutoMapper para mapeo de objetos

#### 3. **Capa de Datos (CapaDatos.ContabilidadAPI)**
- **22 DAOs** para acceso a base de datos
- Entity Framework Core como ORM
- SQL Server como base de datos
- Modelos de entidades que representan tablas

---

## 🔑 Funcionalidades Principales

### 1. **Gestión de Viáticos (Sviáticos)**

El sistema permite gestionar solicitudes de viáticos para empleados:

**Entidades principales:**
- `SviaticosCabecera`: Encabezado de la solicitud de viático
- `SviaticosDetalle`: Detalles/líneas de cada gasto en el viático

**Funcionalidades:**
- ✅ Crear nuevas solicitudes de viático
- ✅ Agregar múltiples detalles/líneas de gasto
- ✅ Filtrar por usuario, estado, fechas
- ✅ Cambiar estados de la solicitud (Pendiente → En Revisión → Aprobado/Rechazado)
- ✅ Marcar detalles como observados
- ✅ Dashboard con estadísticas por usuario
- ✅ Generación de números correlativos automáticos

**Estados del flujo:**
- Pendiente
- En Revisión
- Aprobado
- Rechazado
- Observado

### 2. **Gestión de Comprobantes de Pago**

Sistema completo para registrar y validar comprobantes tributarios:

**Entidades principales:**
- `ComprobantePago`: Representa facturas, boletas y otros comprobantes

**Funcionalidades:**
- ✅ Registro de comprobantes (factura, boleta, recibo)
- ✅ Validación de duplicados (serie + correlativo)
- ✅ Validación con SUNAT en tiempo real
- ✅ Extracción de datos mediante OCR
- ✅ Adjuntar archivos (PDF, imágenes)
- ✅ Marcar comprobantes como observados
- ✅ Búsqueda y filtrado avanzado
- ✅ Estadísticas de comprobantes
- ✅ Asociación con detalles de viáticos

**Tipos de comprobante:**
- Factura
- Boleta de Venta
- Recibo por Honorarios
- Nota de Crédito
- Nota de Débito

### 3. **Procesamiento OCR (Reconocimiento Óptico de Caracteres)**

**Tecnologías:**
- **Tesseract OCR**: Para extracción local de texto
- **Azure Document Intelligence**: Para procesamiento avanzado en la nube

**Funcionalidades:**
- ✅ Extracción automática de datos de comprobantes
- ✅ Reconocimiento de:
  - RUC del emisor
  - Serie y correlativo
  - Fecha de emisión
  - Montos (subtotal, IGV, total)
  - Tipo de comprobante
- ✅ Soporte para imágenes (JPG, PNG) y PDFs
- ✅ Preprocesamiento de imágenes para mejor precisión
- ✅ Procesamiento en español

**Configuración:**
- Archivos de entrenamiento en carpeta `/tessdata`
- Configuración híbrida (desarrollo local, producción en Azure)

### 4. **Validación con SUNAT**

Integración con los servicios de SUNAT para validar comprobantes:

**Funcionalidades:**
- ✅ Validación en tiempo real de comprobantes
- ✅ Verificación de existencia en registros de SUNAT
- ✅ Validación de RUC del emisor
- ✅ Consulta de estado del comprobante

### 5. **Sistema de Notificaciones**

**Funcionalidades:**
- ✅ Notificaciones automáticas en eventos clave
- ✅ Notificación cuando una solicitud cambia de estado
- ✅ Notificación de comprobantes observados
- ✅ Notificación de aprobaciones/rechazos
- ✅ Historial de notificaciones por usuario

**Integración:**
- Hangfire para programación de notificaciones
- Posible integración con correo electrónico

### 6. **Gestión de Tipos de Gasto**

**Funcionalidades:**
- ✅ Catálogo de tipos de gasto permitidos
- ✅ Políticas por tipo de persona (ejecutivo, empleado, etc.)
- ✅ Montos máximos permitidos por tipo
- ✅ Activación/desactivación de tipos de gasto

### 7. **Gestión de Usuarios y Autenticación**

**Funcionalidades:**
- ✅ Autenticación JWT (JSON Web Tokens)
- ✅ Roles y permisos
- ✅ Gestión de usuarios por tipo de persona
- ✅ Empleados con DNI/documento

### 8. **Reportes y Estadísticas**

**Funcionalidades:**
- ✅ Dashboard de estadísticas por usuario
- ✅ Estadísticas de comprobantes
- ✅ Conteo de viáticos por estado
- ✅ Filtros avanzados para reportes

---

## 🛠️ Tecnologías Utilizadas

### Backend Framework
- **.NET 8.0** (C#)
- **ASP.NET Core Web API**
- **Entity Framework Core 9.0.2** (ORM)

### Base de Datos
- **Microsoft SQL Server**
- **Entity Framework Core** con migraciones

### Autenticación y Seguridad
- **JWT Bearer Authentication** (Microsoft.AspNetCore.Authentication.JwtBearer 8.0.13)
- **User Secrets** para desarrollo local
- **Variables de entorno** para producción

### OCR y Procesamiento de Documentos
- **Tesseract OCR** (procesamiento local)
- **Azure Document Intelligence** (servicio en la nube)
- Archivos de entrenamiento en español

### Tareas en Segundo Plano
- **Hangfire 1.8.22** 
  - Procesamiento asíncrono
  - Tareas programadas
  - Dashboard de monitoreo

### Logging
- **Serilog 8.0.0**
  - Logs en archivo
  - Múltiples niveles de log
  - Configuración flexible

### Documentación API
- **Swagger/OpenAPI** (Swashbuckle.AspNetCore 6.6.2)
  - Interfaz interactiva
  - Especificación OpenAPI

### Mapeo de Objetos
- **AutoMapper**
  - Transformación DTO ↔ Entidades
  - Configuración de perfiles

### Testing
- **xUnit 2.9.2** (framework de pruebas)
- **Moq 4.20.72** (mocking)
- **FluentAssertions 8.8.0** (aserciones expresivas)
- **Microsoft.EntityFrameworkCore.InMemory 10.0.1** (BD en memoria)
- **27 pruebas unitarias** implementadas para servicios

### Otros
- **Newtonsoft.Json** (serialización JSON)
- **CORS** habilitado para cross-origin requests

---

## 📊 Modelos de Datos Principales

### 1. **SviaticosCabecera** (Encabezado de Viático)
```csharp
- Id (PK)
- NumeroViático
- FechaSolicitud
- UsuarioId
- EstadoId
- Descripción
- MontoTotal
- FechaCreación
- FechaModificación
```

### 2. **SviaticosDetalle** (Detalle de Gasto)
```csharp
- Id (PK)
- SvIdCabecera (FK)
- TipoGastoId (FK)
- Fecha
- Descripción
- Monto
- Observado (bool)
- Aprobado (bool)
- FechaCreación
```

### 3. **ComprobantePago** (Comprobante)
```csharp
- Id (PK)
- SvIdDetalle (FK)
- TipoComprobante
- Serie
- Correlativo
- RucEmisor
- RazonSocialEmisor
- FechaEmisión
- Subtotal
- IGV
- Total
- Observado (bool)
- ArchivoUrl
- FechaCreación
```

### 4. **Notificacion**
```csharp
- Id (PK)
- UsuarioId (FK)
- Mensaje
- Tipo
- Leída (bool)
- FechaCreación
```

### 5. **TipoGasto**
```csharp
- Id (PK)
- Nombre
- Descripción
- MontoMaximo
- Activo (bool)
```

---

## 🔌 API Endpoints Principales

### Viáticos (SviaticoController)
```
GET    /api/Sviatico                           # Listar todos los viáticos
GET    /api/Sviatico/{id}                      # Obtener viático por ID
GET    /api/Sviatico/codeUsuario/{documento}   # Viáticos por DNI
POST   /api/Sviatico/cabecera                  # Crear nueva solicitud
POST   /api/Sviatico/detalle                   # Agregar detalle de gasto
PUT    /api/Sviatico/detalle/{id}/observado    # Marcar como observado
PUT    /api/Sviatico/estado/{id}               # Cambiar estado
DELETE /api/Sviatico/{id}                      # Eliminar viático
GET    /api/Sviatico/dashboard/{usuarioId}     # Dashboard de usuario
```

### Comprobantes de Pago (ComprobantePagoController)
```
GET    /api/ComprobantePago                    # Listar comprobantes (paginado)
GET    /api/ComprobantePago/{id}               # Obtener por ID
POST   /api/ComprobantePago                    # Crear comprobante
PUT    /api/ComprobantePago/{id}/observado     # Marcar como observado
DELETE /api/ComprobantePago/{id}               # Eliminar comprobante
GET    /api/ComprobantePago/detalle/{id}       # Por detalle de viático
POST   /api/ComprobantePago/buscar             # Búsqueda avanzada
GET    /api/ComprobantePago/estadisticas       # Estadísticas
```

### OCR (OcrController)
```
POST   /api/Ocr/extract-text                   # Extraer texto de imagen/PDF
POST   /api/Ocr/process-comprobante            # Procesar comprobante completo
```

### SUNAT (SunatController)
```
POST   /api/Sunat/validar-comprobante          # Validar con SUNAT
GET    /api/Sunat/validar-ruc/{ruc}            # Validar RUC
```

### Notificaciones (NotificacionController)
```
GET    /api/Notificacion/usuario/{id}          # Notificaciones de usuario
PUT    /api/Notificacion/{id}/leer             # Marcar como leída
POST   /api/Notificacion                       # Crear notificación
```

### Autenticación (AuthController / TokenController)
```
POST   /api/Auth/login                         # Iniciar sesión
POST   /api/Token/refresh                      # Refrescar token
```

---

## 🔒 Seguridad

### Gestión de Secretos

**Desarrollo Local:**
- **User Secrets** (.NET User Secrets)
- Claves de Azure almacenadas fuera del repositorio
- Configuración en `secrets.json` local

**Producción (IIS/Azure):**
- **Variables de entorno** en `web.config`
- **Azure Key Vault** (recomendado para Azure App Service)
- Regeneración de claves comprometidas

### Autenticación
- **JWT (JSON Web Tokens)**
- Tokens firmados con clave secreta
- Expiración configurable
- Atributo `[Authorize]` en controladores

### Protección de Datos Sensibles
- ❌ No se commitean claves en Git
- ✅ Archivos de configuración con placeholders
- ✅ `.gitignore` configurado correctamente
- ✅ GitHub Secret Scanning habilitado

---

## 📦 Despliegue

### Entornos Soportados
1. **IIS (Internet Information Services)** - Windows Server
2. **Azure App Service** - Plataforma en la nube
3. **Docker** (potencial)

### Archivos de Configuración
- `appsettings.json` - Configuración base
- `appsettings.Development.json` - Desarrollo
- `appsettings.Production.json` - Producción
- `web.config` - Configuración IIS

### Requisitos del Sistema
- .NET 8.0 Runtime
- SQL Server (local o Azure SQL)
- Acceso a Azure Document Intelligence (opcional)
- Tesseract OCR instalado (para OCR local)

---

## 🧪 Pruebas Unitarias

### Cobertura Actual
- ✅ **27 pruebas** para servicios de negocio
- ✅ **19 pruebas** para controladores
- ✅ Patrón AAA (Arrange-Act-Assert)
- ✅ Mocking con Moq
- ✅ Aserciones con FluentAssertions

### Proyectos de Pruebas
1. **CapaNegocio.ContabilidadAPI.Tests**
   - ComprobantePagoServiceTests (12 pruebas)
   - SviaticoServiceTests (15 pruebas)

2. **ContabilidadAPI.Tests**
   - ComprobantePagoControllerTests (10 pruebas)
   - SviaticoControllerTests (9 pruebas)

### Ejecución de Pruebas
```bash
# Todas las pruebas
dotnet test

# Solo servicios
cd CapaNegocio.ContabilidadAPI.Tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📁 Estructura de Archivos Completa

```
RENDICIONES_API/
│
├── .github/                           # Configuración de GitHub Actions
│   └── workflows/                     # Pipelines CI/CD
│
├── ContabilidadAPI/                   # 🎮 API Principal
│   ├── Controllers/                   # 14 controladores REST
│   ├── Filters/                       # Filtros de acción
│   ├── Models/                        # DTOs de API
│   ├── Properties/                    # Configuración de lanzamiento
│   ├── Views/                         # Vistas (si aplica)
│   ├── Program.cs                     # Punto de entrada
│   ├── appsettings.json               # Configuración
│   ├── web.config                     # Config IIS
│   └── ContabilidadAPI.csproj         # Proyecto .NET
│
├── CapaNegocio.ContabilidadAPI/       # 💼 Lógica de Negocio
│   ├── Extensions/                    # Extensiones de servicios
│   ├── Models/                        # Modelos de dominio
│   │   ├── DTO/                       # Data Transfer Objects
│   │   └── ApiResponse.cs             # Respuesta estándar
│   ├── Repository/                    # 33 servicios
│   │   ├── Implementation/            # Implementaciones
│   │   └── Interfaces/                # Contratos
│   └── OCR_README.md                  # Documentación OCR
│
├── CapaDatos.ContabilidadAPI/         # 🗄️ Acceso a Datos
│   ├── DAO/                           # 22 repositorios
│   │   ├── Implementation/            # Implementaciones
│   │   └── Interfaces/                # Contratos
│   ├── Models/                        # Entidades EF Core
│   │   ├── Access/                    # Entidades de acceso
│   │   └── General/                   # Entidades generales
│   └── SvrendicionesContext.cs        # DbContext principal
│
├── ContabilidadAPI.Tests/             # 🧪 Tests de API
│   └── Controllers/                   # Tests de controladores
│
├── CapaNegocio.ContabilidadAPI.Tests/ # 🧪 Tests de Servicios
│   └── Services/                      # Tests de lógica de negocio
│
├── tessdata/                          # 📄 Datos de Tesseract OCR
│   └── spa.traineddata                # Modelo español
│
├── AZURE_CONFIG_SETUP.md              # 📖 Configuración Azure
├── AZURE_DOCUMENT_INTELLIGENCE_README.md
├── DEPLOYMENT_SUMMARY.md              # 📖 Resumen de despliegue
├── GITHUB_VARIABLES_CONFIG.md         # 📖 Variables de GitHub
├── README_TESTS.md                    # 📖 Guía de pruebas
├── TESTING_SUMMARY.md                 # 📖 Resumen de testing
└── description_proyect.md             # 📖 Este archivo
```

---

## 🔄 Flujo de Trabajo Típico

### Solicitud de Viático por un Empleado

1. **Empleado crea solicitud**
   ```
   POST /api/Sviatico/cabecera
   {
     "usuarioId": 123,
     "descripcion": "Viaje a Lima",
     "fechaSolicitud": "2024-01-15"
   }
   ```

2. **Empleado agrega gastos**
   ```
   POST /api/Sviatico/detalle
   {
     "svIdCabecera": 1,
     "tipoGastoId": 5,
     "monto": 150.00,
     "descripcion": "Transporte taxi"
   }
   ```

3. **Empleado sube comprobante**
   ```
   POST /api/ComprobantePago
   {
     "svIdDetalle": 1,
     "tipoComprobante": "Boleta",
     "archivo": [binary]
   }
   ```

4. **Sistema procesa con OCR**
   ```
   POST /api/Ocr/process-comprobante
   → Extrae automáticamente: RUC, serie, monto, etc.
   ```

5. **Sistema valida con SUNAT**
   ```
   POST /api/Sunat/validar-comprobante
   → Verifica existencia y validez
   ```

6. **Supervisor revisa y aprueba**
   ```
   PUT /api/Sviatico/estado/1
   {
     "nuevoEstado": "Aprobado"
   }
   ```

7. **Sistema envía notificación**
   ```
   → Notificación automática al empleado
   ```

---

## 🎨 Patrones de Diseño Utilizados

### 1. **Repository Pattern**
- Abstracción de acceso a datos
- Interfaces en `CapaDatos.ContabilidadAPI/DAO/Interfaces`
- Implementaciones en `Implementation`

### 2. **Service Layer Pattern**
- Lógica de negocio en capa separada
- Servicios reutilizables
- Inyección de dependencias

### 3. **Dependency Injection (DI)**
- Constructor injection en todos los componentes
- Configuración en `Program.cs`
- Ciclo de vida de servicios configurado

### 4. **DTO Pattern**
- Data Transfer Objects para comunicación entre capas
- Separación de modelos de dominio y API
- Uso de AutoMapper para transformaciones

### 5. **API Response Pattern**
- Respuesta estándar `ApiResponse<T>`
- Estructura consistente en todos los endpoints
- Manejo uniforme de errores

---

## 🚀 Ventajas del Sistema

### Para Empleados
- ✅ Registro rápido de gastos desde cualquier dispositivo
- ✅ Carga de comprobantes por foto
- ✅ Seguimiento en tiempo real del estado
- ✅ Notificaciones automáticas
- ✅ Dashboard personal de viáticos

### Para Supervisores/Administradores
- ✅ Revisión centralizada de solicitudes
- ✅ Validación automática con SUNAT
- ✅ Detección de duplicados
- ✅ Reportes y estadísticas
- ✅ Flujo de aprobación configurable

### Para la Organización
- ✅ Eliminación de papel
- ✅ Reducción de errores humanos
- ✅ Auditoría completa (logs)
- ✅ Cumplimiento normativo (SUNAT)
- ✅ Métricas de gastos en tiempo real
- ✅ Integración con sistemas contables

---

## 🔮 Posibles Mejoras Futuras

### Funcionalidades
- [ ] Integración con sistemas de aprobación (Active Directory)
- [ ] Exportación a Excel/PDF de reportes
- [ ] Integración con sistema contable (ERP)
- [ ] Aplicación móvil nativa
- [ ] Flujos de aprobación personalizables
- [ ] Alertas por correo electrónico
- [ ] Reconocimiento facial para validación
- [ ] Integración con bancos para validación de pagos

### Técnicas
- [ ] Migración a .NET 9.0
- [ ] Implementación de CQRS
- [ ] Event Sourcing para auditoría
- [ ] Redis para caché
- [ ] GraphQL como alternativa a REST
- [ ] Contenedores Docker
- [ ] Kubernetes para orquestación
- [ ] CI/CD completo con GitHub Actions

---

## 📞 Información Adicional

### Documentación Relacionada

- **AZURE_CONFIG_SETUP.md**: Configuración de Azure Document Intelligence y User Secrets
- **README_TESTS.md**: Guía completa de pruebas unitarias
- **TESTING_SUMMARY.md**: Resumen ejecutivo de testing
- **DEPLOYMENT_SUMMARY.md**: Guía de despliegue
- **GITHUB_VARIABLES_CONFIG.md**: Configuración de variables de entorno
- **OCR_README.md**: Configuración del servicio OCR

### Tecnologías Clave

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 9.0.2 | ORM |
| SQL Server | - | Base de datos |
| JWT | 8.0.13 | Autenticación |
| Hangfire | 1.8.22 | Tareas en segundo plano |
| Serilog | 8.0.0 | Logging |
| xUnit | 2.9.2 | Testing |
| Swagger | 6.6.2 | Documentación API |

---

## 📝 Notas Finales

Este proyecto es un **sistema empresarial completo** de rendición de gastos que:

1. ✅ Sigue **mejores prácticas** de arquitectura .NET
2. ✅ Implementa **arquitectura de capas** bien definida
3. ✅ Utiliza **tecnologías modernas** (.NET 8, EF Core 9)
4. ✅ Incluye **pruebas unitarias** comprehensivas
5. ✅ Tiene **seguridad** integrada (JWT, User Secrets)
6. ✅ Ofrece **integración** con servicios externos (SUNAT, Azure)
7. ✅ Proporciona **documentación** extensa
8. ✅ Está **listo para producción**

El sistema está diseñado para ser **escalable**, **mantenible** y **extensible**, permitiendo agregar nuevas funcionalidades sin afectar el código existente.

---

**Fecha de última actualización:** 2024  
**Equipo de Desarrollo:** Movitec Development Team  
**Mantenedor:** garcia4014
