# Resumen de Configuración y Deployment - ContabilidadAPI

## ✅ Cambios Realizados

### 1. Configuraciones Centralizadas en appsettings

**Archivos modificados:**
- [`appsettings.Development.json`](ContabilidadAPI/appsettings.Development.json)
- [`appsettings.Production.json`](ContabilidadAPI/appsettings.Production.json)

**Configuración de Azure Document Intelligence actualizada:**
```json
"AzureDocumentIntelligence": {
  "Enabled": true,
  "Endpoint": "https://pruebaiadoc.cognitiveservices.azure.com",
  "SubscriptionKey": "YOUR_AZURE_DOCUMENT_INTELLIGENCE_KEY_HERE",
  "ApiVersion": "2023-07-31",
  "ModelId": "prebuilt-invoice",
  "TimeoutSeconds": 120,
  "PollingIntervalMs": 2000,
  "MaxPollingAttempts": 60
}
```

### 2. web.config Simplificado

**Archivo modificado:** [`web.config`](ContabilidadAPI/web.config)

✅ Eliminadas las variables de entorno innecesarias de `web.config`
✅ Las configuraciones ahora se leen exclusivamente desde `appsettings.Production.json`
✅ El deployment de GitHub Actions inyecta los valores automáticamente

### 3. GitHub Actions Workflow Creado

**Archivo nuevo:** [`.github/workflows/deploy.yaml`](.github/workflows/deploy.yaml)

#### Características del workflow:
- ✅ Despliegue automático en IIS cuando se hace push a `main` o `master`
- ✅ Ejecución manual disponible (workflow_dispatch)
- ✅ Backup automático antes del deployment
- ✅ Actualización dinámica de `appsettings.Production.json` con 29 variables de GitHub
- ✅ Compilación, testing y publicación automatizada
- ✅ Gestión completa del Application Pool de IIS
- ✅ Verificación de deployment con estado detallado
- ✅ Mantiene solo los últimos 10 backups

#### Configuración del workflow:
```yaml
env:
  SITE_NAME: API_CONTABILIDAD
  SITE_PATH: C:\inetpub\wwwroot\API_CONTABILIDAD
  BACKUP_PATH: C:\inetpub\backups
  PROJECT_PATH: ContabilidadAPI/ContabilidadAPI.csproj
  PUBLISH_PATH: .\publish
```

### 4. Documentación de Variables de GitHub

**Archivo nuevo:** [`GITHUB_VARIABLES_CONFIG.md`](GITHUB_VARIABLES_CONFIG.md)

Documento completo con:
- ✅ Lista de las 29 variables requeridas
- ✅ Valores de cada variable
- ✅ Instrucciones paso a paso para configurar en GitHub
- ✅ Recomendaciones de seguridad

### 5. Template de web.config

**Archivo nuevo:** [`web.config.production.template`](ContabilidadAPI/web.config.production.template)

Plantilla de referencia por si necesitas sobrescribir configuraciones en el servidor de producción.

---

## 📋 Configuraciones Centralizadas

Todas las siguientes configuraciones ahora se gestionan desde appsettings y GitHub Variables:

### Connection Strings (3)
- BDSVRENDICIONES
- BDMARCACIONES  
- BDGNRLMOVITECNICA

### JWT Security (3)
- Key
- Issuer
- Audience

### SUNAT Configuration (5)
- ClientId
- ClientSecret
- TokenUrl
- ComprobanteUrl
- RUC

### OCR Configuration (10)
- TesseractDataPath
- DefaultLanguage
- DefaultPageSegMode
- EnableImagePreprocessing
- MaxFileSizeMB
- MaxPagesPerPdf
- DpiForPdfConversion
- SaveProcessedImages
- ProcessedImagesPath
- TimeoutSeconds

### Azure Document Intelligence (7)
- Enabled
- Endpoint
- SubscriptionKey
- ApiVersion
- ModelId
- TimeoutSeconds
- PollingIntervalMs
- MaxPollingAttempts

### Otros (1)
- AllowedHosts

**Total: 29 configuraciones gestionadas desde GitHub Variables**

---

## 🚀 Próximos Pasos

### 1. Configurar GitHub Variables

Ve a [`GITHUB_VARIABLES_CONFIG.md`](GITHUB_VARIABLES_CONFIG.md) y sigue las instrucciones para configurar todas las variables en GitHub.

**Ruta en GitHub:**
Settings → Secrets and variables → Actions → Variables tab

### 2. Verificar Runner de GitHub Actions

Asegúrate de tener configurado un self-hosted runner con las siguientes etiquetas:
- `self-hosted`
- `Windows`
- `X64`

