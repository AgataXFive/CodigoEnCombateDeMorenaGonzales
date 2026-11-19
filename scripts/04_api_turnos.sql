USE juego_combate_morena;

-- Migracion aditiva para habilitar la API sin borrar personajes ni batallas.
SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.columns
     WHERE table_schema = DATABASE() AND table_name = 'Batalla' AND column_name = 'vida1') = 0,
    'ALTER TABLE Batalla ADD COLUMN vida1 INT NOT NULL DEFAULT 100',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.columns
     WHERE table_schema = DATABASE() AND table_name = 'Batalla' AND column_name = 'vida2') = 0,
    'ALTER TABLE Batalla ADD COLUMN vida2 INT NOT NULL DEFAULT 100',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

SET @migration_sql = IF(
    (SELECT COUNT(*) FROM information_schema.columns
     WHERE table_schema = DATABASE() AND table_name = 'Batalla' AND column_name = 'estado') = 0,
    'ALTER TABLE Batalla ADD COLUMN estado VARCHAR(20) NOT NULL DEFAULT ''En curso''',
    'SELECT 1');
PREPARE migration_statement FROM @migration_sql;
EXECUTE migration_statement;
DEALLOCATE PREPARE migration_statement;

CREATE TABLE IF NOT EXISTS Turno (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    batalla_id INT NOT NULL,
    numero INT NOT NULL,
    atacante_id INT NOT NULL,
    accion VARCHAR(20) NOT NULL,
    dano INT NOT NULL,
    vida1_antes INT NOT NULL,
    vida2_antes INT NOT NULL,
    estado_antes VARCHAR(20) NOT NULL,
    ganador_antes INT NULL,
    nota VARCHAR(500) NULL,
    CONSTRAINT uq_turno_numero UNIQUE (batalla_id, numero),
    CONSTRAINT ck_turno_accion CHECK (accion IN ('atacar', 'habilidad')),
    CONSTRAINT fk_turno_batalla FOREIGN KEY (batalla_id) REFERENCES Batalla(id),
    CONSTRAINT fk_turno_atacante FOREIGN KEY (atacante_id) REFERENCES Personaje(id),
    CONSTRAINT fk_turno_ganador_antes FOREIGN KEY (ganador_antes) REFERENCES Personaje(id),
    INDEX ix_turno_batalla_numero (batalla_id, numero)
) ENGINE=InnoDB;

UPDATE Batalla b
JOIN Personaje p1 ON p1.id = b.personaje_1_id
JOIN Personaje p2 ON p2.id = b.personaje_2_id
SET b.vida1 = p1.vida,
    b.vida2 = p2.vida,
    b.estado = IF(b.ganador_id IS NULL, 'En curso', 'Finalizado')
WHERE NOT EXISTS (
    SELECT 1 FROM Turno t WHERE t.batalla_id = b.id
);