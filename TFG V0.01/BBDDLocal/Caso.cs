using System;
using System.Collections.Generic;

namespace TFG_V0._01.BBDDLocal;

public partial class Caso
{
    public int Id { get; set; }

    public int IdCliente { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public int IdEstado { get; set; }

    public virtual ICollection<Alerta> Alerta { get; set; } = new List<Alerta>();

    public virtual ICollection<Casosetiqueta> Casosetiqueta { get; set; } = new List<Casosetiqueta>();

    public virtual ICollection<Casosrelacionado> CasosrelacionadoIdCasoNavigations { get; set; } = new List<Casosrelacionado>();

    public virtual ICollection<Casosrelacionado> CasosrelacionadoIdCasoRelacionadoNavigations { get; set; } = new List<Casosrelacionado>();

    public virtual ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual ICollection<EventosCita> EventosCita { get; set; } = new List<EventosCita>();

    public virtual ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual ICollection<Nota> Nota { get; set; } = new List<Nota>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Reciente> Recientes { get; set; } = new List<Reciente>();

    public virtual ICollection<Registrotiempo> Registrotiempos { get; set; } = new List<Registrotiempo>();

    public virtual ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
}
