# Script para crear SECRETS en GitHub para ContabilidadAPI
# Ejecuta este script después de instalar GitHub CLI y autenticarte con: gh auth login
# NOTA: Para ContabilidadAPI, todas las configuraciones se manejan como VARIABLES, no SECRETS

$REPO = "garcia4014/RENDICIONES_API"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Configuración de SECRETS" -ForegroundColor Cyan
Write-Host "Proyecto: ContabilidadAPI" -ForegroundColor White
Write-Host "Repositorio: $REPO" -ForegroundColor Yellow
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "ℹ️  INFORMACIÓN IMPORTANTE" -ForegroundColor Yellow
Write-Host ""
Write-Host "Para el proyecto ContabilidadAPI, TODAS las configuraciones" -ForegroundColor White
Write-Host "se manejan como VARIABLES (no Secrets)." -ForegroundColor White
Write-Host ""
Write-Host "Esto incluye:" -ForegroundColor White
Write-Host "  • Connection Strings de bases de datos" -ForegroundColor Gray
Write-Host "  • Claves JWT" -ForegroundColor Gray
Write-Host "  • Credenciales de SUNAT" -ForegroundColor Gray
Write-Host "  • API Key de Azure Document Intelligence" -ForegroundColor Gray
Write-Host ""
Write-Host "Razón: Facilita el debugging y mantenimiento del deployment." -ForegroundColor Cyan
Write-Host ""
Write-Host "Si prefieres mayor seguridad, puedes convertir las siguientes" -ForegroundColor Yellow
Write-Host "VARIABLES a SECRETS:" -ForegroundColor Yellow
Write-Host "  • CONN_BDSVRENDICIONES" -ForegroundColor Gray
Write-Host "  • CONN_BDMARCACIONES" -ForegroundColor Gray
Write-Host "  • CONN_BDGNRLMOVITECNICA" -ForegroundColor Gray
Write-Host "  • JWT_KEY" -ForegroundColor Gray
Write-Host "  • SUNAT_CLIENT_SECRET" -ForegroundColor Gray
Write-Host "  • AZURE_DOC_INTELLIGENCE_KEY" -ForegroundColor Gray
Write-Host ""
Write-Host "Y modificar deploy.yaml de:" -ForegroundColor Yellow
Write-Host "  `${{ vars.VARIABLE_NAME }}" -ForegroundColor Gray
Write-Host "A:" -ForegroundColor Yellow
Write-Host "  `${{ secrets.SECRET_NAME }}" -ForegroundColor Gray
Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Para configurar las variables, ejecuta:" -ForegroundColor Yellow
Write-Host "  .\setup-github-variables.ps1" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
