using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

public interface IHabilidadServicio
{
    void Registrar(Habilidad habilidad);
    IReadOnlyCollection<Habilidad> ObtenerTodas();
    Habilidad? ObtenerPorMovimiento(string movimiento);
}
