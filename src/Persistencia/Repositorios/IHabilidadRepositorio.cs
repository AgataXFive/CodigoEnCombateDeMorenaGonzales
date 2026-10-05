using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public interface IHabilidadRepositorio : IRepositorio<Habilidad>
{
    Habilidad? ObtenerPorMovimiento(string movimiento);
}
