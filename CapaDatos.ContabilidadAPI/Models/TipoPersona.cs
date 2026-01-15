using System;
using System.Collections.Generic;

namespace CapaDatos.ContabilidadAPI.Models;

public partial class TipoPersona
{
    public int TpId { get; set; }

    public string TpDescripcion { get; set; } = null!;

    public string? TpDescripcionAbreviada { get; set; }

    public bool? TpEstado { get; set; }

    public virtual ICollection<PoliticaTipoGastoPersona> PoliticaTipoGastoPersonas { get; set; } = new List<PoliticaTipoGastoPersona>();

    public virtual ICollection<UsuarioTipoPersona> UsuarioTipoPersonas { get; set; } = new List<UsuarioTipoPersona>();
}
