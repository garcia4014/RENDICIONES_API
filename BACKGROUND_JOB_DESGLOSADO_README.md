# Background Job: Procesamiento Automático de Comprobantes No Desglosados

## Descripción

Este background job se ejecuta automáticamente cada 5 minutos para procesar comprobantes de pago que no tienen el detalle de impuestos desglosado (campo `DESGLOSADO = 0` o `NULL`).

## Funcionamiento

### 1. **Búsqueda de Comprobantes**
El job busca en la tabla `COMPROBANTE_PAGO` todos los registros que cumplan:
- `Activo = true`
- `DESGLOSADO = false` o `DESGLOSADO = NULL`
- Tengan datos de RUC, Serie y Correlativo

### 2. **Obtención del XML desde SUNAT**
Para cada comprobante encontrado:
- Obtiene el token de autorización desde la tabla `PARAMETROS` (Id=1)
- Construye la URL de la API de SUNAT con los datos del comprobante
- Realiza una petición HTTP GET con reintentos (máximo 3 intentos)
- Descarga el archivo ZIP con el XML del comprobante
- Extrae y procesa el XML

### 3. **Procesamiento del XML**
El XML se procesa utilizando la clase `ComprobanteExtractor.ExtractFromXml()` que extrae:
- Montos gravados (operaciones afectas a IGV)
- Montos inafectos (no afectos a IGV)
- Montos exonerados (exonerados de IGV)
- Montos con IGV especial
- Montos de impuesto al consumo

### 4. **Actualización del Comprobante**
Si se obtiene exitosamente el XML, actualiza los siguientes campos:

| Campo | Descripción |
|-------|-------------|
| `MONTO_GRAVADO` | Monto de operaciones gravadas |
| `MONTO_INAFECTO` | Monto de operaciones inafectas |
| `MONTO_EXONERADO` | Monto de operaciones exoneradas |
| `MONTO_IGV_ESPECIAL` | Monto con IGV especial |
| `GRAVADO` | Flag booleano (true si hay monto gravado) |
| `INAFECTO` | Flag booleano (true si hay monto inafecto) |
| `EXONERADO` | Flag booleano (true si hay monto exonerado) |
| `IGV_ESPECIAL` | Flag booleano (true si hay IGV especial) |
| `IGV` | Monto calculado del IGV (si hay operaciones gravadas) |
| `SUBTOTAL` | Subtotal del comprobante |
| `DESGLOSADO` | Se marca como `true` para no volver a procesarlo |

### 5. **Cálculo de Impuestos**
- Si tiene **monto gravado**: `IGV = MontoGravado * (IgvPorcentaje / 100)`, `Subtotal = MontoGravado`
- Si tiene **monto inafecto**: `IGV = 0`, `Subtotal = MontoInafecto`
- Si tiene **monto exonerado**: `IGV = 0`, `Subtotal = MontoExonerado`

## Configuración

### Frecuencia de Ejecución
**Cada 5 minutos** (expresión CRON: `*/5 * * * *`)

### Límite de Procesamiento
Se procesan máximo **50 comprobantes por ejecución** para no sobrecargar el sistema.

### Reintentos a SUNAT
- **Máximo 3 intentos** por comprobante
- **Delays incrementales**: 1s, 2s, 3s
- Si el error es **401 Unauthorized**: No se reintenta (token inválido)
- Si hay errores 500, 503, 502: Se reintenta automáticamente

## Monitoreo

### Dashboard de Hangfire
Puedes monitorear el job en el dashboard de Hangfire:
```
https://<tu-servidor>/hangfire
```

### Logs
El job genera logs detallados en la categoría:
```
CapaNegocio.ContabilidadAPI.Repository.Implementation.ComprobanteDesglosadoBackgroundService
```

Ejemplo de logs:
```
[INFO] ===== INICIO: Procesamiento de comprobantes no desglosados =====
[INFO] Encontrados 15 comprobantes para procesar
[INFO] Procesando comprobante ID=123, RUC=20123456789, Serie=F001, Correlativo=00000001
[INFO] XML procesado - AfectacionDetectada: True, Gravados: 1, Inafectos: 0, Exonerados: 0
[INFO] Comprobante ID=123 actualizado exitosamente - Gravado:850.00, Inafecto:0, Exonerado:0
[INFO] ===== FIN: Procesamiento completado - Exitosos: 12, Fallidos: 3 =====
```

