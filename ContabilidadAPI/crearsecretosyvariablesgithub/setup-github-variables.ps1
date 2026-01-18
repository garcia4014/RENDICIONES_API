# Script para crear VARIABLES en GitHub para ContabilidadAPI
# Ejecuta este script después de instalar GitHub CLI y autenticarte con: gh auth login

$REPO = "garcia4014/RENDICIONES_API"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Creando VARIABLES en GitHub" -ForegroundColor Cyan
Write-Host "Proyecto: ContabilidadAPI" -ForegroundColor White
Write-Host "Repositorio: $REPO" -ForegroundColor Yellow
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Verificar autenticación
Write-Host "Verificando autenticación de GitHub CLI..." -ForegroundColor Yellow
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "No estás autenticado en GitHub CLI. Ejecuta: gh auth login"
    exit 1
}
Write-Host "✓ Autenticado correctamente" -ForegroundColor Green
Write-Host ""

# VARIABLES (29 total)
Write-Host "Creando 29 VARIABLES para ContabilidadAPI..." -ForegroundColor Yellow
Write-Host ""

# Connection Strings (3 variables)
Write-Host "[1/6] Connection Strings (3)" -ForegroundColor Cyan

Write-Host "  → CONN_BDSVRENDICIONES" -ForegroundColor Gray
gh variable set CONN_BDSVRENDICIONES --repo $REPO --body "Data Source=192.168.200.31;Initial Catalog=SVRENDICIONES;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → CONN_BDMARCACIONES" -ForegroundColor Gray
gh variable set CONN_BDMARCACIONES --repo $REPO --body "Data Source=192.168.200.31;Initial Catalog=MARCACIONES;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → CONN_BDGNRLMOVITECNICA" -ForegroundColor Gray
gh variable set CONN_BDGNRLMOVITECNICA --repo $REPO --body "Data Source=192.168.200.31;Initial Catalog=GNRLMOVITECNICA;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

# JWT Configuration (3 variables)
Write-Host "`n[2/6] JWT Configuration (3)" -ForegroundColor Cyan

Write-Host "  → JWT_KEY" -ForegroundColor Gray
gh variable set JWT_KEY --repo $REPO --body "tH1siSTh3s3CR3tk3Y06032025c0NT4b1l1d4dAPI"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → JWT_ISSUER" -ForegroundColor Gray
gh variable set JWT_ISSUER --repo $REPO --body "http://192.168.200.31:48081"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → JWT_AUDIENCE" -ForegroundColor Gray
gh variable set JWT_AUDIENCE --repo $REPO --body "http://192.168.200.31:8081,http://192.168.230.31:48081"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

# Allowed Hosts (1 variable)
Write-Host "`n[3/6] Allowed Hosts (1)" -ForegroundColor Cyan

Write-Host "  → ALLOWED_HOSTS" -ForegroundColor Gray
gh variable set ALLOWED_HOSTS --repo $REPO --body "192.168.200.31;localhost"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

# SUNAT Configuration (5 variables)
Write-Host "`n[4/6] SUNAT Configuration (5)" -ForegroundColor Cyan

Write-Host "  → SUNAT_CLIENT_ID" -ForegroundColor Gray
gh variable set SUNAT_CLIENT_ID --repo $REPO --body "267eb516-caa7-4483-9e7d-34e0f89e036a"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → SUNAT_CLIENT_SECRET" -ForegroundColor Gray
gh variable set SUNAT_CLIENT_SECRET --repo $REPO --body "j9aLKic5ulZrOZHcMI5GqA=="
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → SUNAT_TOKEN_URL" -ForegroundColor Gray
gh variable set SUNAT_TOKEN_URL --repo $REPO --body "https://api-seguridad.sunat.gob.pe/v1/clientessol/clientID/oauth2/token/"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → SUNAT_COMPROBANTE_URL" -ForegroundColor Gray
gh variable set SUNAT_COMPROBANTE_URL --repo $REPO --body "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes/RUC/validarcomprobante"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → SUNAT_RUC" -ForegroundColor Gray
gh variable set SUNAT_RUC --repo $REPO --body "20100172543"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

