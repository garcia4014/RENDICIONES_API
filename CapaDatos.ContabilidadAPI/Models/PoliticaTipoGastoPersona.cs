using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CapaDatos.ContabilidadAPI.Models;

public partial class PoliticaTipoGastoPersona
{
    public int PtgpId { get; set; }

    public int? PtgpIdTg { get; set; }

    public int? PtgpIdTp { get; set; }
    public decimal PtgpMonto { get; set; }

    public bool? PtgpEstado { get; set; }

 
    public virtual TipoGasto? PtgpIdTgNavigation { get; set; }
    public virtual TipoPersona? PtgpIdTpNavigation { get; set; }
}
