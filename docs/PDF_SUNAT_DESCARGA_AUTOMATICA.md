# Descarga Automática de PDFs desde SUNAT

## Descripción General

Sistema automático que descarga PDFs de comprobantes electrónicos directamente desde SUNAT, ejecutándose cada 5 minutos mediante un job en Hangfire.

## Características Implementadas

### 1. Nuevas Columnas en Base de Datos

**Tabla:** `COMPROBANTE_PAGO`

- **PDF_SUNAT** (BIT): Indica si el PDF fue descargado desde SUNAT
  - `0` (false): No descargado
  - `1` (true): Descargado exitosamente
  - Valor inicial: `0`

- **REINTENTOS_PDF_SUNAT** (INT): Contador de intentos de descarga
  - Valor inicial: `0`
  - Máximo permitido: `10` (después de 10 intentos, el job deja de procesar ese comprobante)

### 2. Job en Background (Hangfire)

**Nombre del Job:** `descargar-pdfs-desde-sunat`

**Frecuencia:** Cada 5 minutos (`*/5 * * * *`)

**Archivo:** `ComprobantePdfSunatBackgroundService.cs`

**Criterios de Selección de Comprobantes:**
- `PDF_SUNAT = false` (no se ha descargado)
- `FechaCarga >= últimas 24 horas` (solo comprobantes del último día)
- `RUC, Serie y Correlativo` no vacíos
- `REINTENTOS_PDF_SUNAT < 10` (máximo 10 intentos)
- `Activo = true`
- Límite: 50 comprobantes por ejecución

**Lógica de Procesamiento:**

1. Incrementa el contador `REINTENTOS_PDF_SUNAT`
2. Obtiene el token de SUNAT desde la tabla `PARAMETROS` (ID=1)
3. Construye la URL: `https://api-cpe.sunat.gob.pe/v1/contribuyente/consultacpe/comprobantes/{ruc}-{tipo}-{serie}-{correlativo}-2/01`
4. Realiza la petición con reintentos (máximo 3 intentos por llamada)
5. Decodifica el PDF desde base64 (campo `cdr` de la respuesta)
6. Guarda el PDF en la carpeta `PDF/` con el nombre: `{ruc}_{tipo}_{serie}_{correlativo}.pdf`
7. Actualiza el comprobante:
   - `Ruta = "PDF/{nombre_archivo}.pdf"`
   - `PDF_SUNAT = true`
   - `Extension = ".pdf"`

**Manejo de Errores:**
- Si falla la descarga, incrementa el contador de reintentos
- Después de 10 intentos fallidos, el comprobante deja de procesarse
- Los errores se registran en los logs de Serilog

### 3. Endpoint Público para Servir PDFs

**URL:** `GET /api/ComprobantePago/{id}/pdf`

**Autenticación:** Público (no requiere token)

**Atributo:** `[AllowAnonymous]`

**Respuestas:**

- **200 OK**: Devuelve el archivo PDF
  - Content-Type: `application/pdf`, `image/jpeg`, `image/png`, o `application/xml`
  - Headers incluyen el nombre del archivo para descarga

- **400 Bad Request**: ID inválido

- **404 Not Found**: 
  - Comprobante no existe
  - Comprobante no tiene archivo adjunto
  - Archivo no encontrado en el servidor

- **500 Internal Server Error**: Error al leer el archivo

**Ejemplo de Uso:**
```
GET https://api.example.com/api/ComprobantePago/123/pdf
```

### 4. Estructura de Carpetas

```
RENDICIONES_API/
├── PDF/                          # Nueva carpeta creada automáticamente
│   ├── 20600650859_01_F001_0005156.pdf
│   ├── 20123456789_03_B001_0001234.pdf
│   └── ...
├── CapaNegocio.ContabilidadAPI/
│   └── Repository/
│       └── Implementation/
│           └── ComprobantePdfSunatBackgroundService.cs  # Nuevo servicio
└── docs/
    └── SQL_AGREGAR_COLUMNAS_PDF_SUNAT.sql  # Script de migración
```

## Instalación y Configuración

### 1. Ejecutar Script SQL

Ejecutar el archivo `SQL_AGREGAR_COLUMNAS_PDF_SUNAT.sql` en la base de datos `SVRENDICIONES`:

```sql
-- Agrega las columnas PDF_SUNAT y REINTENTOS_PDF_SUNAT
-- Actualiza valores por defecto en registros existentes
```

### 2. Verificar Token de SUNAT

Asegurarse de que la tabla `PARAMETROS` (ID=1) contenga un token válido de SUNAT:

