-- Ejecutar desde la raiz del repositorio:
-- mysql -u root -p < scripts/install.sql
-- ATENCION: script1.sql elimina y vuelve a crear juego_combate_morena.
-- Respaldar la base antes de ejecutar este instalador.
SOURCE scripts/script1.sql
SOURCE scripts/script2.sql
SOURCE scripts/04_api_turnos.sql
SOURCE scripts/SP.SQL
SOURCE scripts/USUARIOS.SQL