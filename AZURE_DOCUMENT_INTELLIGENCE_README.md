# Azure Document Intelligence Integration

## Descripción General

Se ha integrado **Azure Document Intelligence** (Form Recognizer) al sistema OCR existente. El servicio funciona de manera híbrida:

- **Modo Primario**: Cuando está habilitado en `appsettings.json`, el sistema usará Azure Document Intelligence para extraer información de documentos.
- **Modo Fallback**: Si Azure IA falla o no está habilitado, el sistema automáticamente usará Tesseract OCR como respaldo.

## Configuración

### appsettings.json / appsettings.Development.json / appsettings.Production.json

```json
{
  "AzureDocumentIntelligence": {
    "Enabled": true,
    "Endpoint": "https://pruebaiadoc.cognitiveservices.azure.com",
    "SubscriptionKey": "YOUR_SUBSCRIPTION_KEY_HERE",
    "ApiVersion": "2023-07-31",
    "ModelId": "prebuilt-invoice",
    "TimeoutSeconds": 120,
    "PollingIntervalMs": 2000,
    "MaxPollingAttempts": 60
  }
}
```

### Parámetros de Configuración

- **Enabled**: `true` para activar Azure IA, `false` para usar solo Tesseract
- **Endpoint**: URL del servicio de Azure Cognitive Services
- **SubscriptionKey**: Clave de suscripción (Ocp-Apim-Subscription-Key)
- **ApiVersion**: Versión de la API (por defecto: 2023-07-31)
- **ModelId**: Modelo a utilizar (por defecto: prebuilt-invoice para facturas)
- **TimeoutSeconds**: Tiempo máximo de espera para el análisis completo
- **PollingIntervalMs**: Intervalo entre consultas de estado (en milisegundos)
- **MaxPollingAttempts**: Número máximo de intentos para obtener el resultado

## Nuevos Endpoints

### 1. Analizar documento desde URL (Azure IA)

**POST** `/api/Ocr/azure-analyze-url`

Analiza un documento público desde una URL usando Azure Document Intelligence.

**Request Body:**
```json
{
  "urlSource": "https://example.com/invoice.pdf",
  "queryFields": [
    "ruc_emisor",
    "serie_correlativo",
    "fecha_emision",
    "moneda",
    "monto_total_neto"
  ]
}
```

**Response:**
```json
{
  "data": {
    "status": "succeeded",
    "analyzeResult": {
      "content": "...",
      "documents": [
        {
          "fields": {
            "InvoiceId": {
              "type": "string",
              "valueString": "EB01-2",
              "confidence": 0.684
            },
            "InvoiceDate": {
              "type": "date",
              "valueDate": "2020-12-19",
              "confidence": 0.938
            },
            "InvoiceTotal": {
              "type": "currency",
              "valueCurrency": {
                "amount": 17426.0,
                "currencyCode": "EUR"
              },
              "confidence": 0.82
            }
          }
        }
      ]
    }
  },
  "message": "Análisis completado exitosamente",
  "success": true
}
```

### 2. Analizar documento desde archivo (Azure IA)

**POST** `/api/Ocr/azure-analyze-file`

Analiza un documento subido como archivo usando Azure Document Intelligence.

**Form Data:**
- `file`: Archivo del documento (PDF, JPG, PNG, etc.)
- `queryFields`: (Opcional) Campos personalizados separados por coma

**Ejemplo con curl:**
```bash
curl -X POST "https://your-api/api/Ocr/azure-analyze-file" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@factura.pdf" \
  -F "queryFields=ruc_emisor,serie_correlativo,monto_total_neto"
```

### 3. Estado de servicios OCR

**GET** `/api/Ocr/services-status`

Obtiene el estado de configuración de Tesseract y Azure Document Intelligence.

**Response:**
```json
{
  "data": {
    "tesseract": {
      "isConfigured": true,
      "status": "OK",
      "message": "Tesseract OCR configurado correctamente"
    },
    "azureDocumentIntelligence": {
      "isEnabled": true,
      "status": "ENABLED",
      "message": "Azure Document Intelligence está habilitado y configurado"
    },
    "preferredService": "Azure Document Intelligence",
    "supportedFormats": ["PDF", "JPG", "JPEG", "PNG", "BMP", "TIFF"]
  },
  "message": "Estado de servicios OCR obtenido exitosamente",
  "success": true
}
```

## Endpoints Existentes (Ahora con Azure IA)

Los endpoints existentes ahora usan automáticamente Azure IA cuando está habilitado:

### POST `/api/Ocr/extract-text`
- Usa Azure IA si está habilitado
- Fallback a Tesseract si Azure falla o no está habilitado

### POST `/api/Ocr/extract-text-image`
- Usa Azure IA para imágenes
- Fallback a Tesseract

### POST `/api/Ocr/extract-text-pdf`
- Usa Azure IA para PDFs
- Fallback a Tesseract

### POST `/api/Ocr/extract-text-all`
- Extrae información completa del comprobante
- Usa Azure IA cuando está disponible

## Flujo de Procesamiento

```
1. Usuario envía documento
   ↓
2. ¿Azure IA habilitado?
   ├─ SÍ → Intenta con Azure Document Intelligence
   │        ├─ Éxito → Retorna resultado de Azure
   │        └─ Error → Fallback a Tesseract
   └─ NO → Usa Tesseract directamente
```

## Arquitectura de Servicios

### Servicios Implementados

