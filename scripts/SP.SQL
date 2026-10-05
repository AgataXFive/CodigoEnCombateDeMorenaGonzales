USE juego_combate_morena;

DELIMITER $$

DROP PROCEDURE IF EXISTS sp_registrar_batalla $$
CREATE PROCEDURE sp_registrar_batalla(
    IN p_nombre VARCHAR(100),
    IN p_personaje_1_id INT,
    IN p_personaje_2_id INT,
    OUT p_batalla_id INT
)
BEGIN
    DECLARE v_vida_1 INT DEFAULT NULL;
    DECLARE v_vida_2 INT DEFAULT NULL;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
DROP PROCEDURE IF EXISTS sp_registrar_batalla $$
CREATE PROCEDURE sp_registrar_batalla(
    IN p_nombre VARCHAR(100),
    IN p_personaje_1_id INT,
    IN p_personaje_2_id INT,
    OUT p_batalla_id INT
)
BEGIN
    DECLARE v_vida_1 INT DEFAULT NULL;
    DECLARE v_vida_2 INT DEFAULT NULL;
    END;

    START TRANSACTION;

    IF p_nombre IS NULL OR TRIM(p_nombre) = ''
        OR p_personaje_1_id IS NULL OR p_personaje_2_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El nombre y los dos personajes son obligatorios.';
    END IF;

    IF p_personaje_1_id = p_personaje_2_id THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Una batalla requiere dos personajes distintos.';
    END IF;

    SELECT vida INTO v_vida_1
    FROM Personaje
    WHERE id = p_personaje_1_id
    FOR UPDATE;

    IF v_vida_1 IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El primer personaje no existe.';
    END IF;

    SELECT vida INTO v_vida_2
    FROM Personaje
    WHERE id = p_personaje_2_id
    FOR UPDATE;

    IF v_vida_2 IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El segundo personaje no existe.';
    END IF;

    IF v_vida_1 <= 0 OR v_vida_2 <= 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No se puede iniciar una batalla con personajes derrotados.';
    END IF;

    INSERT INTO Batalla (nombre, personaje_1_id, personaje_2_id, ganador_id)
    VALUES (TRIM(p_nombre), p_personaje_1_id, p_personaje_2_id, NULL);

    SET p_batalla_id = LAST_INSERT_ID();
    COMMIT;
END $$

DROP PROCEDURE IF EXISTS sp_cerrar_batalla $$
CREATE PROCEDURE sp_cerrar_batalla(
    IN p_batalla_id INT,
    IN p_ganador_id INT
)
BEGIN
    DECLARE v_personaje_1_id INT DEFAULT NULL;
    DECLARE v_personaje_2_id INT DEFAULT NULL;
    DECLARE v_ganador_actual_id INT DEFAULT NULL;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    IF p_batalla_id IS NULL OR p_ganador_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'La batalla y el ganador son obligatorios.';
    END IF;

    SELECT personaje_1_id, personaje_2_id, ganador_id
    INTO v_personaje_1_id, v_personaje_2_id, v_ganador_actual_id
    FROM Batalla
    WHERE id = p_batalla_id
    FOR UPDATE;

    IF v_personaje_1_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'La batalla indicada no existe.';
    END IF;

    IF v_ganador_actual_id IS NOT NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'La batalla ya fue cerrada.';
    END IF;

    IF p_ganador_id <> v_personaje_1_id AND p_ganador_id <> v_personaje_2_id THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El ganador debe ser uno de los participantes.';
    END IF;

    UPDATE Batalla
    SET ganador_id = p_ganador_id
    WHERE id = p_batalla_id;

    COMMIT;
END $$

DROP PROCEDURE IF EXISTS sp_obtener_ranking_por_tipo_periodo $$
CREATE PROCEDURE sp_obtener_ranking_por_tipo_periodo(
    IN p_desde DATETIME,
    IN p_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    IF p_desde IS NULL OR p_hasta IS NULL OR p_desde >= p_hasta THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El período debe tener fechas válidas y p_desde menor que p_hasta.';
    END IF;

    SELECT
        tp.nombre AS tipo_personaje,
        COUNT(*) AS participaciones,
        COUNT(DISTINCT participaciones.batalla_id) AS batallas,
        SUM(CASE WHEN participaciones.ganador_id = p.id THEN 1 ELSE 0 END) AS victorias,
        ROUND(
            100.0 * SUM(CASE WHEN participaciones.ganador_id = p.id THEN 1 ELSE 0 END) / COUNT(*),
            2
        ) AS porcentaje_victorias
    FROM (
        SELECT b.id AS batalla_id, b.ganador_id, b.personaje_1_id AS personaje_id
        FROM Batalla b
        WHERE b.fecha_creacion >= p_desde
            AND b.fecha_creacion < p_hasta
            AND b.ganador_id IS NOT NULL

        UNION ALL

        SELECT b.id AS batalla_id, b.ganador_id, b.personaje_2_id AS personaje_id
        FROM Batalla b
        WHERE b.fecha_creacion >= p_desde
            AND b.fecha_creacion < p_hasta
            AND b.ganador_id IS NOT NULL
    ) AS participaciones
    INNER JOIN Personaje p ON p.id = participaciones.personaje_id
    INNER JOIN TipoPersonaje tp ON tp.id = p.tipo_id
    GROUP BY tp.id, tp.nombre
    ORDER BY victorias DESC, porcentaje_victorias DESC, tp.nombre ASC;

    COMMIT;
END $$

DROP PROCEDURE IF EXISTS sp_reporte_batallas_periodo $$
CREATE PROCEDURE sp_reporte_batallas_periodo(
    IN p_desde DATETIME,
    IN p_hasta DATETIME
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    IF p_desde IS NULL OR p_hasta IS NULL OR p_desde >= p_hasta THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El período debe tener fechas válidas y p_desde menor que p_hasta.';
    END IF;

    SELECT
        b.id AS batalla_id,
        b.nombre AS batalla,
        b.fecha_creacion,
        p1.nombre AS personaje_1,
        t1.nombre AS tipo_personaje_1,
        p2.nombre AS personaje_2,
        t2.nombre AS tipo_personaje_2,
        pg.nombre AS ganador,
        CASE
            WHEN b.ganador_id IS NULL THEN 'En curso'
            ELSE 'Finalizada'
        END AS estado
    FROM Batalla b
    INNER JOIN Personaje p1 ON p1.id = b.personaje_1_id
    INNER JOIN TipoPersonaje t1 ON t1.id = p1.tipo_id
    INNER JOIN Personaje p2 ON p2.id = b.personaje_2_id
    INNER JOIN TipoPersonaje t2 ON t2.id = p2.tipo_id
    LEFT JOIN Personaje pg ON pg.id = b.ganador_id
    WHERE b.fecha_creacion >= p_desde
        AND b.fecha_creacion < p_hasta
    ORDER BY b.fecha_creacion DESC, b.id DESC;

    COMMIT;
END $$

DELIMITER ;
