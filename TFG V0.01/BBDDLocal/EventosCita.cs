using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class EventosCita
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdEstado { get; set; }

    public DateOnly Fecha { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;
}
