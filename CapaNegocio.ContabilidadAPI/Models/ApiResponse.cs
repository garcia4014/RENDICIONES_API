namespace CapaNegocio.ContabilidadAPI.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse(T data, string message = null)
        {
            Success = true;
            Message = message ?? "Operación exitosa";
            Data = data;
            Errors = null;
        }

        public ApiResponse(string errorMessage)
        {
            Success = false;
            Message = "Ocurrió un error";
            Data = default;
            Errors = new List<string> { errorMessage };
        }

        public ApiResponse(List<string> errors)
        {
            Success = false;
            Message = "Ocurrió un error";
            Data = default;
            Errors = errors;
        }
    }

}
