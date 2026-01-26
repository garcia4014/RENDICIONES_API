namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class ActualizarTokenRequestDto
    {
        public string Valor { get; set; } = string.Empty;
    }

    public class ActualizarTokenResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
