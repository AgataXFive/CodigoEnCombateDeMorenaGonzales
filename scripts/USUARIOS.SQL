USE juego_combate_morena;

-- Reemplazar ambas contrasenas por claves seguras antes de ejecutar.
-- Estos usuarios solo aceptan conexiones desde el mismo servidor.
CREATE USER IF NOT EXISTS 'administrador'@'localhost'
    IDENTIFIED BY 'CAMBIAR_ADMIN_PASSWORD_ANTES_DE_EJECUTAR_2026!';
CREATE USER IF NOT EXISTS 'desarrollo'@'localhost'
    IDENTIFIED BY 'CAMBIAR_DESARROLLO_PASSWORD_ANTES_DE_EJECUTAR_2026!';

REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'administrador'@'localhost';
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'desarrollo'@'localhost';

-- El administrador accede a todas las bases, sin permiso para delegar privilegios.
GRANT ALL PRIVILEGES ON *.* TO 'administrador'@'localhost';

-- Desarrollo queda limitado al esquema del proyecto.
GRANT ALL PRIVILEGES ON juego_combate_morena.* TO 'desarrollo'@'localhost';
