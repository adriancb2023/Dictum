# Dictum

<div align="center">

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-512BD4?style=for-the-badge&logo=windows&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=.net&logoColor=white)

**Solución integral de gestión y documentación jurídica para despachos modernos.**

</div>

---

## 📋 Descripción

**Dictum** es una suite tecnológica diseñada para optimizar el flujo de trabajo en despachos jurídicos en España. Combina un potente **ERP de escritorio** para la gestión interna con una **API especializada** para la recuperación automática de jurisprudencia desde CENDOJ.

El sistema permite a los abogados centrarse en sus casos, automatizando la organización de clientes, expedientes y la búsqueda de documentación legal.

## 🚀 Componentes del Ecosistema

### 1. ERP de Escritorio (`TFG V0.01`)
El núcleo de gestión para el día a día del despacho.
- **👥 Gestión de Clientes**: Base de datos centralizada con alta, edición y consulta rápida.
- **📂 Expedientes Jurídicos**: Organización detallada y seguimiento de casos.
- **📄 Gestión Documental**: Almacenamiento seguro de documentos asociados a cada caso.
- **📝 Notas y Tareas**: Sistema interno para notas rápidas y control de tareas pendientes.
- **🔍 Buscador Integrado**: Conexión directa con la API para encontrar jurisprudencia sin salir de la aplicación.
- **🎨 UI Adaptable**: Interfaz moderna con soporte nativo para temas Claro/Oscuro y multi-idioma.

### 2. Jurisprudencia API (`JurisprudenciaAPI`)
El motor de búsqueda avanzado.
- **🤖 Automatización**: Búsqueda automática de sentencias y resoluciones en CENDOJ.
- **🔌 Integración**: Datos estructurados listos para ser consumidos por el ERP.
- **⚡ Rendimiento**: Optimizado para consultas rápidas y precisas.

## 🛠️ Stack Tecnológico

Proyecto construido con las últimas tecnologías del ecosistema .NET:

*   **Core**: .NET 9 y C# 13
*   **Desktop**: Windows Presentation Foundation (WPF)
*   **Backend**: ASP.NET Core Web API
*   **Arquitectura**: Limpia y modular

## 💻 Instalación y Uso

Sigue estos pasos para desplegar el entorno de desarrollo:

1.  **Clonar el repositorio**
    ```bash
    git clone https://github.com/tu-usuario/Dictum.git
    cd Dictum
    ```

2.  **Preparar el entorno**
    *   Abre la solución `TFG V0.01.sln` en Visual Studio 2022 (o superior).
    *   Restaura los paquetes NuGet para todos los proyectos.

3.  **Ejecutar**
    *   Inicia primero el proyecto de backend: `JurisprudenciaAPI`.
    *   Una vez la API esté activa, inicia el cliente de escritorio: `TFG V0.01` (o el ejecutable generado).

## ⚠️ Nota Importante
> Este software utiliza técnicas de *scraping* para obtener datos de fuentes públicas (CENDOJ). Su uso debe ser responsable y respetar los términos de servicio de los proveedores de datos. Diseñado exclusivamente para uso interno y educativo.

## 📄 Licencia
Este proyecto es **PRIVADO**. Todos los derechos reservados.

## 👥 Autores

Desarrollado por el equipo de **Kybernatech**:

*   **Adrian Cruz Barranco** - [GitHub](https://github.com/adriancb2023) - 📧 [Email](mailto:a.cruz@kybernatech.com)
*   **Francisco Jose Navarro Ruiz** - [GitHub](https://github.com/KatEston23) - 📧 [Email](mailto:f.navarro@kybernatech.com)
