using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Contacto
{
    public int Id { get; set; }

    public int IdCaso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Email { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