### 3. Verificar IIS

El sitio debe existir previamente en IIS con el nombre: `API_CONTABILIDAD`

### 4. Ejecutar el Deployment

**Opción A: Push automático**
```bash
git add .
git commit -m "Configuración de deployment automatizado"
git push origin main
```

**Opción B: Ejecución manual**
1. Ve a GitHub → Actions
2. Selecciona "Deploy ContabilidadAPI to IIS Production"
3. Click en "Run workflow"
4. Selecciona la rama
5. Click en "Run workflow"

---

## 📊 Flujo del Deployment

```
1. Checkout del código
2. Setup .NET 8.0
3. Mostrar información del ambiente
4. ⚙️  Actualizar appsettings.Production.json con 29 variables
5. Restaurar dependencias NuGet
6. Compilar en modo Release
7. Ejecutar tests
8. Publicar aplicación
9. Verificar que el sitio IIS existe
10. 🛑 Detener Application Pool
11. 💾 Crear backup (mantiene últimos 10)
12. 🚀 Deploy a IIS (mantiene Logs, keys, tessdata, processed-images)
13. ▶️  Iniciar Application Pool
14. ✅ Verificar deployment exitoso
15. 📢 Notificación de estado
```

---

## 🔒 Notas de Seguridad

### Configuración Actual
Todas las configuraciones están como **GitHub Variables** (no Secrets).

### Recomendación para Mayor Seguridad
Si deseas mayor seguridad, mueve las siguientes a **GitHub Secrets**:
- `CONN_BDSVRENDICIONES`
- `CONN_BDMARCACIONES`
- `CONN_BDGNRLMOVITECNICA`
- `JWT_KEY`
- `SUNAT_CLIENT_SECRET`
- `AZURE_DOC_INTELLIGENCE_KEY`

Para usar Secrets, modifica en `deploy.yaml`:
```powershell
# De:
${{ vars.VARIABLE_NAME }}

# A:
${{ secrets.SECRET_NAME }}
```

---

## 📁 Estructura de Archivos Modificados/Creados

```
RENDICIONES_API/
├── .github/
│   └── workflows/
│       └── deploy.yaml ⭐ NUEVO
├── ContabilidadAPI/
│   ├── appsettings.Development.json ✏️ MODIFICADO
│   ├── appsettings.Production.json ✏️ MODIFICADO
│   ├── web.config ✏️ MODIFICADO
│   └── web.config.production.template ⭐ NUEVO
├── GITHUB_VARIABLES_CONFIG.md ⭐ NUEVO
└── DEPLOYMENT_SUMMARY.md ⭐ NUEVO (este archivo)
```

---

## ✅ Checklist de Implementación

- [x] Actualizar appsettings.Development.json con credenciales Azure
- [x] Actualizar appsettings.Production.json con credenciales Azure
- [x] Simplificar web.config
- [x] Crear workflow de GitHub Actions
- [x] Documentar variables de GitHub
- [x] Crear template de web.config
- [ ] Configurar 29 variables en GitHub
- [ ] Verificar self-hosted runner configurado
- [ ] Verificar sitio IIS existe
- [ ] Ejecutar primer deployment
- [ ] Validar deployment exitoso

---

## 🐛 Troubleshooting

### El workflow falla en "Verify IIS site exists"
- Asegúrate de que el sitio `API_CONTABILIDAD` existe en IIS
- Verifica que el runner tiene permisos para acceder a IIS

### El Application Pool no se detiene
- El workflow espera 30 segundos antes de forzar el stop
- Verifica que no haya procesos bloqueando el pool

### Error al actualizar appsettings.Production.json
- Verifica que todas las 29 variables están configuradas en GitHub
- Revisa los nombres de las variables (son case-sensitive)

### Error de permisos en deployment
- El runner debe tener permisos de escritura en `C:\inetpub\wwwroot\API_CONTABILIDAD`
- Verifica permisos en la carpeta de backups

---

## 📞 Soporte

Para más información sobre cada componente:
- **Variables de GitHub**: Ver [`GITHUB_VARIABLES_CONFIG.md`](GITHUB_VARIABLES_CONFIG.md)
- **Workflow Details**: Ver [`.github/workflows/deploy.yaml`](.github/workflows/deploy.yaml)
- **Configuración Azure**: Ver [`appsettings.Production.json`](ContabilidadAPI/appsettings.Production.json)

---

**Fecha de creación**: 18 de enero de 2026  
**Versión**: 1.0  
**Proyecto**: ContabilidadAPI - Sistema de Rendiciones Movitécnica
