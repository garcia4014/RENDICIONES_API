namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para la solicitud de token a SUNAT
    /// </summary>
    public class SunatTokenRequestDto
    {
        public string grant_type { get; set; } = "client_credentials";
        public string scope { get; set; } = "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes";
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }
}