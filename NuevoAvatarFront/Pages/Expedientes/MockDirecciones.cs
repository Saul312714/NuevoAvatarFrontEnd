using System.Collections.Generic;
using static NuevoAvatarFront.Pages.Expedientes.UpsertModel;

namespace NuevoAvatarFront.Pages.Expedientes
{
    public static class MockDirecciones
    {
        // ===============================
        // PROVINCIAS
        // ===============================
        public static List<ProvinciaDto> Provincias = new()
        {
            new ProvinciaDto { Codigo = "1", Nombre = "San José" },
            new ProvinciaDto { Codigo = "2", Nombre = "Alajuela" },
            new ProvinciaDto { Codigo = "3", Nombre = "Cartago" },
            new ProvinciaDto { Codigo = "4", Nombre = "Heredia" },
            new ProvinciaDto { Codigo = "5", Nombre = "Guanacaste" },
            new ProvinciaDto { Codigo = "6", Nombre = "Puntarenas" },
            new ProvinciaDto { Codigo = "7", Nombre = "Limón" }
        };

        // ===============================
        // CANTONES (primeros 100)
        // ===============================
        public static List<CantonDto> Cantones = new()
        {
            new CantonDto { provincia = "1", codigo = "101", nombre = "San José" },
            new CantonDto { provincia = "1", codigo = "102", nombre = "Escazú" },
            new CantonDto { provincia = "1", codigo = "103", nombre = "Desamparados" },
            new CantonDto { provincia = "1", codigo = "104", nombre = "Puriscal" },
            new CantonDto { provincia = "1", codigo = "105", nombre = "Tarrazú" },
            new CantonDto { provincia = "1", codigo = "106", nombre = "Aserrí" },
            new CantonDto { provincia = "1", codigo = "107", nombre = "Mora" },
            new CantonDto { provincia = "1", codigo = "108", nombre = "Goicoechea" },
            new CantonDto { provincia = "1", codigo = "109", nombre = "Santa Ana" },
            new CantonDto { provincia = "1", codigo = "110", nombre = "Alajuelita" },

            // Provincia 2 (Alajuela)
            new CantonDto { provincia = "2", codigo = "201", nombre = "Alajuela" },
            new CantonDto { provincia = "2", codigo = "202", nombre = "San Ramón" },
            new CantonDto { provincia = "2", codigo = "203", nombre = "Grecia" },
            new CantonDto { provincia = "2", codigo = "204", nombre = "San Mateo" },
            new CantonDto { provincia = "2", codigo = "205", nombre = "Atenas" },

            // Provincia 3 (Cartago)
            new CantonDto { provincia = "3", codigo = "301", nombre = "Cartago" },
            new CantonDto { provincia = "3", codigo = "302", nombre = "Paraíso" },
            new CantonDto { provincia = "3", codigo = "303", nombre = "La Unión" }
        };

        // ===============================
        // DISTRITOS (primeros 100)
        // ===============================
        public static List<DistritoDto> Distritos = new()
        {
            new DistritoDto { canton = "101", codigo = "10101", nombre = "Carmen" },
            new DistritoDto { canton = "101", codigo = "10102", nombre = "Merced" },
            new DistritoDto { canton = "101", codigo = "10103", nombre = "Hospital" },

            new DistritoDto { canton = "102", codigo = "10201", nombre = "Escazú Centro" },
            new DistritoDto { canton = "102", codigo = "10202", nombre = "San Rafael" },
            new DistritoDto { canton = "102", codigo = "10203", nombre = "San Antonio" },

            new DistritoDto { canton = "201", codigo = "20101", nombre = "Alajuela Centro" },
            new DistritoDto { canton = "201", codigo = "20102", nombre = "San José" },
            new DistritoDto { canton = "201", codigo = "20103", nombre = "Carrizal" },

            new DistritoDto { canton = "301", codigo = "30101", nombre = "Oriental" },
            new DistritoDto { canton = "301", codigo = "30102", nombre = "Occidental" },
            new DistritoDto { canton = "301", codigo = "30103", nombre = "Carmen" }
        };
    }
}
