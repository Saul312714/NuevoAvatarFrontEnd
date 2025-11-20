using System.Collections.Generic;

namespace NuevoAvatarFront.Services
{
    public class MockDirecciones : IMockDirecciones
    {
        private readonly Dictionary<string, Dictionary<string, List<string>>> _datos;

        public MockDirecciones()
        {
            _datos = new Dictionary<string, Dictionary<string, List<string>>>
            {
                {
                    "San José",
                    new Dictionary<string, List<string>>
                    {
                        { "Central", new List<string> { "Carmen", "Merced", "Hospital", "Catedral" } },
                        { "Escazú", new List<string> { "Escazú Centro", "San Rafael", "San Antonio" } }
                    }
                },
                {
                    "Alajuela",
                    new Dictionary<string, List<string>>
                    {
                        { "Central", new List<string> { "Alajuela Centro", "San José", "Carrizal" } },
                        { "San Ramón", new List<string> { "San Ramón Centro", "Santiago", "Piedades Norte" } }
                    }
                },
                {
                    "Cartago",
                    new Dictionary<string, List<string>>
                    {
                        { "Cartago", new List<string> { "Oriental", "Occidental", "Carmen", "San Nicolás", "Agua Caliente" } },
                        { "Paraíso", new List<string> { "Paraíso Centro", "Orosi", "Cachí", "Llanos de Santa Lucía" } },
                        { "La Unión", new List<string> { "Tres Ríos", "San Diego", "San Juan", "Concepción", "Dulce Nombre" } },
                        { "Oreamuno", new List<string> { "San Rafael", "Cot", "Potrero Cerrado", "Santa Rosa" } },
                        { "El Guarco", new List<string> { "Tejar", "San Isidro", "Tobosi", "Patio de Agua" } }
                    }
                }
            };
        }

        public List<string> ObtenerProvincias()
        {
            return new List<string>(_datos.Keys);
        }

        public List<string> ObtenerCantones(string provincia)
        {
            if (_datos.ContainsKey(provincia))
                return new List<string>(_datos[provincia].Keys);

            return new List<string>();
        }

        public List<string> ObtenerDistritos(string provincia, string canton)
        {
            if (_datos.ContainsKey(provincia) &&
                _datos[provincia].ContainsKey(canton))
            {
                return _datos[provincia][canton];
            }

            return new List<string>();
        }
    }
}
