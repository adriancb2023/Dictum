namespace TFG_V0._01.Models
{
    public class JurisprudenciaSearchParameters
    {
        public string? TextoLibre { get; set; }
        public string? Jurisdiccion { get; set; }
        public List<string>? TiposResolucion { get; set; }
        public List<string>? OrganosJudiciales { get; set; }
        public List<string>? ComunidadesAutonomas { get; set; }
        public List<string>? Provincias { get; set; }
        public string? Seccion { get; set; }
        public string? Idioma { get; set; }
        public string? Legislacion { get; set; }
        public string? Roj { get; set; }
        public string? Ecli { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? NumeroResolucion { get; set; }
        public string? NumeroRecurso { get; set; }
        public string? Ponente { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
    }
} 