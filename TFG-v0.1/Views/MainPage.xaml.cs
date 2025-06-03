private void CargarIdioma(int idioma)
{
    var idiomas = new (string Inicio, string Buscar, string Agenda, string Casos, string Clientes, string Documentos, string Ajustes, string Gpt)[]
    {
        ("Inicio", "Buscar", "Agenda", "Casos", "Clientes", "Documentos", "Ajustes", "GPT"),
        ("Home", "Search", "Calendar", "Cases", "Clients", "Documents", "Settings", "GPT"),
        ("Inici", "Cercar", "Agenda", "Casos", "Clients", "Documents", "Ajustos", "GPT"),
        ("Inicio", "Buscar", "Axenda", "Casos", "Clientes", "Documentos", "Axustes", "GPT"),
        ("Hasiera", "Bilatu", "Agenda", "Kasuak", "Bezeroak", "Dokumentuak", "Ezarpenak", "GPT"),
        ("GPT", "GPT", "GPT", "GPT", "GPT", "GPT", "GPT", "GPT")
    };
} 