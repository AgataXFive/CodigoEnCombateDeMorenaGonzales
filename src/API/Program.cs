using API;
using Persistencia;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var corsOriginsOverride = builder.Configuration["COMBATE_API_CORS_ORIGINS"];
if (!string.IsNullOrWhiteSpace(corsOriginsOverride))
    allowedOrigins = corsOriginsOverride.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddSingleton<ApiConnectionFactories>();
builder.Services.AddSingleton<MySqlCombateStore>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("Frontend");
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ApiException exception)
    {
        context.Response.StatusCode = exception.StatusCode;
        await context.Response.WriteAsJsonAsync(new { error = exception.Message });
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "Error procesando la solicitud.");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "Error interno al procesar la solicitud." });
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/estado/base-datos", (ApiConnectionFactories factories) =>
{
    try
    {
        using var connection = factories.Lectura.CrearConexion();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        var result = Convert.ToInt32(command.ExecuteScalar());
        return (IResult)Results.Ok(new { estado = "conectada", comprobacion = result });
    }
    catch (Exception exception)
    {
        app.Logger.LogWarning(exception, "No se pudo comprobar la conexión de lectura a MySQL.");
        return Results.Problem("No se pudo conectar con la base configurada.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).WithTags("Diagnóstico").WithSummary("Comprueba la conexión MySQL mediante Persistencia").Produces(200).Produces(503);

var characters = app.MapGroup("/api/personajes").WithTags("Personajes");
characters.MapGet("", (MySqlCombateStore store) => Results.Ok(store.GetCharacters()))
    .WithSummary("Lista los personajes").Produces<IReadOnlyCollection<PersonajeResponse>>(200);
characters.MapGet("/{id:int}", (int id, MySqlCombateStore store) => Results.Ok(store.GetCharacter(id)))
    .WithSummary("Obtiene un personaje").Produces<PersonajeResponse>(200).Produces(404);
characters.MapGet("/{id:int}/combates", (int id, MySqlCombateStore store) => Results.Ok(store.GetCharacterFights(id)))
    .WithSummary("Lista los combates de un personaje").Produces<IReadOnlyCollection<CombateResponse>>(200).Produces(404);
characters.MapPost("", (CrearPersonajeRequest request, MySqlCombateStore store) =>
    Results.Created($"/api/personajes", store.CreateCharacter(request.Nombre, request.Tipo)))
    .WithSummary("Crea un personaje").Produces<IdNombreResponse>(201).Produces(400);
characters.MapPut("/{id:int}", (int id, ActualizarPersonajeRequest request, MySqlCombateStore store) =>
    Results.Ok(store.UpdateCharacter(id, request.Nombre)))
    .WithSummary("Modifica el nombre de un personaje").Produces<PersonajeResponse>(200).Produces(400).Produces(404);
characters.MapDelete("/{id:int}", (int id, MySqlCombateStore store) =>
{
    store.DeleteCharacter(id);
    return Results.NoContent();
}).WithSummary("Elimina un personaje sin combates").Produces(204).Produces(404).Produces(409);

var fights = app.MapGroup("/api/combates").WithTags("Combates");
fights.MapGet("", (MySqlCombateStore store) => Results.Ok(store.GetFights()))
    .WithSummary("Lista los combates").Produces<IReadOnlyCollection<CombateResponse>>(200);
fights.MapPost("", (CrearCombateRequest request, MySqlCombateStore store) =>
    Results.Created("/api/combates", store.CreateFight(request.Personaje1Id, request.Personaje2Id)))
    .WithSummary("Inicia un combate").Produces<IdNombreResponse>(201).Produces(400).Produces(404);
fights.MapGet("/{id:int}", (int id, MySqlCombateStore store) => Results.Ok(store.GetFight(id)))
    .WithSummary("Obtiene el combate, sus turnos y el resultado").Produces<CombateDetalleResponse>(200).Produces(404);
fights.MapPut("/{id:int}", (int id, CrearCombateRequest request, MySqlCombateStore store) =>
    Results.Ok(store.UpdateFight(id, request.Personaje1Id, request.Personaje2Id)))
    .WithSummary("Cambia los participantes antes del primer turno").Produces<CombateResponse>(200).Produces(400).Produces(404).Produces(409);
fights.MapDelete("/{id:int}", (int id, MySqlCombateStore store) =>
{
    store.DeleteFight(id);
    return Results.NoContent();
}).WithSummary("Elimina un combate sin turnos").Produces(204).Produces(404).Produces(409);
fights.MapGet("/{id:int}/turnos", (int id, MySqlCombateStore store) => Results.Ok(store.GetFightTurns(id)))
    .WithSummary("Lista los turnos de un combate").Produces<IReadOnlyCollection<TurnoResponse>>(200).Produces(404);
fights.MapPost("/{id:int}/turnos", (int id, CrearTurnoRequest request, MySqlCombateStore store) =>
    Results.Ok(store.ExecuteTurn(id, request.AtacanteId, request.Accion)))
    .WithSummary("Ejecuta un turno del combate").Produces<TurnoEjecutadoResponse>(200).Produces(400).Produces(404);
fights.MapPut("/{id:int}/turnos/{turnoId:int}", (int id, int turnoId, ActualizarTurnoRequest request, MySqlCombateStore store) =>
    Results.Ok(store.UpdateTurnNote(id, turnoId, request.Nota)))
    .WithSummary("Modifica solo la nota de un turno").Produces<TurnoResponse>(200).Produces(400).Produces(404);
fights.MapDelete("/{id:int}/turnos/{turnoId:int}", (int id, int turnoId, MySqlCombateStore store) =>
{
    store.DeleteTurn(id, turnoId);
    return Results.NoContent();
}).WithSummary("Elimina el último turno y restaura el estado previo").Produces(204).Produces(404).Produces(409);

app.Run();

public partial class Program;