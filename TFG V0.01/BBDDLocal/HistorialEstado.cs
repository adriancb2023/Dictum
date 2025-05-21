using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class HistorialEstado
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public int IdEstado { get; set; }

    public DateOnly FechaCambio { get; set; }

    public string Observaciones { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;
}
