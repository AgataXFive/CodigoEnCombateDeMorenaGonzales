namespace Persistencia.Repositorios;

public interface IRepositorio<T> where T : class
{
    void Agregar(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
    IReadOnlyCollection<T> ObtenerTodos();
}
