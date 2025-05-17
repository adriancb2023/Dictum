using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido1 { get; set; } = null!;

    public string? Apellido2 { get; set; }

    public string Email1 { get; set; } = null!;

    public string? Email2 { get; set; }

    public string Telf1 { get; set; } = null!;

    public string? Telf2 { get; set; }

    public string Direccion { get; set; } = null!;

    public DateOnly FechaContrato { get; set; }

    public virtual ICollection<Caso> Casos { get; set; } = new List<Caso>();
}
