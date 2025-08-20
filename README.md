# Dictum

Dictum es una soluci�n compuesta por un ERP de escritorio y una API, dise�ada espec�ficamente para abogados y despachos jur�dicos en Espa�a.

## Componentes del Proyecto

### 1. ERP de Escritorio (`TFG V0.01`)
- **Gesti�n de clientes**: Alta, edici�n y consulta de clientes.
- **Gesti�n de casos**: Organizaci�n y seguimiento de casos jur�dicos.
- **B�squeda de documentaci�n**: Integraci�n con la API propia para buscar documentaci�n jur�dica relevante en CENDOJ.
- **Gesti�n documental**: Almacenamiento y organizaci�n de documentos asociados a clientes y casos.
- **Notas y tareas**: Registro de notas internas y gesti�n de tareas pendientes.
- **Multi-idioma**: Interfaz disponible en varios idiomas.
- **Temas claro/oscuro**: Adaptaci�n visual seg�n preferencia o sistema.

### 2. API de Jurisprudencia (`JurisprudenciaAPI`)
- **B�squeda en CENDOJ**: Permite realizar b�squedas autom�ticas de documentaci�n jur�dica en la web oficial de CENDOJ (Centro de Documentaci�n Judicial de Espa�a).
- **Exposici�n de resultados**: Devuelve la informaci�n relevante para su integraci�n en el ERP.

## Tecnolog�as
- **.NET 9**
- **C# 13**
- **WPF** para la interfaz de usuario del ERP
- **ASP.NET Core** para la API

## Instalaci�n y Uso
1. Clona el repositorio.
2. Abre la soluci�n en Visual Studio 2022 o superior.
3. Restaura los paquetes NuGet.
4. Compila ambos proyectos.
5. Ejecuta primero la API (`JurisprudenciaAPI`) y luego el ERP (`TFG V0.01`).

## Notas
- El acceso a la documentaci�n de CENDOJ se realiza mediante t�cnicas de scraping, respetando los t�rminos de uso del sitio web.
- El ERP est� pensado para uso interno en despachos jur�dicos.

## Licencia
Este proyecto es de uso PRIVADO.

## Desarrollado por:
- **[Adrian Cruz Barranco]** - [a.cruz@kybernatech.com] - [https://github.com/adriancb2023]
- **[Francisco Jose Navarro Ruiz]** - [f.navarro@kybernatech.com] - [https://github.com/KatEston23]
