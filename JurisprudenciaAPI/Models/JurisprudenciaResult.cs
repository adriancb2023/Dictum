namespace JurisprudenciaApi.Models
{
    public class JurisprudenciaResult
    {
        public string? Roj { get; set; }
        public string? Ecli { get; set; }
        public string? FechaResolucion { get; set; } // Or DateTime?
        public string? TipoResolucion { get; set; }
        public string? OrganoJudicial { get; set; }
        public string? Ponente { get; set; }
        public string? Resumen { get; set; } // Or snippet
        public string? UrlDocumento { get; set; } // Link to PDF/details
        // Add any other relevant fields extracted from the search results page
    }
} 