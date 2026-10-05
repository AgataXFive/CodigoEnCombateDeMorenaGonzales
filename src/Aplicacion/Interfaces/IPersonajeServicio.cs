using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

public interface IPersonajeServicio
{
    void Registrar(Personaje personaje);
    IReadOnlyCollection<Personaje> ObtenerTodos();
    Personaje? ObtenerPorNombre(string nombre);
    IReadOnlyCollection<Personaje> ObtenerPorTipo(string tipo);
}
