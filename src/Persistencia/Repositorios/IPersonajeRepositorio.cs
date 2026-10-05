using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public interface IPersonajeRepositorio : IRepositorio<Personaje>
{
    Personaje? ObtenerPorNombre(string nombre);
    IReadOnlyCollection<Personaje> ObtenerPorTipo(string tipo);
}
