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
dotnet user-secrets set "AzureDocumentIntelligence:Endpoint" "https://your-resource-name.cognitiveservices.azure.com"
dotnet user-secrets set "AzureDocumentIntelligence:SubscriptionKey" "YOUR_AZURE_KEY_HERE"
```

## Configuración en Producción (IIS/Azure)

### Opción 1: Variables de Entorno en web.config (Recomendado para IIS)

La forma más confiable en IIS es configurar las variables directamente en el `web.config`:

1. Abre `C:\inetpub\wwwroot\API_CONTABILIDAD\web.config`
2. Dentro de la sección `<aspNetCore>`, agrega las variables de entorno:

```xml
<aspNetCore processPath="dotnet" arguments=".\ContabilidadAPI.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="AzureDocumentIntelligence__Endpoint" value="https://pruebaiadoc.cognitiveservices.azure.com" />
    <environmentVariable name="AzureDocumentIntelligence__SubscriptionKey" value="TU_CLAVE_AQUI" />
  </environmentVariables>
</aspNetCore>
```

3. Guarda el archivo
4. **NO NECESITAS reiniciar IIS**, el cambio se aplica automáticamente

Nota: Usa doble guion bajo `__` para separar secciones en las variables de entorno.

### Opción 2: Variables de Entorno del Sistema (Requiere reinicio)

Si prefieres usar variables del sistema:

1. Panel de Control → Sistema → Configuración avanzada del sistema → Variables de entorno
2. Agrega las siguientes variables del sistema:
   - `AzureDocumentIntelligence__Endpoint` = `https://pruebaiadoc.cognitiveservices.azure.com`
   - `AzureDocumentIntelligence__SubscriptionKey` = `TU_CLAVE_AQUI`

3. **IMPORTANTE**: Después de agregar las variables, debes:
   - Abrir IIS Manager
   - Ir a Application Pools
   - Seleccionar tu Application Pool (ej: API_CONTABILIDAD)
   - Click derecho → Recycle
   - O ejecutar en cmd como administrador:
     ```cmd
     %windir%\system32\inetsrv\appcmd recycle apppool /apppool.name:"API_CONTABILIDAD"
     ```

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
