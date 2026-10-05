using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public class HabilidadRepositorio : RepositorioBase<Habilidad>, IHabilidadRepositorio
{
    public Habilidad? ObtenerPorMovimiento(string movimiento)
    {
        if (string.IsNullOrWhiteSpace(movimiento))
            throw new ArgumentException("El movimiento es obligatorio.", nameof(movimiento));

        return Entidades.FirstOrDefault(h =>
            string.Equals(h.Movimiento, movimiento.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
