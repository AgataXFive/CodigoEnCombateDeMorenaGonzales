using Persistencia;

namespace API;

public sealed class ApiConnectionFactories(IConfiguration configuration)
{
    public IDbConnectionFactory Lectura => Crear("COMBATE_API_READ_CONNECTION");
    public IDbConnectionFactory Administrador => Crear("COMBATE_API_ADMIN_CONNECTION");

    private IDbConnectionFactory Crear(string settingName)
    {
        var connectionString = configuration[settingName];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Configure la variable {settingName}.");

        return new MySqlConnectionFactory(connectionString);
    }
}