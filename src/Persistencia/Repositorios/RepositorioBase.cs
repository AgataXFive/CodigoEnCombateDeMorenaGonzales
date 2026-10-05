namespace Persistencia.Repositorios;

public abstract class RepositorioBase<T> : IRepositorio<T> where T : class
{
    protected readonly List<T> Entidades = new();

    public virtual void Agregar(T entidad)
    {
        ArgumentNullException.ThrowIfNull(entidad);
        Entidades.Add(entidad);
    }

    public virtual void Actualizar(T entidad)
    {
        ArgumentNullException.ThrowIfNull(entidad);

        int indice = Entidades.IndexOf(entidad);
        if (indice >= 0)
        {
            Entidades[indice] = entidad;
        }
        else
        {
            Entidades.Add(entidad);
        }
    }

    public virtual void Eliminar(T entidad)
    {
        ArgumentNullException.ThrowIfNull(entidad);
        Entidades.Remove(entidad);
    }

    public virtual IReadOnlyCollection<T> ObtenerTodos()
    {
        return Entidades.AsReadOnly();
    }
}
