# Configuración de Azure Document Intelligence

## ✅ Solución Implementada

Las claves de Azure ahora se gestionan mediante **User Secrets** (desarrollo) y variables de entorno (producción), nunca en los archivos de configuración que se commitean a Git.

## Configuración Actual

### Desarrollo Local (Ya configurado)

El proyecto ya tiene configurado User Secrets con tus claves reales:

```bash
# Ver las claves configuradas
dotnet user-secrets list --project ContabilidadAPI

# Resultado:
# AzureDocumentIntelligence:Endpoint = https://pruebaiadoc.cognitiveservices.azure.com
# AzureDocumentIntelligence:SubscriptionKey = [tu clave]
```

✅ **El proyecto funciona exactamente igual que antes**, pero las claves están seguras fuera del repositorio.

Los User Secrets se almacenan en:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\rendiciones-api-secrets-2025\secrets.json`
- **Linux/Mac**: `~/.microsoft/usersecrets/rendiciones-api-secrets-2025/secrets.json`

### Para otros desarrolladores

Si otro desarrollador clona el repositorio, debe configurar sus propias claves:

```bash
cd ContabilidadAPI
dotnet user-secrets set "AzureDocumentIntelligence:Endpoint" "https://pruebaiadoc.cognitiveservices.azure.com"
dotnet user-secrets set "AzureDocumentIntelligence:SubscriptionKey" "LA_CLAVE_REAL_AQUI"
```

## Configuración en Producción (IIS/Azure)

### Opción 1: Variables de Entorno del Sistema (Recomendado para IIS)

En el servidor de producción, configura las variables de entorno:

1. Panel de Control → Sistema → Configuración avanzada del sistema → Variables de entorno
2. Agrega las siguientes variables del sistema:
   - `AzureDocumentIntelligence__Endpoint` = `https://pruebaiadoc.cognitiveservices.azure.com`
   - `AzureDocumentIntelligence__SubscriptionKey` = `tu_clave_real`

Nota: Usa doble guion bajo `__` para separar secciones en las variables de entorno.

### Opción 2: Azure Key Vault (Recomendado para Azure App Service)

1. Crea un Azure Key Vault
2. Agrega los secretos:
   - `AzureDocumentIntelligence--Endpoint`
   - `AzureDocumentIntelligence--SubscriptionKey`

3. Configura la Managed Identity en tu App Service
4. Agrega la configuración en `Program.cs`:
```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVaultUrl"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}
```

## Resolver el Bloqueo de GitHub

Ahora que las claves han sido removidas:

1. Commit los cambios:
```bash
git add .
git commit -m "Remove hardcoded Azure keys, use environment variables"
```

2. Revoca la clave antigua en Azure Portal:
   - Ve a tu recurso de Azure Document Intelligence
   - Regenera las claves (Keys and Endpoint)
   - Actualiza tus configuraciones locales con la nueva clave

3. Push al repositorio:
```bash
git push origin master
```

## Archivos que NO deben tener claves reales

❌ `appsettings.json` - Valores por defecto/placeholders
❌ `appsettings.Development.json` - Valores por defecto/placeholders
❌ `appsettings.Production.json` - Valores por defecto/placeholders

✅ `.env` - Local, en .gitignore
✅ User Secrets - Almacenados fuera del proyecto
✅ Variables de entorno del servidor
✅ Azure Key Vault
