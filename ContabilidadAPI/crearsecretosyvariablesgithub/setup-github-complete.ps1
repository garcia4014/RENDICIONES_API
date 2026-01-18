# Script combinado para configurar VARIABLES en GitHub para ContabilidadAPI
# Requisito: GitHub CLI instalado y autenticado (gh auth login)

$REPO = "garcia4014/RENDICIONES_API"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ContabilidadAPI - Setup GitHub Actions" -ForegroundColor White
Write-Host " Repositorio: $REPO" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si GitHub CLI está instalado
Write-Host "Verificando GitHub CLI..." -ForegroundColor Yellow
$ghExists = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghExists) {
    Write-Host "✗ GitHub CLI no está instalado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Instala GitHub CLI desde:" -ForegroundColor Yellow
    Write-Host "  https://cli.github.com/" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "O con winget:" -ForegroundColor Yellow
    Write-Host "  winget install --id GitHub.cli" -ForegroundColor Cyan
    exit 1
}
Write-Host "✓ GitHub CLI instalado" -ForegroundColor Green

# Verificar autenticación
Write-Host "Verificando autenticación..." -ForegroundColor Yellow
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ No estás autenticado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Autentícate ejecutando:" -ForegroundColor Yellow
    Write-Host "  gh auth login" -ForegroundColor Cyan
    exit 1
}
Write-Host "✓ Autenticado correctamente" -ForegroundColor Green
Write-Host ""

# Confirmar antes de continuar
Write-Host "Este script creará 29 VARIABLES en GitHub:" -ForegroundColor Yellow
Write-Host "  • 3 Connection Strings" -ForegroundColor White
Write-Host "  • 3 JWT Configuration" -ForegroundColor White
Write-Host "  • 1 Allowed Hosts" -ForegroundColor White
Write-Host "  • 5 SUNAT Configuration" -ForegroundColor White
Write-Host "  • 10 OCR Configuration" -ForegroundColor White
Write-Host "  • 7 Azure Document Intelligence" -ForegroundColor White
Write-Host ""
$confirm = Read-Host "¿Deseas continuar? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Operación cancelada" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Creando VARIABLES (29)" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# VARIABLES
$variablesCreated = 0
$variablesFailed = 0
$varCount = 1

# Connection Strings (3 variables)
Write-Host "» Connection Strings" -ForegroundColor Cyan
$variables = @(
    @{Name="CONN_BDSVRENDICIONES"; Value="Data Source=192.168.200.31;Initial Catalog=SVRENDICIONES;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True"},
    @{Name="CONN_BDMARCACIONES"; Value="Data Source=192.168.200.31;Initial Catalog=MARCACIONES;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True"},
    @{Name="CONN_BDGNRLMOVITECNICA"; Value="Data Source=192.168.200.31;Initial Catalog=GNRLMOVITECNICA;Persist Security Info=True;User ID=sa;Password=B1Admin;Encrypt=True;TrustServerCertificate=True"}
)

foreach ($var in $variables) {
    Write-Host "[$varCount/29] $($var.Name)" -ForegroundColor Gray
    gh variable set $var.Name --repo $REPO --body $var.Value 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "        ✓ Creado" -ForegroundColor Green
        $variablesCreated++
    } else {
        Write-Host "        ✗ Error" -ForegroundColor Red
        $variablesFailed++
    }
    $varCount++
}

# JWT Configuration (3 variables)
Write-Host ""
Write-Host "» JWT Configuration" -ForegroundColor Cyan
$variables = @(
    @{Name="JWT_KEY"; Value="tH1siSTh3s3CR3tk3Y06032025c0NT4b1l1d4dAPI"},
    @{Name="JWT_ISSUER"; Value="http://192.168.200.31:48081"},
    @{Name="JWT_AUDIENCE"; Value="http://192.168.200.31:8081,http://192.168.230.31:48081"}
)