# OCR Configuration (10 variables)
Write-Host "`n[5/6] OCR Configuration (10)" -ForegroundColor Cyan

Write-Host "  → OCR_TESSERACT_DATA_PATH" -ForegroundColor Gray
gh variable set OCR_TESSERACT_DATA_PATH --repo $REPO --body "C:\inetpub\wwwroot\API_CONTABILIDAD\tessdata"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_DEFAULT_LANGUAGE" -ForegroundColor Gray
gh variable set OCR_DEFAULT_LANGUAGE --repo $REPO --body "spa"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_PAGE_SEG_MODE" -ForegroundColor Gray
gh variable set OCR_PAGE_SEG_MODE --repo $REPO --body "3"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_ENABLE_PREPROCESSING" -ForegroundColor Gray
gh variable set OCR_ENABLE_PREPROCESSING --repo $REPO --body "true"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_MAX_FILE_SIZE_MB" -ForegroundColor Gray
gh variable set OCR_MAX_FILE_SIZE_MB --repo $REPO --body "10"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_MAX_PAGES_PER_PDF" -ForegroundColor Gray
gh variable set OCR_MAX_PAGES_PER_PDF --repo $REPO --body "50"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_DPI_CONVERSION" -ForegroundColor Gray
gh variable set OCR_DPI_CONVERSION --repo $REPO --body "300"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_SAVE_PROCESSED_IMAGES" -ForegroundColor Gray
gh variable set OCR_SAVE_PROCESSED_IMAGES --repo $REPO --body "false"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_PROCESSED_IMAGES_PATH" -ForegroundColor Gray
gh variable set OCR_PROCESSED_IMAGES_PATH --repo $REPO --body "C:\inetpub\wwwroot\API_CONTABILIDAD\processed-images"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → OCR_TIMEOUT_SECONDS" -ForegroundColor Gray
gh variable set OCR_TIMEOUT_SECONDS --repo $REPO --body "300"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

# Azure Document Intelligence (7 variables)
Write-Host "`n[6/6] Azure Document Intelligence (7)" -ForegroundColor Cyan

Write-Host "  → AZURE_DOC_INTELLIGENCE_ENABLED" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_ENABLED --repo $REPO --body "true"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_ENDPOINT" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_ENDPOINT --repo $REPO --body "https://pruebaiadoc.cognitiveservices.azure.com"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_KEY" -ForegroundColor Gray
# IMPORTANTE: Reemplaza YOUR_AZURE_KEY con tu clave real de Azure Document Intelligence
gh variable set AZURE_DOC_INTELLIGENCE_KEY --repo $REPO --body "YOUR_AZURE_KEY"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_API_VERSION" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_API_VERSION --repo $REPO --body "2023-07-31"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_MODEL_ID" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_MODEL_ID --repo $REPO --body "prebuilt-invoice"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_TIMEOUT_SECONDS" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_TIMEOUT_SECONDS --repo $REPO --body "120"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_POLLING_INTERVAL_MS" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_POLLING_INTERVAL_MS --repo $REPO --body "2000"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host "  → AZURE_DOC_INTELLIGENCE_MAX_POLLING_ATTEMPTS" -ForegroundColor Gray
gh variable set AZURE_DOC_INTELLIGENCE_MAX_POLLING_ATTEMPTS --repo $REPO --body "60"
if ($LASTEXITCODE -eq 0) { Write-Host "    ✓ Creado" -ForegroundColor Green } else { Write-Host "    ✗ Error" -ForegroundColor Red }

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "✓ VARIABLES configuradas (29 total)" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Desglose:" -ForegroundColor Yellow
Write-Host "  • Connection Strings: 3" -ForegroundColor White
Write-Host "  • JWT Configuration: 3" -ForegroundColor White
Write-Host "  • Allowed Hosts: 1" -ForegroundColor White
Write-Host "  • SUNAT Configuration: 5" -ForegroundColor White
Write-Host "  • OCR Configuration: 10" -ForegroundColor White
Write-Host "  • Azure Document Intelligence: 7" -ForegroundColor White
Write-Host ""
Write-Host "Verifica las variables en:" -ForegroundColor Yellow
Write-Host "https://github.com/$REPO/settings/variables/actions" -ForegroundColor Cyan
