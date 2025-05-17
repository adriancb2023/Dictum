using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Nota
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
