using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Registrotiempo
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string? Descripcion { get; set; }

    public decimal Horas { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
