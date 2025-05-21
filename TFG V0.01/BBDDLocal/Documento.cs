using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Documento
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Ruta { get; set; } = null!;

    public DateOnly FechaSubid { get; set; }

    public int TipoDocumento { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;

    public virtual TipoDocumento TipoDocumentoNavigation { get; set; } = null!;
}
