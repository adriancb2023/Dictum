using System;
using System.Collections.Generic;

namespace JurisprudenciaApi.Models
{
    public class JurisprudenciaSearchParameters
    {
        public string? TextoLibre { get; set; }
        public string? Jurisdiccion { get; set; } // Ejemplo: "Civil", "Penal"
        public List<string>? TiposResolucion { get; set; }
        public List<string>? OrganosJudiciales { get; set; }
        public List<string>? Localizaciones { get; set; }
        public string? Seccion { get; set; }
        public string? Idioma { get; set; }
        public string? Legislacion { get; set; }
        public string? Roj { get; set; } // Número ROJ
        public string? Ecli { get; set; } // Identificador ECLI
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? NumeroResolucion { get; set; }
        public string? NumeroRecurso { get; set; }
        public string? Ponente { get; set; }

        // Nuevas propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10; // Default 10, como lo hace CENDOJ

        // Añadir más propiedades según sea necesario para cubrir todos los campos del formulario CENDOJ
    }
} 