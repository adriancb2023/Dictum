using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Casosetiqueta
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public int IdEtiqueta { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;

    public virtual Estado IdEtiquetaNavigation { get; set; } = null!;
}
