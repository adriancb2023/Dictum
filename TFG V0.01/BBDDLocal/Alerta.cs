using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Alerta
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateOnly FechaAlerta { get; set; }

    public string EstadoAlerta { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
