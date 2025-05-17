using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Notificacione
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateOnly FechaVencimiento { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
