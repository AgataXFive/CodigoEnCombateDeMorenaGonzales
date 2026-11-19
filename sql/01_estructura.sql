CREATE DATABASE IF NOT EXISTS combate_api CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE combate_api;

CREATE TABLE IF NOT EXISTS personajes (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    nivel INT NOT NULL DEFAULT 1 CHECK (nivel > 0),
    poder INT NOT NULL DEFAULT 20 CHECK (poder > 0),
    vida INT NOT NULL DEFAULT 100 CHECK (vida >= 0),
    ataque INT NOT NULL DEFAULT 10 CHECK (ataque > 0),
    defensa INT NOT NULL DEFAULT 5 CHECK (defensa >= 0),
    atributo_especial INT NOT NULL DEFAULT 10 CHECK (atributo_especial > 0),
    CONSTRAINT ck_personajes_tipo CHECK (tipo IN ('Guerrero', 'Mago', 'Arquero', 'Asesino')),
    INDEX ix_personajes_nombre (nombre)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS combates (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    personaje1_id INT NOT NULL,
    personaje2_id INT NOT NULL,
    vida1 INT NOT NULL CHECK (vida1 >= 0),
    vida2 INT NOT NULL CHECK (vida2 >= 0),
    estado VARCHAR(20) NOT NULL DEFAULT 'En curso',
    ganador_id INT NULL,
    creado_en TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ck_combatientes_distintos CHECK (personaje1_id <> personaje2_id),
    CONSTRAINT ck_combate_estado CHECK (estado IN ('En curso', 'Finalizado')),
    CONSTRAINT fk_combate_personaje1 FOREIGN KEY (personaje1_id) REFERENCES personajes(id),
    CONSTRAINT fk_combate_personaje2 FOREIGN KEY (personaje2_id) REFERENCES personajes(id),
    CONSTRAINT fk_combate_ganador FOREIGN KEY (ganador_id) REFERENCES personajes(id),
    INDEX ix_combate_personaje1 (personaje1_id),
    INDEX ix_combate_personaje2 (personaje2_id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS turnos (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    combate_id INT NOT NULL,
    numero INT NOT NULL CHECK (numero > 0),
    atacante_id INT NOT NULL,
    accion VARCHAR(20) NOT NULL,
    dano INT NOT NULL CHECK (dano >= 0),
    vida1_antes INT NOT NULL CHECK (vida1_antes >= 0),
    vida2_antes INT NOT NULL CHECK (vida2_antes >= 0),
    estado_antes VARCHAR(20) NOT NULL,
    ganador_antes INT NULL,
    nota VARCHAR(500) NULL,
    CONSTRAINT uq_turno_numero UNIQUE (combate_id, numero),
    CONSTRAINT ck_turno_accion CHECK (accion IN ('atacar', 'habilidad')),
    CONSTRAINT fk_turno_combate FOREIGN KEY (combate_id) REFERENCES combates(id),
    CONSTRAINT fk_turno_atacante FOREIGN KEY (atacante_id) REFERENCES personajes(id),
    CONSTRAINT fk_turno_ganador_antes FOREIGN KEY (ganador_antes) REFERENCES personajes(id),
    INDEX ix_turnos_combate (combate_id, numero)
) ENGINE=InnoDB;