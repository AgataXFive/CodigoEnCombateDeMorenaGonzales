USE juego_combate_morena;

INSERT INTO TipoPersonaje (nombre) VALUES
('Guerrero'),
('Mago'),
('Arquero'),
('Asesino');

INSERT INTO Personaje (nombre, nivel, poder, vida, ataque, defensa, mana, fuerza, punteria, sigilo, tipo_id) VALUES
('Aragorn', 3, 20, 100, 12, 6, 0, 12, 0, 0, 1),
('Gandalf', 4, 18, 80, 10, 4, 25, 0, 0, 0, 2),
('Legolas', 3, 17, 75, 11, 5, 0, 0, 10, 0, 3),
('Lara', 5, 19, 90, 14, 3, 0, 0, 0, 14, 4);

INSERT INTO Habilidad (nombre, descripcion) VALUES
('Espada de la justicia', 'Golpe pesado del guerrero.'),
('Bola de fuego', 'Hechizo ofensivo del mago.'),
('Tiro certero', 'Disparo preciso del arquero.'),
('Ataque furtivo', 'Golpe sigiloso del asesino.');

INSERT INTO PersonajeHabilidad (personaje_id, habilidad_id) VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 4);

INSERT INTO Batalla (nombre, personaje_1_id, personaje_2_id, ganador_id) VALUES
('Final de prueba', 1, 2, 1),
('Duelo de precisión', 3, 4, 4);

SELECT * FROM TipoPersonaje;
SELECT * FROM Personaje;
SELECT * FROM Habilidad;
SELECT * FROM Batalla;