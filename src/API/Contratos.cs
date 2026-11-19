namespace API;

public sealed record CrearPersonajeRequest(string Nombre, string Tipo);
public sealed record ActualizarPersonajeRequest(string Nombre);
public sealed record CrearCombateRequest(int Personaje1Id, int Personaje2Id);
public sealed record CrearTurnoRequest(int AtacanteId, string Accion);
public sealed record ActualizarTurnoRequest(string Nota);
public sealed record IdNombreResponse(int Id, string Nombre);
public sealed record PersonajeResponse(int Id, string Nombre, string Tipo, int Nivel, int Poder, int Vida, int Ataque, int Defensa);
public sealed record CombateResponse(int Id, int Personaje1Id, string Personaje1, int Personaje2Id, string Personaje2, string Estado);
public sealed record TurnoResponse(int Id, int Numero, int AtacanteId, string Accion, int Danio, int VidaPersonaje1, int VidaPersonaje2, string? Nota);
public sealed record CombateDetalleResponse(int Id, PersonajeResponse Personaje1, PersonajeResponse Personaje2, string Estado, string? Ganador, IReadOnlyCollection<TurnoResponse> Turnos);
public sealed record TurnoEjecutadoResponse(TurnoResponse Turno, string Estado, int VidaPersonaje1, int VidaPersonaje2, string? Ganador);

public sealed class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}