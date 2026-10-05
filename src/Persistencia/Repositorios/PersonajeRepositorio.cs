using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public class PersonajeRepositorio : RepositorioBase<Personaje>, IPersonajeRepositorio
{
    public Personaje? ObtenerPorNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));

        return Entidades.FirstOrDefault(p =>
            string.Equals(p.Nombre, nombre.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<Personaje> ObtenerPorTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("El tipo es obligatorio.", nameof(tipo));

        string tipoNormalizado = tipo.Trim();

        return Entidades
            .Where(p => p.GetType().Name.Equals(tipoNormalizado, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }
}
