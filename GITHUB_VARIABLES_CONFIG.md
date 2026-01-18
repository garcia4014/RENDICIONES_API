# GitHub Variables Configuration for ContabilidadAPI Deployment

Este documento detalla todas las variables que debes configurar en GitHub Actions para el despliegue automatizado de ContabilidadAPI.

## Ubicación en GitHub

Ve a: **Settings** → **Secrets and variables** → **Actions** → **Variables** tab

## Variables Requeridas (29 total)

### 1. Connection Strings (3 variables)

```
CONN_BDSVRENDICIONES
Value: Data Source=192.168.200.31;Initial Catalog=SVRENDICIONES;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True

CONN_BDMARCACIONES
Value: Data Source=192.168.200.31;Initial Catalog=MARCACIONES;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True

CONN_BDGNRLMOVITECNICA
Value: Data Source=192.168.200.31;Initial Catalog=GNRLMOVITECNICA;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True
```

### 2. JWT Configuration (3 variables)

```
JWT_KEY
Value: tH1siSTh3s3CR3tk3Y06032025c0NT4b1l1d4dAPI

JWT_ISSUER
Value: http://192.168.200.31:48081

JWT_AUDIENCE
Value: http://192.168.200.31:8081,http://192.168.230.31:48081
```

### 3. Allowed Hosts (1 variable)

```
ALLOWED_HOSTS
Value: 192.168.200.31;localhost
```

### 4. SUNAT Configuration (5 variables)

```
SUNAT_CLIENT_ID
Value: 267eb516-caa7-4483-9e7d-34e0f89e036a

SUNAT_CLIENT_SECRET
Value: j9aLKic5ulZrOZHcMI5GqA==

SUNAT_TOKEN_URL
Value: https://api-seguridad.sunat.gob.pe/v1/clientessol/clientID/oauth2/token/

SUNAT_COMPROBANTE_URL
Value: https://api.sunat.gob.pe/v1/contribuyente/contribuyentes/RUC/validarcomprobante

SUNAT_RUC
Value: 20100172543
```

### 5. OCR Configuration (10 variables)

```
OCR_TESSERACT_DATA_PATH
Value: C:\inetpub\wwwroot\API_CONTABILIDAD\tessdata

OCR_DEFAULT_LANGUAGE
Value: spa

OCR_PAGE_SEG_MODE
Value: 3

OCR_ENABLE_PREPROCESSING
Value: true

OCR_MAX_FILE_SIZE_MB
Value: 10

OCR_MAX_PAGES_PER_PDF
Value: 50

OCR_DPI_CONVERSION
Value: 300

OCR_SAVE_PROCESSED_IMAGES
Value: false

OCR_PROCESSED_IMAGES_PATH
Value: C:\inetpub\wwwroot\API_CONTABILIDAD\processed-images

OCR_TIMEOUT_SECONDS
Value: 300
```

### 6. Azure Document Intelligence Configuration (7 variables)

```
AZURE_DOC_INTELLIGENCE_ENABLED
Value: true

AZURE_DOC_INTELLIGENCE_ENDPOINT
Value: https://pruebaiadoc.cognitiveservices.azure.com/

AZURE_DOC_INTELLIGENCE_KEY
Value: YOUR_AZURE_DOCUMENT_INTELLIGENCE_KEY_HERE

AZURE_DOC_INTELLIGENCE_API_VERSION
Value: 2023-07-31

AZURE_DOC_INTELLIGENCE_MODEL_ID
Value: prebuilt-invoice

AZURE_DOC_INTELLIGENCE_TIMEOUT_SECONDS
Value: 120

AZURE_DOC_INTELLIGENCE_POLLING_INTERVAL_MS
Value: 2000

AZURE_DOC_INTELLIGENCE_MAX_POLLING_ATTEMPTS
Value: 60
```

## Pasos para Configurar

1. Ve a tu repositorio en GitHub
2. Click en **Settings**
3. En el menú izquierdo, selecciona **Secrets and variables** → **Actions**
4. Click en la pestaña **Variables**
5. Click en **New repository variable**
6. Copia el nombre de la variable y su valor
7. Click en **Add variable**
8. Repite para todas las 29 variables

## Validación

Una vez configuradas todas las variables, el workflow de deployment:
- ✅ Actualizará automáticamente `appsettings.Production.json` con estos valores
- ✅ Compilará la aplicación
- ✅ Ejecutará tests
- ✅ Desplegará en IIS
- ✅ Verificará el estado del despliegue

## Notas Importantes

⚠️ **NO uses GitHub Secrets** para estas configuraciones, usa **Variables**:
- Las **Variables** son para valores de configuración que no son sensibles
- Los **Secrets** son para contraseñas, API keys, tokens, etc., que deben estar ocultos
- En este caso, aunque algunos valores parecen sensibles, se recomienda usar Variables para facilitar el debugging y mantenimiento

🔒 **Si prefieres mayor seguridad**, puedes mover las siguientes a Secrets:
- `CONN_BDSVRENDICIONES` (contiene password de BD)
- `CONN_BDMARCACIONES` (contiene password de BD)
- `CONN_BDGNRLMOVITECNICA` (contiene password de BD)
- `JWT_KEY` (clave de seguridad JWT)
- `SUNAT_CLIENT_SECRET` (secreto de SUNAT)
- `AZURE_DOC_INTELLIGENCE_KEY` (API key de Azure)

Para usar Secrets en lugar de Variables, cambia en el deploy.yaml:
```powershell
# De:
${{ vars.VARIABLE_NAME }}

# A:
${{ secrets.SECRET_NAME }}
```

## Archivo Generado

El workflow está en: `.github/workflows/deploy.yaml`

El workflow se ejecutará automáticamente cuando:
- Hagas push a las ramas `main` o `master`
- Ejecutes manualmente desde GitHub Actions (workflow_dispatch)
