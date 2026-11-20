DROP DATABASE IF EXISTS 5to_combate_morena;
CREATE DATABASE 5to_combate_morena CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;
USE 5to_combate_morena;

CREATE TABLE TipoPersonaje (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(30) NOT NULL UNIQUE
) ENGINE=InnoDB;

CREATE TABLE Personaje (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    nivel INT NOT NULL CHECK (nivel > 0),
    poder INT NOT NULL CHECK (poder > 0),
    vida INT NOT NULL CHECK (vida >= 0),
    ataque INT NOT NULL CHECK (ataque > 0),
    defensa INT NOT NULL CHECK (defensa >= 0),
    mana INT DEFAULT 0 CHECK (mana >= 0),
    fuerza INT DEFAULT 0 CHECK (fuerza >= 0),
    punteria INT DEFAULT 0 CHECK (punteria >= 0),
    sigilo INT DEFAULT 0 CHECK (sigilo >= 0),
    tipo_id INT NOT NULL,
    FOREIGN KEY (tipo_id) REFERENCES TipoPersonaje(id)
) ENGINE=InnoDB;

CREATE TABLE Habilidad (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(80) NOT NULL UNIQUE,
    descripcion VARCHAR(255) NULL
) ENGINE=InnoDB;

CREATE TABLE PersonajeHabilidad (
    personaje_id INT NOT NULL,
    habilidad_id INT NOT NULL,
    PRIMARY KEY (personaje_id, habilidad_id),
    FOREIGN KEY (personaje_id) REFERENCES Personaje(id),
    FOREIGN KEY (habilidad_id) REFERENCES Habilidad(id)
) ENGINE=InnoDB;

CREATE TABLE Batalla (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    personaje_1_id INT NOT NULL,
    personaje_2_id INT NOT NULL,
    ganador_id INT NULL,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    vida1 INT NOT NULL DEFAULT 100,
    vida2 INT NOT NULL DEFAULT 100,
    estado VARCHAR(20) NOT NULL DEFAULT 'En curso',
    FOREIGN KEY (personaje_1_id) REFERENCES Personaje(id),
    FOREIGN KEY (personaje_2_id) REFERENCES Personaje(id),
    FOREIGN KEY (ganador_id) REFERENCES Personaje(id)
) ENGINE=InnoDB;

CREATE TABLE Turno (
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

