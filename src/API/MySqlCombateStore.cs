using System.Data;
using API;
using MySql.Data.MySqlClient;
using Persistencia;
using Persistencia.Entidades;

namespace API;

public sealed class MySqlCombateStore(ApiConnectionFactories connectionFactories)
{
    private sealed record CharacterRow(int Id, string Name, string Type, int Level, int Power, int Health, int Attack, int Defense, int Special);
    private sealed record FightRow(int Id, int Character1Id, int Character2Id, int Health1, int Health2, string State, int? WinnerId);

    private static readonly IReadOnlyDictionary<string, Func<CharacterRow, int, Personaje>> CharacterFactories =
        new Dictionary<string, Func<CharacterRow, int, Personaje>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Guerrero"] = (row, health) => new Guerrero(row.Name, row.Level, row.Power, row.Special, health, row.Attack, row.Defense),
            ["Mago"] = (row, health) => new Mago(row.Name, row.Level, row.Power, row.Special, health, row.Attack, row.Defense),
            ["Arquero"] = (row, health) => new Arquero(row.Name, row.Level, row.Power, row.Special, health, row.Attack, row.Defense),
            ["Asesino"] = (row, health) => new Asesino(row.Name, row.Level, row.Power, row.Special, health, row.Attack, row.Defense)
        };

    private MySqlConnection Open(bool write)
    {
        var factory = write ? connectionFactories.Administrador : connectionFactories.Lectura;
        var connection = (MySqlConnection)factory.CrearConexion();
        connection.Open();
        return connection;
    }

    private static MySqlCommand Command(MySqlConnection connection, string sql, MySqlTransaction? transaction = null, params (string Name, object? Value)[] parameters)
    {
        var command = new MySqlCommand(sql, connection, transaction);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }

    private static CharacterRow ReadCharacter(MySqlDataReader reader) => new(
        reader.GetInt32("id"), reader.GetString("nombre"), reader.GetString("tipo"), reader.GetInt32("nivel"),
        reader.GetInt32("poder"), reader.GetInt32("vida"), reader.GetInt32("ataque"), reader.GetInt32("defensa"),
        reader.GetInt32("atributo_especial"));

    private const string CharacterSelect = """
        SELECT p.id, p.nombre, tp.nombre AS tipo, p.nivel, p.poder, p.vida, p.ataque, p.defensa,
            CASE tp.nombre
                WHEN 'Guerrero' THEN p.fuerza
                WHEN 'Mago' THEN p.mana
                WHEN 'Arquero' THEN p.punteria
                WHEN 'Asesino' THEN p.sigilo
            END AS atributo_especial
        FROM Personaje p
        JOIN TipoPersonaje tp ON tp.id = p.tipo_id
        """;

    private static PersonajeResponse ToResponse(CharacterRow row) =>
        new(row.Id, row.Name, row.Type, row.Level, row.Power, row.Health, row.Attack, row.Defense);

    private static CharacterRow? GetCharacter(MySqlConnection connection, int id, MySqlTransaction? transaction = null, bool lockRow = false)
    {
        using var command = Command(connection,
            $"{CharacterSelect} WHERE p.id = @id{(lockRow ? " FOR UPDATE" : "")}",
            transaction, ("@id", id));
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadCharacter(reader) : null;
    }

    private static FightRow? GetFight(MySqlConnection connection, int id, MySqlTransaction? transaction = null, bool lockRow = false)
    {
        using var command = Command(connection,
            $"SELECT id, personaje_1_id AS personaje1_id, personaje_2_id AS personaje2_id, vida1, vida2, estado, ganador_id FROM Batalla WHERE id = @id{(lockRow ? " FOR UPDATE" : "")}",
            transaction, ("@id", id));
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;
        return new FightRow(reader.GetInt32("id"), reader.GetInt32("personaje1_id"), reader.GetInt32("personaje2_id"),
            reader.GetInt32("vida1"), reader.GetInt32("vida2"), reader.GetString("estado"),
            reader.IsDBNull("ganador_id") ? null : reader.GetInt32("ganador_id"));
    }

    private static CharacterRow RequireCharacter(MySqlConnection connection, int id, MySqlTransaction? transaction = null, bool lockRow = false) =>
        GetCharacter(connection, id, transaction, lockRow) ?? throw new ApiException(404, $"No existe el personaje {id}.");

    private static int GetTypeId(MySqlConnection connection, string type, MySqlTransaction transaction)
    {
        using var command = Command(connection, "SELECT id FROM TipoPersonaje WHERE nombre = @nombre", transaction,
            ("@nombre", type));
        var result = command.ExecuteScalar();
        return result is null || result is DBNull
            ? throw new ApiException(400, $"No existe el tipo de personaje {type} en la base.")
            : Convert.ToInt32(result);
    }

    private static FightRow RequireFight(MySqlConnection connection, int id, MySqlTransaction? transaction = null, bool lockRow = false) =>
        GetFight(connection, id, transaction, lockRow) ?? throw new ApiException(404, $"No existe el combate {id}.");

    public IReadOnlyCollection<PersonajeResponse> GetCharacters()
    {
        using var connection = Open(false);
        using var command = Command(connection, $"{CharacterSelect} ORDER BY p.id");
        using var reader = command.ExecuteReader();
        var characters = new List<PersonajeResponse>();
        while (reader.Read()) characters.Add(ToResponse(ReadCharacter(reader)));
        return characters;
    }

    public PersonajeResponse GetCharacter(int id)
    {
        using var connection = Open(false);
        return ToResponse(RequireCharacter(connection, id));
    }

    public IReadOnlyCollection<CombateResponse> GetCharacterFights(int id)
    {
        using var connection = Open(false);
        _ = RequireCharacter(connection, id);
        using var command = Command(connection, """
            SELECT c.id, c.personaje_1_id AS personaje1_id, p1.nombre AS nombre1,
                c.personaje_2_id AS personaje2_id, p2.nombre AS nombre2, c.estado
            FROM Batalla c
            JOIN Personaje p1 ON p1.id = c.personaje_1_id
            JOIN Personaje p2 ON p2.id = c.personaje_2_id
            WHERE c.personaje_1_id = @id OR c.personaje_2_id = @id
            ORDER BY c.id
            """, null, ("@id", id));
        using var reader = command.ExecuteReader();
        var fights = new List<CombateResponse>();
        while (reader.Read()) fights.Add(ReadFightResponse(reader));
        return fights;
    }

    public IdNombreResponse CreateCharacter(string name, string type)
    {
        ValidateName(name);
        if (string.IsNullOrWhiteSpace(type) || !CharacterFactories.ContainsKey(type.Trim()))
            throw new ApiException(400, "Tipo inválido. Use Guerrero, Mago, Arquero o Asesino.");

        var normalizedType = CharacterFactories.Keys.First(key => key.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        var typeId = GetTypeId(connection, normalizedType, transaction);
        const int special = 10;
        using var command = Command(connection, """
            INSERT INTO Personaje (nombre, nivel, poder, vida, ataque, defensa, mana, fuerza, punteria, sigilo, tipo_id)
            VALUES (@nombre, 1, 20, 100, 10, 5, @mana, @fuerza, @punteria, @sigilo, @tipo)
            """, transaction, ("@nombre", name.Trim()), ("@tipo", typeId),
            ("@mana", (object)(normalizedType == "Mago" ? special : 0)),
            ("@fuerza", normalizedType == "Guerrero" ? special : 0),
            ("@punteria", normalizedType == "Arquero" ? special : 0),
            ("@sigilo", normalizedType == "Asesino" ? special : 0));
        command.ExecuteNonQuery();
        var id = (int)command.LastInsertedId;
        transaction.Commit();
        return new IdNombreResponse(id, name.Trim());
    }

    public PersonajeResponse UpdateCharacter(int id, string name)
    {
        ValidateName(name);
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        var row = RequireCharacter(connection, id, transaction, true);
        using var command = Command(connection, "UPDATE Personaje SET nombre = @nombre WHERE id = @id", transaction,
            ("@nombre", name.Trim()), ("@id", id));
        command.ExecuteNonQuery();
        transaction.Commit();
        return ToResponse(row with { Name = name.Trim() });
    }

    public void DeleteCharacter(int id)
    {
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        _ = RequireCharacter(connection, id, transaction, true);
        using (var usage = Command(connection, "SELECT EXISTS(SELECT 1 FROM Batalla WHERE personaje_1_id = @id OR personaje_2_id = @id)", transaction, ("@id", id)))
        {
            if (Convert.ToInt32(usage.ExecuteScalar()) != 0)
                throw new ApiException(409, "No se puede eliminar un personaje que participó en un combate.");
        }
        using (var abilities = Command(connection, "DELETE FROM PersonajeHabilidad WHERE personaje_id = @id", transaction, ("@id", id)))
            abilities.ExecuteNonQuery();
        using var command = Command(connection, "DELETE FROM Personaje WHERE id = @id", transaction, ("@id", id));
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public IReadOnlyCollection<CombateResponse> GetFights()
    {
        using var connection = Open(false);
        using var command = Command(connection, """
            SELECT c.id, c.personaje_1_id AS personaje1_id, p1.nombre AS nombre1,
                c.personaje_2_id AS personaje2_id, p2.nombre AS nombre2, c.estado
            FROM Batalla c
            JOIN Personaje p1 ON p1.id = c.personaje_1_id
            JOIN Personaje p2 ON p2.id = c.personaje_2_id
            ORDER BY c.id
            """);
        using var reader = command.ExecuteReader();
        var fights = new List<CombateResponse>();
        while (reader.Read()) fights.Add(ReadFightResponse(reader));
        return fights;
    }

    private static CombateResponse ReadFightResponse(MySqlDataReader reader) => new(
        reader.GetInt32("id"), reader.GetInt32("personaje1_id"), reader.GetString("nombre1"),
        reader.GetInt32("personaje2_id"), reader.GetString("nombre2"), reader.GetString("estado"));

    public IdNombreResponse CreateFight(int character1Id, int character2Id)
    {
        ValidateParticipants(character1Id, character2Id);
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        var character1 = RequireCharacter(connection, character1Id, transaction, true);
        var character2 = RequireCharacter(connection, character2Id, transaction, true);
        using var command = Command(connection, """
            INSERT INTO Batalla (nombre, personaje_1_id, personaje_2_id, ganador_id, vida1, vida2, estado)
            VALUES (@nombre, @personaje1, @personaje2, NULL, @vida1, @vida2, 'En curso')
            """, transaction, ("@nombre", $"Combate {character1.Name} vs. {character2.Name}"),
            ("@personaje1", character1Id), ("@personaje2", character2Id),
            ("@vida1", character1.Health), ("@vida2", character2.Health));
        command.ExecuteNonQuery();
        transaction.Commit();
        return new IdNombreResponse((int)command.LastInsertedId, $"Combate {character1.Name} vs. {character2.Name}");
    }

    public CombateDetalleResponse GetFight(int id)
    {
        using var connection = Open(false);
        var fight = RequireFight(connection, id);
        var character1 = RequireCharacter(connection, fight.Character1Id);
        var character2 = RequireCharacter(connection, fight.Character2Id);
        string? winner = fight.WinnerId is int winnerId ? RequireCharacter(connection, winnerId).Name : null;
        return new CombateDetalleResponse(fight.Id,
            ToResponse(character1 with { Health = fight.Health1 }), ToResponse(character2 with { Health = fight.Health2 }),
            fight.State, winner, GetTurns(connection, id));
    }

    public CombateResponse UpdateFight(int id, int character1Id, int character2Id)
    {
        ValidateParticipants(character1Id, character2Id);
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        _ = RequireFight(connection, id, transaction, true);
        if (HasTurns(connection, id, transaction))
            throw new ApiException(409, "No se puede modificar un combate que ya tiene turnos.");
        var character1 = RequireCharacter(connection, character1Id, transaction, true);
        var character2 = RequireCharacter(connection, character2Id, transaction, true);
        using var command = Command(connection, """
            UPDATE Batalla SET personaje_1_id = @personaje1, personaje_2_id = @personaje2,
                vida1 = @vida1, vida2 = @vida2, estado = 'En curso', ganador_id = NULL
            WHERE id = @id
            """, transaction, ("@personaje1", character1Id), ("@personaje2", character2Id),
            ("@vida1", character1.Health), ("@vida2", character2.Health), ("@id", id));
        command.ExecuteNonQuery();
        transaction.Commit();
        return new CombateResponse(id, character1Id, character1.Name, character2Id, character2.Name, "En curso");
    }

    public void DeleteFight(int id)
    {
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        _ = RequireFight(connection, id, transaction, true);
        if (HasTurns(connection, id, transaction))
            throw new ApiException(409, "No se puede eliminar un combate que ya tiene turnos.");
        using var command = Command(connection, "DELETE FROM Batalla WHERE id = @id", transaction, ("@id", id));
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public IReadOnlyCollection<TurnoResponse> GetFightTurns(int id)
    {
        using var connection = Open(false);
        _ = RequireFight(connection, id);
        return GetTurns(connection, id);
    }

    private static IReadOnlyCollection<TurnoResponse> GetTurns(MySqlConnection connection, int fightId)
    {
        using var command = Command(connection, """
            SELECT id, numero, atacante_id, accion, dano, vida1_antes, vida2_antes, nota
            FROM Turno WHERE batalla_id = @id ORDER BY numero
            """, null, ("@id", fightId));
        using var reader = command.ExecuteReader();
        var turns = new List<TurnoResponse>();
        while (reader.Read()) turns.Add(new TurnoResponse(reader.GetInt32("id"), reader.GetInt32("numero"),
            reader.GetInt32("atacante_id"), reader.GetString("accion"), reader.GetInt32("dano"),
            reader.GetInt32("vida1_antes"), reader.GetInt32("vida2_antes"),
            reader.IsDBNull("nota") ? null : reader.GetString("nota")));
        return turns;
    }

    public TurnoEjecutadoResponse ExecuteTurn(int fightId, int attackerId, string action)
    {
        if (string.IsNullOrWhiteSpace(action) || !(action.Equals("atacar", StringComparison.OrdinalIgnoreCase) || action.Equals("habilidad", StringComparison.OrdinalIgnoreCase)))
            throw new ApiException(400, "La acción debe ser 'atacar' o 'habilidad'.");

        using var connection = Open(true);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        var fight = RequireFight(connection, fightId, transaction, true);
        if (fight.State != "En curso")
            throw new ApiException(400, "El combate ya finalizó.");

        var attackerRow = RequireCharacter(connection, attackerId, transaction, true);
        var number = CountTurns(connection, fightId, transaction) + 1;
        var expectedAttacker = number % 2 == 1 ? fight.Character1Id : fight.Character2Id;
        if (attackerId != expectedAttacker)
            throw new ApiException(400, "El atacante está fuera de turno.");

        var defenderId = attackerId == fight.Character1Id ? fight.Character2Id : fight.Character1Id;
        var defenderRow = RequireCharacter(connection, defenderId, transaction, true);
        var attackerHealth = attackerId == fight.Character1Id ? fight.Health1 : fight.Health2;
        var defenderHealth = defenderId == fight.Character1Id ? fight.Health1 : fight.Health2;
        var attacker = CreateDomainCharacter(attackerRow, attackerHealth);
        var defender = CreateDomainCharacter(defenderRow, defenderHealth);
        var before1 = fight.Health1;
        var before2 = fight.Health2;

        try
        {
            if (action.Equals("atacar", StringComparison.OrdinalIgnoreCase)) attacker.Atacar(defender);
            else attacker.UsarHabilidad(defender);
        }
        catch (InvalidOperationException exception)
        {
            throw new ApiException(400, exception.Message);
        }

        var health1 = attackerId == fight.Character1Id ? attacker.Vida : defender.Vida;
        var health2 = attackerId == fight.Character2Id ? attacker.Vida : defender.Vida;
        var damage = Math.Max(0, defenderHealth - defender.Vida);
        int? winnerId = defender.Derrotado ? attackerId : null;
        var state = winnerId is null ? "En curso" : "Finalizado";
        using (var insert = Command(connection, """
            INSERT INTO Turno (batalla_id, numero, atacante_id, accion, dano, vida1_antes, vida2_antes, estado_antes, ganador_antes)
            VALUES (@combate, @numero, @atacante, @accion, @dano, @vida1, @vida2, @estado, @ganador)
            """, transaction, ("@combate", fightId), ("@numero", number), ("@atacante", attackerId),
            ("@accion", action.ToLowerInvariant()), ("@dano", damage), ("@vida1", before1), ("@vida2", before2),
            ("@estado", fight.State), ("@ganador", fight.WinnerId)))
        {
            insert.ExecuteNonQuery();
            var turnId = (int)insert.LastInsertedId;
            using var update = Command(connection, "UPDATE Batalla SET vida1 = @vida1, vida2 = @vida2, estado = @estado, ganador_id = @ganador WHERE id = @id", transaction,
                ("@vida1", health1), ("@vida2", health2), ("@estado", state), ("@ganador", winnerId), ("@id", fightId));
            update.ExecuteNonQuery();
            transaction.Commit();
            return new TurnoEjecutadoResponse(new TurnoResponse(turnId, number, attackerId, action.ToLowerInvariant(), damage, before1, before2, null), state, health1, health2,
                winnerId is int id ? (attackerId == id ? attackerRow.Name : defenderRow.Name) : null);
        }
    }

    public TurnoResponse UpdateTurnNote(int fightId, int turnId, string note)
    {
        if (string.IsNullOrWhiteSpace(note) || note.Trim().Length > 500)
            throw new ApiException(400, "La nota es obligatoria y no puede superar 500 caracteres.");
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction();
        _ = RequireFight(connection, fightId, transaction, true);
        var turn = GetTurn(connection, fightId, turnId, transaction, true) ?? throw new ApiException(404, $"No existe el turno {turnId} en el combate {fightId}.");
        using var command = Command(connection, "UPDATE Turno SET nota = @nota WHERE id = @id", transaction,
            ("@nota", note.Trim()), ("@id", turnId));
        command.ExecuteNonQuery();
        transaction.Commit();
        return turn.ToResponse() with { Nota = note.Trim() };
    }

    public void DeleteTurn(int fightId, int turnId)
    {
        using var connection = Open(true);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        _ = RequireFight(connection, fightId, transaction, true);
        var turn = GetTurn(connection, fightId, turnId, transaction, true) ?? throw new ApiException(404, $"No existe el turno {turnId} en el combate {fightId}.");
        if (turn.Number != CountTurns(connection, fightId, transaction))
            throw new ApiException(409, "Solo se puede eliminar el último turno del combate.");

        using (var delete = Command(connection, "DELETE FROM Turno WHERE id = @id", transaction, ("@id", turnId)))
            delete.ExecuteNonQuery();
        using (var restore = Command(connection, """
            UPDATE Batalla SET vida1 = @vida1, vida2 = @vida2, estado = @estado, ganador_id = @ganador
            WHERE id = @id
            """, transaction, ("@vida1", turn.Health1Before), ("@vida2", turn.Health2Before),
            ("@estado", turn.StateBefore), ("@ganador", turn.WinnerBefore), ("@id", fightId)))
            restore.ExecuteNonQuery();
        transaction.Commit();
    }

    private sealed record TurnRow(int Id, int Number, int AttackerId, string Action, int Damage, int Health1Before, int Health2Before, string StateBefore, int? WinnerBefore, string? Note)
    {
        public TurnoResponse ToResponse() => new(Id, Number, AttackerId, Action, Damage, Health1Before, Health2Before, Note);
    }

    private static TurnRow? GetTurn(MySqlConnection connection, int fightId, int turnId, MySqlTransaction? transaction = null, bool lockRow = false)
    {
        using var command = Command(connection, $"""
            SELECT id, numero, atacante_id, accion, dano, vida1_antes, vida2_antes, estado_antes, ganador_antes, nota
            FROM Turno WHERE batalla_id = @combate AND id = @turno{(lockRow ? " FOR UPDATE" : "")}
            """, transaction, ("@combate", fightId), ("@turno", turnId));
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new TurnRow(reader.GetInt32("id"), reader.GetInt32("numero"), reader.GetInt32("atacante_id"), reader.GetString("accion"),
            reader.GetInt32("dano"), reader.GetInt32("vida1_antes"), reader.GetInt32("vida2_antes"), reader.GetString("estado_antes"),
            reader.IsDBNull("ganador_antes") ? null : reader.GetInt32("ganador_antes"), reader.IsDBNull("nota") ? null : reader.GetString("nota"));
    }

    private static Personaje CreateDomainCharacter(CharacterRow row, int health) =>
        CharacterFactories.TryGetValue(row.Type, out var factory)
            ? factory(row, health)
            : throw new ApiException(400, $"Tipo de personaje no soportado: {row.Type}.");

    private static int CountTurns(MySqlConnection connection, int fightId, MySqlTransaction transaction)
    {
        using var command = Command(connection, "SELECT COUNT(*) FROM Turno WHERE batalla_id = @id", transaction, ("@id", fightId));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static bool HasTurns(MySqlConnection connection, int fightId, MySqlTransaction transaction) => CountTurns(connection, fightId, transaction) > 0;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 50)
            throw new ApiException(400, "El nombre es obligatorio y no puede superar 50 caracteres.");
    }

    private static void ValidateParticipants(int character1Id, int character2Id)
    {
        if (character1Id <= 0 || character2Id <= 0 || character1Id == character2Id)
            throw new ApiException(400, "Se requieren dos IDs de personajes distintos y válidos.");
    }
}