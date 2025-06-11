using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace JurisprudenciaApi.Pages
{
    public class JurisprudenciaSearchModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public SearchInputModel Input { get; set; } = new SearchInputModel();

        public void OnGet()
        {
            // Initial page load logic (if any)
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Logic to call the API endpoint with Input data
            // For now, just returning the page
            Console.WriteLine($"Search Text: {Input.TextoLibre}");
            Console.WriteLine($"Jurisdiccion: {Input.Jurisdiccion}");
            // ... log other properties

            // Redirect or display results after processing
            return Page();
        }
    }

    // Basic model to hold form data
    // Add more properties corresponding to your form fields
    public class SearchInputModel
    {
        [Display(Name = "Texto libre")]
        public string? TextoLibre { get; set; }

        [Display(Name = "Jurisdicción")]
        public string? Jurisdiccion { get; set; } // Simple string for now, could be List<string> for multi-select

        [Display(Name = "Tipo res.")]
        public string? TipoResolucion { get; set; }

        [Display(Name = "Tipo de órgano")]
        public string? TipoOrgano { get; set; }

        [Display(Name = "Localización")]
        public List<string>? Localizacion { get; set; } = new List<string>();

        [Display(Name = "Provincias")]
        public List<string>? Provincias { get; set; } = new List<string>();

        [Display(Name = "Nº ROJ")]
        public string? Roj { get; set; }

        [Display(Name = "ECLI")]
        public string? Ecli { get; set; }

        [Display(Name = "Fecha resolución Desde")]
        [DataType(DataType.Date)]
        public DateTime? FechaResolucionDesde { get; set; }

        [Display(Name = "Fecha resolución Hasta")]
        [DataType(DataType.Date)]
        public DateTime? FechaResolucionHasta { get; set; }

        [Display(Name = "Nº Resolución")]
        public string? NumeroResolucion { get; set; }

        [Display(Name = "Nº Recurso")]
        public string? NumeroRecurso { get; set; }

        [Display(Name = "Ponente")]
        public string? Ponente { get; set; }

        [Display(Name = "Idioma")]
        public string? Idioma { get; set; }

         // Add other fields as needed based on the HTML form
    }
} 