## Archivo de Código

**Ruta**: `CapaNegocio.ContabilidadAPI/Repository/Implementation/ComprobanteDesglosadoBackgroundService.cs`

## Configuración en Program.cs

```csharp
// Registrar el servicio
builder.Services.AddScoped<ComprobanteDesglosadoBackgroundService>();

// Configurar job recurrente
RecurringJob.AddOrUpdate<ComprobanteDesglosadoBackgroundService>(
    "procesar-comprobantes-no-desglosados",
    service => service.ProcesarComprobantesNoDesglosados(),
    "*/5 * * * *"); // Cada 5 minutos
```

## Ejecución Manual

Si necesitas ejecutar el job manualmente (por ejemplo, para procesar comprobantes pendientes de inmediato), puedes hacerlo desde el dashboard de Hangfire:

1. Ir a `/hangfire`
2. Navegar a "Recurring Jobs"
3. Buscar el job `procesar-comprobantes-no-desglosados`
4. Hacer clic en "Trigger now"

## Consideraciones

### Performance
- **Delay entre comprobantes**: 500ms para evitar saturar la API de SUNAT
- **Commit individual**: Cada comprobante se guarda independientemente para no perder el progreso en caso de error
- **Timeout**: Si un comprobante falla, se marca como `DESGLOSADO=true` para no bloquearlo indefinidamente

### Seguridad
- El token de SUNAT se obtiene de la base de datos (`PARAMETROS.Id=1`)
- Si el token está vencido o es inválido (401), el job lo detecta y no reintenta

### Manejo de Errores
- **Errores de red**: Se reintentan automáticamente
- **Errores de SUNAT**: Se logean y el comprobante se marca como procesado
- **XML inválido**: Se logea el error y se marca como procesado
- **Errores generales**: Se capturan y logean sin detener el procesamiento de los demás comprobantes

## Dependencias

- **Hangfire**: Framework de background jobs
- **Entity Framework Core**: Para acceso a base de datos
- **HttpClient**: Para llamadas a la API de SUNAT
- **System.IO.Compression**: Para extraer XML del ZIP
- **System.Text.Json**: Para deserializar respuestas JSON

## Tabla de Parámetros

La tabla `PARAMETROS` debe contener el token de SUNAT:

| Id | Nombre | Valor | Descripción |
|----|--------|-------|-------------|
| 1 | TOKEN_SUNAT | Bearer eyJ... | Token de autorización para API de SUNAT |

## Flujo Completo

```
┌─────────────────────────────────────────────┐
│  Job ejecutado cada 5 minutos (CRON)       │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│  Buscar comprobantes con DESGLOSADO=0       │
│  (máximo 50 por ejecución)                  │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│  Para cada comprobante:                     │
│  1. Obtener token desde PARAMETROS          │
│  2. Llamar API SUNAT (con reintentos)       │
│  3. Descargar y extraer XML del ZIP         │
│  4. Procesar XML con ComprobanteExtractor   │
│  5. Actualizar campos de impuestos          │
│  6. Marcar DESGLOSADO=true                  │
│  7. Guardar en BD                           │
│  8. Delay 500ms                             │
└────────────────┬────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────┐
│  Log de resultados:                         │
│  - Comprobantes procesados exitosamente     │
│  - Comprobantes con errores                 │
└─────────────────────────────────────────────┘
```

## Troubleshooting

### El job no se ejecuta
- Verificar que Hangfire Server esté iniciado correctamente
- Revisar logs de la aplicación
- Verificar conexión a la base de datos de Hangfire

### Todos los comprobantes fallan
- Verificar que el token de SUNAT en `PARAMETROS` sea válido
- Comprobar conectividad con la API de SUNAT
- Revisar que los datos del comprobante (RUC, Serie, Correlativo) sean correctos

### El job se ejecuta pero no actualiza
- Verificar que los comprobantes tengan `DESGLOSADO=false` o `NULL`
- Comprobar que tengan RUC, Serie y Correlativo válidos
- Revisar logs para ver errores específicos

## Actualización Manual de Configuración

Para cambiar la frecuencia de ejecución, editar en `Program.cs`:

```csharp
// Cada 10 minutos
"*/10 * * * *"

// Cada hora
"0 * * * *"

// Cada día a las 2 AM
"0 2 * * *"
```

## Fecha de Creación
02 de febrero de 2026
