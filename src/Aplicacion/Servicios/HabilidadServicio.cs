using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

public class HabilidadServicio : IHabilidadServicio
{
    private readonly IHabilidadRepositorio _repositorio;

    public HabilidadServicio(IHabilidadRepositorio repositorio)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        _repositorio = repositorio;
    }

    public void Registrar(Habilidad habilidad)
    {
        ArgumentNullException.ThrowIfNull(habilidad);
        _repositorio.Agregar(habilidad);
    }

    public IReadOnlyCollection<Habilidad> ObtenerTodas()
    {
        return _repositorio.ObtenerTodos();
    }

    public Habilidad? ObtenerPorMovimiento(string movimiento)
    {
        return _repositorio.ObtenerPorMovimiento(movimiento);
    }
}
