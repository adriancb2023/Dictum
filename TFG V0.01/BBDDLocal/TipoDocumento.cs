using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class TipoDocumento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
