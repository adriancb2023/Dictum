using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Expediente
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string NumExpediente { get; set; } = null!;

    public string Juzgado { get; set; } = null!;

    public string Jurisdiccion { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public string Observaciones { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
