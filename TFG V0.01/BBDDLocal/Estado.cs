using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Estado
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Caso> Casos { get; set; } = new List<Caso>();

    public virtual ICollection<Casosetiqueta> Casosetiqueta { get; set; } = new List<Casosetiqueta>();

    public virtual ICollection<EventosCita> EventosCita { get; set; } = new List<EventosCita>();

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();
}
