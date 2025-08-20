# Dictum

Dictum es una solución compuesta por un ERP de escritorio y una API, diseñada específicamente para abogados y despachos jurídicos en España.

## Componentes del Proyecto

### 1. ERP de Escritorio (`TFG V0.01`)
- **Gestión de clientes**: Alta, edición y consulta de clientes.
- **Gestión de casos**: Organización y seguimiento de casos jurídicos.
- **Búsqueda de documentación**: Integración con la API propia para buscar documentación jurídica relevante en CENDOJ.
- **Gestión documental**: Almacenamiento y organización de documentos asociados a clientes y casos.
- **Notas y tareas**: Registro de notas internas y gestión de tareas pendientes.
- **Multi-idioma**: Interfaz disponible en varios idiomas.
- **Temas claro/oscuro**: Adaptación visual según preferencia o sistema.

### 2. API de Jurisprudencia (`JurisprudenciaAPI`)
- **Búsqueda en CENDOJ**: Permite realizar búsquedas automáticas de documentación jurídica en la web oficial de CENDOJ (Centro de Documentación Judicial de España).
- **Exposición de resultados**: Devuelve la información relevante para su integración en el ERP.

## Tecnologías
- **.NET 9**
- **C# 13**
- **WPF** para la interfaz de usuario del ERP
- **ASP.NET Core** para la API

## Instalación y Uso
1. Clona el repositorio.
2. Abre la solución en Visual Studio 2022 o superior.
3. Restaura los paquetes NuGet.
4. Compila ambos proyectos.
5. Ejecuta primero la API (`JurisprudenciaAPI`) y luego el ERP (`TFG V0.01`).

## Notas
- El acceso a la documentación de CENDOJ se realiza mediante técnicas de scraping, respetando los términos de uso del sitio web.
- El ERP está pensado para uso interno en despachos jurídicos.

## Licencia
Este proyecto es de uso PRIVADO.

## Desarrollado por:
- **[Adrian Cruz Barranco]** - [a.cruz@kybernatech.com] - [https://github.com/adriancb2023]
- **[Francisco Jose Navarro Ruiz]** - [f.navarro@kybernatech.com] - [https://github.com/KatEston23]