1. **IAzureDocumentIntelligenceService**: Interface para Azure Document Intelligence
2. **AzureDocumentIntelligenceService**: Implementación del servicio de Azure IA
3. **HybridOcrService**: Servicio híbrido que decide entre Azure IA y Tesseract
4. **TesseractOcrService**: Servicio OCR existente con Tesseract

### DTOs Creados

1. **AzureDocumentIntelligenceConfigurationDto**: Configuración del servicio
2. **AzureDocumentIntelligenceRequestDto**: Solicitud de análisis
3. **AzureDocumentIntelligenceResponseDto**: Respuesta del análisis con fields extraídos

## Modelos de Azure Document Intelligence

### prebuilt-invoice (por defecto)
Optimizado para facturas, extrae:
- InvoiceId (número de factura)
- InvoiceDate (fecha de emisión)
- InvoiceTotal (monto total)
- VendorName (nombre del proveedor)
- VendorTaxId (RUC del emisor)
- CustomerName (nombre del cliente)
- Items (líneas de la factura)
- SubTotal, TotalTax, etc.

### Otros modelos disponibles
- `prebuilt-receipt`: Recibos
- `prebuilt-businessCard`: Tarjetas de presentación
- `prebuilt-idDocument`: Documentos de identidad
- `prebuilt-document`: Documentos generales

Para cambiar el modelo, modifica `ModelId` en `appsettings.json`.

## Estructura de Fields Extraídos

Los campos (`fields`) en la respuesta de Azure tienen la siguiente estructura:

```json
{
  "FieldName": {
    "type": "string|date|currency|number|address|array|object",
    "valueString": "valor si es string",
    "valueDate": "valor si es fecha",
    "valueNumber": 123.45,
    "valueCurrency": {
      "amount": 17426.0,
      "currencySymbol": "S/.",
      "currencyCode": "PEN"
    },
    "content": "texto extraído",
    "confidence": 0.85,
    "boundingRegions": [...],
    "spans": [...]
  }
}
```

## Ejemplo de Uso Completo

### Ejemplo 1: Analizar factura desde URL

```javascript
const response = await fetch('https://your-api/api/Ocr/azure-analyze-url', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer YOUR_TOKEN'
  },
  body: JSON.stringify({
    urlSource: 'https://imgv2-2-f.scribdassets.com/img/document/664848593/original/75c77008a1/1712704210',
    queryFields: [
      'ruc_emisor',
      'serie_correlativo',
      'fecha_emision',
      'moneda',
      'monto_total_neto'
    ]
  })
});

const result = await response.json();
console.log('Factura analizada:', result.data);
```

### Ejemplo 2: Analizar documento desde archivo

```javascript
const formData = new FormData();
formData.append('file', documentFile);
formData.append('queryFields', 'ruc_emisor,serie_correlativo,monto_total_neto');

const response = await fetch('https://your-api/api/Ocr/azure-analyze-file', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer YOUR_TOKEN'
  },
  body: formData
});

const result = await response.json();
console.log('Documento analizado:', result.data);
```

## Registro de Actividad (Logs)

El servicio registra toda la actividad:

```
[INFO] Iniciando análisis de documento desde URL: https://...
[INFO] Documento enviado para análisis. Operation-Location: https://...
[INFO] Intento 1/60 - Estado: running
[INFO] Intento 2/60 - Estado: running
[INFO] Intento 3/60 - Estado: succeeded
[INFO] Análisis completado exitosamente
[INFO] Análisis completado en 4523ms
```

## Consideraciones de Seguridad

1. **Credenciales**: La `SubscriptionKey` debe mantenerse segura
2. **URLs Públicas**: Solo usar URLs públicamente accesibles para el análisis
3. **Archivos**: Validar tamaño y tipo de archivos subidos
4. **Logs**: No registrar información sensible en los logs

## Troubleshooting

### Error: "Azure Document Intelligence no está habilitado"
- Verificar que `Enabled: true` en appsettings
- Verificar que `Endpoint` y `SubscriptionKey` estén configurados

### Error: "Tiempo de espera agotado"
- Aumentar `TimeoutSeconds` en configuración
- Aumentar `MaxPollingAttempts`
- Verificar conectividad con Azure

### Error: "Operation-Location no encontrado"
- Verificar que el `Endpoint` sea correcto
- Verificar que la `SubscriptionKey` sea válida
- Revisar logs para más detalles

### El sistema usa Tesseract en lugar de Azure
- Verificar configuración `Enabled: true`
- Revisar logs para ver si hay errores de Azure
- Verificar conectividad con el servicio de Azure

## Migración desde Tesseract

No se requiere migración. El sistema es compatible hacia atrás:
- Los endpoints existentes siguen funcionando
- Tesseract sigue disponible como fallback
- Se pueden usar ambos servicios simultáneamente

## Próximos Pasos

1. Monitorear el uso y costos de Azure Document Intelligence
2. Optimizar el polling interval según necesidades
3. Considerar cache de resultados para documentos frecuentes
4. Implementar métricas de precisión Azure vs Tesseract
5. Agregar soporte para modelos personalizados de Azure

## Soporte y Documentación

- **Azure Document Intelligence**: https://learn.microsoft.com/azure/ai-services/document-intelligence/
- **API Reference**: https://learn.microsoft.com/rest/api/aiservices/document-models/analyze-document
- **Modelos Prebuilt**: https://learn.microsoft.com/azure/ai-services/document-intelligence/concept-model-overview

---

**Fecha de implementación**: Diciembre 2025  
**Versión**: 1.0  
**Autor**: Sistema de IA
