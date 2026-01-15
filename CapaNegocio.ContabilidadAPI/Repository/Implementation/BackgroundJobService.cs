using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio para ejecutar tareas en background sin Hangfire
    /// Usa una cola en memoria con procesamiento asíncrono
    /// </summary>
    public class BackgroundJobService : IBackgroundJobService
    {
        private static readonly ConcurrentQueue<int> _validacionQueue = new();
        private static bool _isProcessing = false;
        private static readonly object _lock = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundJobService> _logger;

        public BackgroundJobService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Encola una validación de comprobante SUNAT para ejecutar en background
        /// </summary>
        public void EnqueueValidacionSunat(int comprobanteId)
        {
            _validacionQueue.Enqueue(comprobanteId);
            _logger.LogInformation("Comprobante {ComprobanteId} encolado para validación SUNAT", comprobanteId);

            // Iniciar procesamiento si no está corriendo
            lock (_lock)
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;
                    Task.Run(() => ProcessQueueAsync());
                }
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (_validacionQueue.TryDequeue(out int comprobanteId))
            {
                try
                {
                    _logger.LogInformation("Procesando validación SUNAT para comprobante {ComprobanteId}", comprobanteId);
                    await ValidarComprobanteEnSunatAsync(comprobanteId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar validación SUNAT para comprobante {ComprobanteId}", comprobanteId);
                }

                // Pequeña pausa entre validaciones para no saturar SUNAT
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            lock (_lock)
            {
                _isProcessing = false;
            }
        }

        private async Task ValidarComprobanteEnSunatAsync(int comprobanteId)
        {
            using var scope = _serviceProvider.CreateScope();
            var comprobanteDao = scope.ServiceProvider.GetRequiredService<IComprobantePago>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ISunatTokenService>();
            var comprobanteService = scope.ServiceProvider.GetRequiredService<ISunatComprobanteService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BackgroundJobService>>();

            try
            {
                // Obtener el comprobante
                var comprobante = await comprobanteDao.GetByIdAsync(comprobanteId);
                
                if (comprobante == null)
                {
                    logger.LogWarning("No se encontró el comprobante {ComprobanteId} para validar", comprobanteId);
                    return;
                }

                if (!comprobante.Activo)
                {
                    logger.LogWarning("El comprobante {ComprobanteId} no está activo", comprobanteId);
                    return;
                }

                // Obtener configuración SUNAT
                var sunatConfig = configuration.GetSection("SunatConfiguration").Get<SunatConfigurationDto>();
                if (sunatConfig == null)
                {
                    logger.LogError("No se encontró la configuración de SUNAT");
                    return;
                }

                // Obtener token de SUNAT
                logger.LogInformation("Obteniendo token de SUNAT para comprobante {ComprobanteId}", comprobanteId);
                var tokenResponse = await tokenService.ObtenerTokenAsync(sunatConfig.ClientId, sunatConfig.ClientSecret);
                
                if (!tokenResponse.Success || tokenResponse.Data == null)
                {
                    logger.LogError("Error al obtener token de SUNAT: {Error}", tokenResponse.Message);
                    return;
                }

                var token = tokenResponse.Data.access_token;

                // Preparar request de validación
                var request = new SunatComprobanteRequestDto
                {
                    numRuc = comprobante.Ruc?.ToString() ?? string.Empty,
                    codComp = comprobante.TipoComprobante?.PadLeft(2, '0') ?? "01",
                    numeroSerie = comprobante.Serie ?? string.Empty,
                    numero = comprobante.Correlativo ?? string.Empty,
                    fechaEmision = comprobante.FechaEmision?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"),
                    monto = comprobante.Monto?.ToString("F2") ?? "0.00"
                };

                logger.LogInformation("Validando en SUNAT - RUC: {RUC}, Serie: {Serie}, Número: {Numero}", 
                    request.numRuc, request.numeroSerie, request.numero);

                // Validar en SUNAT
                var result = await comprobanteService.ValidarComprobanteAsync(sunatConfig.RUC, token, request);

                if (result.Success)
                {
                    logger.LogInformation("Comprobante {ComprobanteId} validado exitosamente en SUNAT", comprobanteId);
                    
                    // Actualizar comprobante con resultado
                    comprobante.ValidoSunat = true;
                    comprobante.ResultadoSunat = JsonSerializer.Serialize(result);
                    await comprobanteDao.UpdateAsync(comprobante);
                }
                else
                {
                    logger.LogWarning("Validación SUNAT falló para comprobante {ComprobanteId}: {Message}", 
                        comprobanteId, result.Message);
                    
                    comprobante.ValidoSunat = false;
                    comprobante.ResultadoSunat = JsonSerializer.Serialize(result);
                    await comprobanteDao.UpdateAsync(comprobante);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al validar comprobante {ComprobanteId} en SUNAT", comprobanteId);
            }
        }
    }
}
