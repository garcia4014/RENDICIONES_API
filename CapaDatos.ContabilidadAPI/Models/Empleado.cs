using System;
using System.Collections.Generic;

namespace CapaDatos.ContabilidadAPI.Models;

public partial class Empleado
{
    public string EmpCodigo { get; set; } = null!;

    public string? EmpDni { get; set; }

    public string? EmpNombres { get; set; }

    public string? EmpTelefono { get; set; }

    public string? EmpCorreo { get; set; }

    public string? EmpUnidadNegocio { get; set; }

    public string? EmpCentroCosto { get; set; }
}
