using System.Data;
using MySql.Data.MySqlClient;

namespace Persistencia;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La cadena de conexión es obligatoria.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public IDbConnection CrearConexion()
    {
        return new MySqlConnection(_connectionString);
    }
}
