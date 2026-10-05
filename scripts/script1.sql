DROP DATABASE IF EXISTS juego_combate_morena;
CREATE DATABASE juego_combate_morena CHARACTER SET utf8mb4 COLLATE utf8mb4_spanish_ci;
USE juego_combate_morena;

CREATE TABLE TipoPersonaje (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(30) NOT NULL UNIQUE
);

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
);

CREATE TABLE Habilidad (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(80) NOT NULL UNIQUE,
    descripcion VARCHAR(255) NULL
);

CREATE TABLE PersonajeHabilidad (
    personaje_id INT NOT NULL,
    habilidad_id INT NOT NULL,
    PRIMARY KEY (personaje_id, habilidad_id),
    FOREIGN KEY (personaje_id) REFERENCES Personaje(id),
    FOREIGN KEY (habilidad_id) REFERENCES Habilidad(id)
);

CREATE TABLE Batalla (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    personaje_1_id INT NOT NULL,
    personaje_2_id INT NOT NULL,
    ganador_id INT NULL,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (personaje_1_id) REFERENCES Personaje(id),
    FOREIGN KEY (personaje_2_id) REFERENCES Personaje(id),
    FOREIGN KEY (ganador_id) REFERENCES Personaje(id)
);
