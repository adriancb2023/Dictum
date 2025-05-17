using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Tarea
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
