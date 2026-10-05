using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

public class PersonajeServicio : IPersonajeServicio
{
    private readonly IPersonajeRepositorio _repositorio;

    public PersonajeServicio(IPersonajeRepositorio repositorio)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        _repositorio = repositorio;
    }

    public void Registrar(Personaje personaje)
    {
        ArgumentNullException.ThrowIfNull(personaje);
        _repositorio.Agregar(personaje);
    }

    public IReadOnlyCollection<Personaje> ObtenerTodos()
    {
        return _repositorio.ObtenerTodos();
    }

    public Personaje? ObtenerPorNombre(string nombre)
    {
        return _repositorio.ObtenerPorNombre(nombre);
    }

    public IReadOnlyCollection<Personaje> ObtenerPorTipo(string tipo)
    {
        return _repositorio.ObtenerPorTipo(tipo);
    }
}