```sql
SELECT * FROM PARAMETROS WHERE Id = 1;
```

### 3. Configuración del Job

El job se configura automáticamente al iniciar la aplicación en `Program.cs`:

```csharp
RecurringJob.AddOrUpdate<ComprobantePdfSunatBackgroundService>(
    "descargar-pdfs-desde-sunat",
    service => service.ProcesarComprobantesParaPdfSunat(),
    "*/5 * * * *");
```

### 4. Permisos de Carpeta

La carpeta `PDF/` se crea automáticamente con permisos de escritura. En servidores de producción, verificar que el usuario de IIS/App Pool tenga permisos adecuados.

## Monitoreo y Logs

### Dashboard de Hangfire

Acceder al dashboard en: `https://{host}/hangfire`

**Jobs a monitorear:**
- `descargar-pdfs-desde-sunat` (cada 5 minutos)
- Ver historial de ejecuciones
- Ver jobs fallidos y reintentar manualmente

### Logs de Serilog

Los logs se guardan en: `Logs/log-{fecha}.txt`

**Eventos importantes:**
- `"===== INICIO: Descarga de PDFs desde SUNAT ====="`
- `"Encontrados {Cantidad} comprobantes para descargar PDF"`
- `"PDF descargado exitosamente para comprobante ID={Id}"`
- `"No se pudo descargar PDF para comprobante ID={Id}"`
- `"===== FIN: Descarga PDFs - Exitosos: {X}, Fallidos: {Y} ====="`

**Ejemplo de log exitoso:**
```
[18:05:01 INF] ===== INICIO: Descarga de PDFs desde SUNAT =====
[18:05:01 INF] Encontrados 3 comprobantes para descargar PDF
[18:05:01 INF] Descargando PDF para comprobante ID=123, RUC=20600650859, Serie=F001, Correlativo=0005156
[18:05:02 INF] PDF guardado en: C:\API\PDF\20600650859_01_F001_0005156.pdf, Tamaño: 45678 bytes
[18:05:02 INF] PDF descargado exitosamente para comprobante ID=123
[18:05:03 INF] ===== FIN: Descarga PDFs - Exitosos: 3, Fallidos: 0 =====
```

## Comportamiento del Sistema

### Flujo de Creación de Comprobante

1. **Usuario crea comprobante** (con o sin archivo adjunto)
   - `PDF_SUNAT = false`
   - `REINTENTOS_PDF_SUNAT = 0`

2. **Job se ejecuta cada 5 minutos**
   - Busca comprobantes del último día sin PDF de SUNAT
   - Incrementa contador de reintentos
   - Intenta descargar el PDF

3. **Descarga exitosa**
   - Guarda PDF en carpeta `PDF/`
   - Actualiza `Ruta` con nueva ubicación
   - Marca `PDF_SUNAT = true`
   - **NOTA:** Si el comprobante ya tenía un archivo manual, este será sobrescrito

4. **Descarga fallida**
   - Incrementa `REINTENTOS_PDF_SUNAT`
   - Si llega a 10 reintentos, el job deja de intentar
   - El comprobante queda con el archivo original (si lo tenía)

### Priorización de Fuentes

El sistema **siempre intenta** obtener el PDF de SUNAT al menos 1 vez, incluso si el usuario ya subió un archivo manualmente. Esto garantiza que se tenga la versión oficial del comprobante.

**Orden de prioridad:**
1. PDF de SUNAT (si está disponible) → Sobrescribe `Ruta`
2. Archivo manual del usuario (si SUNAT falla después de 10 intentos)

## Consultas SQL Útiles

### Ver estado de descargas de PDF

```sql
SELECT 
    Id,
    Ruc,
    Serie,
    Correlativo,
    FechaCarga,
    PDF_SUNAT,
    REINTENTOS_PDF_SUNAT,
    Ruta
FROM COMPROBANTE_PAGO
WHERE FechaCarga >= DATEADD(DAY, -1, GETDATE())
ORDER BY PDF_SUNAT, REINTENTOS_PDF_SUNAT DESC;
```

### Comprobantes pendientes de descarga

```sql
SELECT 
    COUNT(*) as Pendientes
FROM COMPROBANTE_PAGO
WHERE Activo = 1
  AND (PDF_SUNAT = 0 OR PDF_SUNAT IS NULL)
  AND FechaCarga >= DATEADD(DAY, -1, GETDATE())
  AND (REINTENTOS_PDF_SUNAT < 10 OR REINTENTOS_PDF_SUNAT IS NULL);
```

### Comprobantes con reintentos agotados