foreach ($var in $variables) {
    Write-Host "[$varCount/29] $($var.Name)" -ForegroundColor Gray
    gh variable set $var.Name --repo $REPO --body $var.Value 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "        ✓ Creado" -ForegroundColor Green
        $variablesCreated++
    } else {
        Write-Host "        ✗ Error" -ForegroundColor Red
        $variablesFailed++
    }
    $varCount++
}

# Allowed Hosts (1 variable)
Write-Host ""
Write-Host "» Allowed Hosts" -ForegroundColor Cyan
$variables = @(
    @{Name="ALLOWED_HOSTS"; Value="192.168.200.31;localhost"}
)

foreach ($var in $variables) {
    Write-Host "[$varCount/29] $($var.Name)" -ForegroundColor Gray
    gh variable set $var.Name --repo $REPO --body $var.Value 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "        ✓ Creado" -ForegroundColor Green
        $variablesCreated++
    } else {
        Write-Host "        ✗ Error" -ForegroundColor Red
        $variablesFailed++
    }
    $varCount++
}

# SUNAT Configuration (5 variables)
Write-Host ""
Write-Host "» SUNAT Configuration" -ForegroundColor Cyan
$variables = @(
    @{Name="SUNAT_CLIENT_ID"; Value="267eb516-caa7-4483-9e7d-34e0f89e036a"},
    @{Name="SUNAT_CLIENT_SECRET"; Value="j9aLKic5ulZrOZHcMI5GqA=="},
    @{Name="SUNAT_TOKEN_URL"; Value="https://api-seguridad.sunat.gob.pe/v1/clientessol/clientID/oauth2/token/"},
    @{Name="SUNAT_COMPROBANTE_URL"; Value="https://api.sunat.gob.pe/v1/contribuyente/contribuyentes/RUC/validarcomprobante"},
    @{Name="SUNAT_RUC"; Value="20100172543"}
)

foreach ($var in $variables) {
    Write-Host "[$varCount/29] $($var.Name)" -ForegroundColor Gray
    gh variable set $var.Name --repo $REPO --body $var.Value 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "        ✓ Creado" -ForegroundColor Green
        $variablesCreated++
    } else {
        Write-Host "        ✗ Error" -ForegroundColor Red
        $variablesFailed++
    }
    $varCount++
}

# OCR Configuration (10 variables)
Write-Host ""
Write-Host "» OCR Configuration" -ForegroundColor Cyan
$variables = @(
    @{Name="OCR_TESSERACT_DATA_PATH"; Value="C:\inetpub\wwwroot\API_CONTABILIDAD\tessdata"},
    @{Name="OCR_DEFAULT_LANGUAGE"; Value="spa"},
    @{Name="OCR_PAGE_SEG_MODE"; Value="3"},
    @{Name="OCR_ENABLE_PREPROCESSING"; Value="true"},
    @{Name="OCR_MAX_FILE_SIZE_MB"; Value="10"},
    @{Name="OCR_MAX_PAGES_PER_PDF"; Value="50"},
    @{Name="OCR_DPI_CONVERSION"; Value="300"},
    @{Name="OCR_SAVE_PROCESSED_IMAGES"; Value="false"},
    @{Name="OCR_PROCESSED_IMAGES_PATH"; Value="C:\inetpub\wwwroot\API_CONTABILIDAD\processed-images"},
    @{Name="OCR_TIMEOUT_SECONDS"; Value="300"}
)

foreach ($var in $variables) {
    Write-Host "[$varCount/29] $($var.Name)" -ForegroundColor Gray
    gh variable set $var.Name --repo $REPO --body $var.Value 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "        ✓ Creado" -ForegroundColor Green
        $variablesCreated++
    } else {
        Write-Host "        ✗ Error" -ForegroundColor Red
        $variablesFailed++
    }
    $varCount++
}

