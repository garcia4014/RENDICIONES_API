using System;
using System.Collections.Generic;

namespace CapaDatos.ContabilidadAPI.Models;

public partial class TipoGasto
{
    public int TgId { get; set; }

    public string? TgDescripcion { get; set; }

    public decimal? TgPrecioUMax { get; set; }

    public bool? TgEstado { get; set; }

    public virtual ICollection<PoliticaTipoGastoPersona> PoliticaTipoGastoPersonas { get; set; } = new List<PoliticaTipoGastoPersona>();
}
