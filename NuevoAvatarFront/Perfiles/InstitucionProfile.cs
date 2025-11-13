using AutoMapper;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Entities;

public class InstitucionProfile : Profile
{
    public InstitucionProfile()
    {
        // Mapea la entidad 'Institucion' a 'InstitucionDto'
        CreateMap<Institucion, InstitucionDto>();  // Mapea 'Institucion' -> 'InstitucionDto'
    }

}