# Azure Document Intelligence (7 variables)
Write-Host ""
Write-Host "» Azure Document Intelligence" -ForegroundColor Cyan
# IMPORTANTE: Reemplaza YOUR_AZURE_KEY con tu clave real de Azure Document Intelligence
$variables = @(
    @{Name="AZURE_DOC_INTELLIGENCE_ENABLED"; Value="true"},
    @{Name="AZURE_DOC_INTELLIGENCE_ENDPOINT"; Value="https://pruebaiadoc.cognitiveservices.azure.com"},
    @{Name="AZURE_DOC_INTELLIGENCE_KEY"; Value="YOUR_AZURE_KEY"},
    @{Name="AZURE_DOC_INTELLIGENCE_API_VERSION"; Value="2023-07-31"},
    @{Name="AZURE_DOC_INTELLIGENCE_MODEL_ID"; Value="prebuilt-invoice"},
    @{Name="AZURE_DOC_INTELLIGENCE_TIMEOUT_SECONDS"; Value="120"},
    @{Name="AZURE_DOC_INTELLIGENCE_POLLING_INTERVAL_MS"; Value="2000"},
    @{Name="AZURE_DOC_INTELLIGENCE_MAX_POLLING_ATTEMPTS"; Value="60"}
)

foreach ($var in $variables) {
    Write-Host "[$varCount/29] $($var.Name)" -ForegroundColor Gray
    gh variable set $var.Name --repo $REPO --body $var.Value 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "        ✓ Creado" -ForegroundColor Green
        $variablesCreated++
    } else {
        Write-Host "        ✗ Error" -ForegroundColor Red
        $variablesFailed++
    }
    $varCount++
}

# Resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " RESUMEN" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$variablesColor = if ($variablesCreated -eq 29) { "Green" } else { "Yellow" }
Write-Host "Variables creadas: $variablesCreated/29" -ForegroundColor $variablesColor

$variablesFailedColor = if ($variablesFailed -eq 0) { "Green" } else { "Red" }
Write-Host "Variables fallidas: $variablesFailed/29" -ForegroundColor $variablesFailedColor

Write-Host ""

if ($variablesFailed -eq 0) {
    Write-Host "✓ CONFIGURACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host ""
    Write-Host "Desglose de variables creadas:" -ForegroundColor Yellow
    Write-Host "  • Connection Strings: 3" -ForegroundColor White
    Write-Host "  • JWT Configuration: 3" -ForegroundColor White
    Write-Host "  • Allowed Hosts: 1" -ForegroundColor White
    Write-Host "  • SUNAT Configuration: 5" -ForegroundColor White
    Write-Host "  • OCR Configuration: 10" -ForegroundColor White
    Write-Host "  • Azure Document Intelligence: 7" -ForegroundColor White
    Write-Host ""
    Write-Host "Verifica la configuración en:" -ForegroundColor Yellow
    Write-Host "  https://github.com/$REPO/settings/variables/actions" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Próximo paso:" -ForegroundColor Yellow
    Write-Host "  1. Verifica que el self-hosted runner esté configurado" -ForegroundColor White
    Write-Host "  2. Haz push a main/master para ejecutar el workflow" -ForegroundColor White
} else {
    Write-Host "⚠ CONFIGURACIÓN COMPLETADA CON ERRORES" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Revisa los errores arriba y configura manualmente los elementos fallidos" -ForegroundColor Yellow
}

Write-Host ""
}

# Resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " RESUMEN" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$variablesColor = if ($variablesCreated -eq 16) { "Green" } else { "Yellow" }
Write-Host "Variables creadas: $variablesCreated/16" -ForegroundColor $variablesColor

$variablesFailedColor = if ($variablesFailed -eq 0) { "Green" } else { "Red" }
Write-Host "Variables fallidas: $variablesFailed/16" -ForegroundColor $variablesFailedColor

Write-Host ""

if ($variablesFailed -eq 0) {
    Write-Host "✓ CONFIGURACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host ""
    Write-Host "Verifica la configuración en:" -ForegroundColor Yellow
    Write-Host "  Variables: https://github.com/$REPO/settings/variables/actions" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Próximo paso: Hacer push a main/master para ejecutar el workflow" -ForegroundColor Yellow
} else {
    Write-Host "⚠ CONFIGURACIÓN COMPLETADA CON ERRORES" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Revisa los errores arriba y configura manualmente los elementos fallidos" -ForegroundColor Yellow
}

Write-Host ""