```sql
SELECT 
    Id,
    Ruc,
    Serie,
    Correlativo,
    REINTENTOS_PDF_SUNAT,
    FechaCarga
FROM COMPROBANTE_PAGO
WHERE PDF_SUNAT = 0
  AND REINTENTOS_PDF_SUNAT >= 10
ORDER BY FechaCarga DESC;
```

### Resetear reintentos para volver a intentar

```sql
-- Resetear reintentos de un comprobante específico
UPDATE COMPROBANTE_PAGO
SET REINTENTOS_PDF_SUNAT = 0
WHERE Id = 123;

-- Resetear todos los comprobantes con más de 5 reintentos
UPDATE COMPROBANTE_PAGO
SET REINTENTOS_PDF_SUNAT = 0
WHERE REINTENTOS_PDF_SUNAT >= 5;
```

## Solución de Problemas

### PDF no se descarga

**Verificar:**
1. Token de SUNAT válido en tabla `PARAMETROS`
2. RUC, Serie y Correlativo son correctos
3. El comprobante existe en SUNAT
4. No se han agotado los 10 reintentos
5. El comprobante es de las últimas 24 horas

**Solución:**
- Revisar logs de Hangfire y Serilog
- Probar manualmente la URL de SUNAT
- Verificar conectividad con SUNAT
- Resetear contador de reintentos si es necesario

### Endpoint público no devuelve el PDF

**Verificar:**
1. El comprobante tiene valor en columna `Ruta`
2. El archivo existe físicamente en la carpeta `PDF/`
3. Los permisos de lectura están correctos
4. La ruta no contiene caracteres especiales

**Solución:**
- Verificar en base de datos: `SELECT Ruta FROM COMPROBANTE_PAGO WHERE Id = X`
- Verificar existencia del archivo
- Comprobar logs del endpoint

### Job no se ejecuta

**Verificar:**
1. Hangfire está activo
2. El servicio está registrado en `Program.cs`
3. No hay excepciones en el startup
4. Dashboard de Hangfire muestra el job

**Solución:**
- Reiniciar la aplicación
- Verificar en Hangfire Dashboard
- Revisar logs de startup

## Seguridad

### Endpoint Público

El endpoint `GET /api/ComprobantePago/{id}/pdf` es **público** (`[AllowAnonymous]`), lo que significa que cualquier persona con el ID del comprobante puede descargar el PDF.

**Consideraciones de seguridad:**

1. **IDs secuenciales**: Los IDs son secuenciales, lo que facilita enumerar comprobantes
   - **Recomendación**: Implementar rate limiting
   - **Alternativa**: Usar GUIDs en lugar de IDs incrementales

2. **Sin autenticación**: No valida que el usuario tenga permisos sobre ese comprobante
   - **Recomendación futura**: Agregar token de acceso temporal en la URL
   - **Ejemplo**: `/api/ComprobantePago/{id}/pdf?token={temp_token}`

3. **Datos sensibles**: Los PDFs contienen información de RUC, montos, etc.
   - **Mitigación actual**: Solo accesible conociendo el ID exacto
   - **Mitigación adicional**: Implementar logging de accesos

### Token de SUNAT

- El token se almacena en tabla `PARAMETROS` (ID=1)
- Solo debe ser accesible por el backend
- Renovar periódicamente según políticas de SUNAT

## Mejoras Futuras

1. **Notificaciones**: Enviar notificación cuando se descarga un PDF exitosamente
2. **Webhook**: Notificar al frontend cuando el PDF esté disponible
3. **Caché**: Cachear PDFs frecuentemente accedidos
4. **CDN**: Servir PDFs a través de CDN para mejor performance
5. **Versionado**: Mantener historial de versiones del PDF
6. **Compresión**: Comprimir PDFs grandes para ahorrar espacio
7. **Cleanup**: Job para eliminar PDFs antiguos (>90 días)
8. **Estadísticas**: Dashboard de éxito/fallo de descargas

## Notas Técnicas

- El PDF viene codificado en **base64** en el campo `cdr` de la respuesta de SUNAT
- El endpoint de SUNAT para PDF es `/01` (vs `/02` para XML)
- Los reintentos tienen delay exponencial: 1s, 2s, 4s
- El job procesa máximo 50 comprobantes por ejecución
- La carpeta `PDF/` se crea automáticamente si no existe
- Los PDFs sobrescriben archivos manuales previos en la misma ruta

## Contacto y Soporte

Para problemas o consultas sobre esta funcionalidad, revisar:
- Logs de Serilog en `Logs/`
- Dashboard de Hangfire en `/hangfire`
- Documentación de API de SUNAT
