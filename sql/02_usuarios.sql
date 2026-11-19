CREATE USER IF NOT EXISTS 'combate_admin'@'localhost'
    IDENTIFIED BY 'REEMPLAZAR_ADMIN_PASSWORD';
CREATE USER IF NOT EXISTS 'combate_invitado'@'localhost'
    IDENTIFIED BY 'REEMPLAZAR_INVITADO_PASSWORD';

GRANT ALL PRIVILEGES ON combate_api.* TO 'combate_admin'@'localhost';
GRANT SELECT ON combate_api.* TO 'combate_invitado'@'localhost';