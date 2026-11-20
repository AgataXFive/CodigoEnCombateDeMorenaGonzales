# Guia de uso de Combate por API

## 1. Descripcion

Combate por API es una aplicacion .NET 10 que administra personajes y combates almacenados en MySQL. Expone operaciones HTTP mediante una API minimal y permite consultar personajes, iniciar combates y ejecutar turnos.

La aplicacion utiliza dos cuentas de base de datos:

- **Lectura**: ejecuta consultas `SELECT`.
- **Administrador**: ejecuta inserciones, actualizaciones y eliminaciones.

Las cadenas de conexion se reciben por variables de entorno y no deben escribirse directamente en los archivos versionados.

## 2. Requisitos

- Windows, Linux o macOS.
- .NET SDK 10.
- MySQL 8 o compatible.
- Cliente `mysql` disponible en la terminal.
- Visual Studio Code, Visual Studio o una terminal.

Verifique las herramientas:

```sh
dotnet --version
mysql --version
```

## 3. Preparar la base de datos

La API esta preparada para trabajar con el esquema legado `juego_combate_morena`.

Desde la raiz del repositorio, cree y cargue la base:

```sh
mysql -u root -p < scripts/script1.sql
mysql -u root -p < scripts/script2.sql
mysql -u root -p < scripts/04_api_turnos.sql
mysql -u root -p < scripts/SP.SQL
```

Advertencia: `scripts/script1.sql` elimina `juego_combate_morena` si ya existe. Realice una copia de seguridad antes de ejecutarlo.

Para ejecutar todos los scripts desde un instalador:

```sh
mysql -u root -p < scripts/install.sql
```

El instalador tambien llama a `scripts/USUARIOS.SQL`. Antes de usarlo, revise y reemplace las claves de ejemplo en `scripts/02_usuarios.sql` y `scripts/USUARIOS.SQL`.

### Usuarios de MySQL

El usuario invitado solo necesita permisos de lectura. El usuario administrador necesita permisos de escritura sobre la base.

Ejemplo con las cuentas incluidas en `scripts/02_usuarios.sql`:

```sql
CREATE USER IF NOT EXISTS 'combate_admin'@'localhost'
    IDENTIFIED BY 'CLAVE_ADMIN_PROPIA';
CREATE USER IF NOT EXISTS 'combate_invitado'@'localhost'
    IDENTIFIED BY 'CLAVE_LECTURA_PROPIA';

GRANT ALL PRIVILEGES ON combate_api.* TO 'combate_admin'@'localhost';
GRANT SELECT ON combate_api.* TO 'combate_invitado'@'localhost';
```

Para el esquema legado, el instalador principal usa las cuentas configuradas en `scripts/USUARIOS.SQL`. Ajuste sus claves antes de ejecutarlo.

## 4. Configurar la API

En PowerShell:

```powershell
$env:COMBATE_API_READ_CONNECTION = "Server=localhost;Database=juego_combate_morena;User ID=invitado;Password=CLAVE_LECTURA"
$env:COMBATE_API_ADMIN_CONNECTION = "Server=localhost;Database=juego_combate_morena;User ID=administrador;Password=CLAVE_ADMIN"
$env:COMBATE_API_CORS_ORIGINS = "http://localhost:5173,http://localhost:3000"
```

En Bash:

```sh
export COMBATE_API_READ_CONNECTION='Server=localhost;Database=juego_combate_morena;User ID=invitado;Password=CLAVE_LECTURA'
export COMBATE_API_ADMIN_CONNECTION='Server=localhost;Database=juego_combate_morena;User ID=administrador;Password=CLAVE_ADMIN'
export COMBATE_API_CORS_ORIGINS='http://localhost:5173,http://localhost:3000'
```

`COMBATE_API_CORS_ORIGINS` es opcional. Cuando no se configura, no se habilita acceso CORS para orígenes externos. En desarrollo se aplican los dos orígenes locales definidos en `src/API/appsettings.Development.json`.

## 5. Compilar y ejecutar

Desde la raiz del repositorio:

```sh
dotnet build src/Proyecto.sln
dotnet test src/Tests/Tests.csproj
dotnet run --project src/API/API.csproj
```

La terminal mostrara la URL asignada por Kestrel. Por defecto puede usar una direccion similar a `http://localhost:5093`.

Abra la documentacion interactiva:

```text
http://localhost:5093/swagger
```

La especificacion OpenAPI esta disponible en:

```text
http://localhost:5093/swagger/v1/swagger.json
```

Si Kestrel informa otro puerto, reemplaze `5093` por el puerto mostrado.

## 6. Comprobar la conexion

```sh
curl -i http://localhost:5093/api/estado/base-datos
```

Respuesta esperada:

```json
{"estado":"conectada","comprobacion":1}
```

Un `503` indica que faltan las variables de entorno o que MySQL rechazo la conexion de lectura.

## 7. Ejemplos de uso

### Crear personajes

```sh
curl -X POST http://localhost:5093/api/personajes \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Gimli","tipo":"Guerrero"}'

curl -X POST http://localhost:5093/api/personajes \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Merlin","tipo":"Mago"}'
```

Los tipos validos son `Guerrero`, `Mago`, `Arquero` y `Asesino`.

### Listar personajes

```sh
curl http://localhost:5093/api/personajes
```

### Iniciar un combate

Reemplace los identificadores por los personajes creados:

```sh
curl -X POST http://localhost:5093/api/combates \
  -H "Content-Type: application/json" \
  -d '{"personaje1Id":1,"personaje2Id":2}'
```

### Ejecutar un turno

```sh
curl -X POST http://localhost:5093/api/combates/1/turnos \
  -H "Content-Type: application/json" \
  -d '{"atacanteId":1,"accion":"atacar"}'
```

Acciones disponibles:

- `atacar`: ataque normal.
- `habilidad`: utiliza la habilidad correspondiente al tipo de personaje.

Para consultar el estado completo:

```sh
curl http://localhost:5093/api/combates/1
curl http://localhost:5093/api/combates/1/turnos
```

### Modificar notas y revertir turnos

```sh
curl -X PUT http://localhost:5093/api/combates/1/turnos/1 \
  -H "Content-Type: application/json" \
  -d '{"nota":"Golpe decisivo"}'

curl -X DELETE http://localhost:5093/api/combates/1/turnos/1
```

Solo se puede eliminar el ultimo turno. La operacion elimina el turno y restaura las vidas, el estado y el ganador anteriores.

## 8. Endpoints

| Metodo | Ruta | Uso |
|---|---|---|
| GET | `/api/estado/base-datos` | Verificar MySQL |
| GET | `/api/personajes` | Listar personajes |
| GET | `/api/personajes/{id}` | Obtener personaje |
| GET | `/api/personajes/{id}/combates` | Combates de un personaje |
| POST | `/api/personajes` | Crear personaje |
| PUT | `/api/personajes/{id}` | Cambiar nombre |
| DELETE | `/api/personajes/{id}` | Eliminar personaje sin combates |
| GET | `/api/combates` | Listar combates |
| POST | `/api/combates` | Crear combate |
| GET | `/api/combates/{id}` | Obtener detalle y turnos |
| PUT | `/api/combates/{id}` | Cambiar participantes antes del primer turno |
| DELETE | `/api/combates/{id}` | Eliminar combate sin turnos |
| GET | `/api/combates/{id}/turnos` | Listar turnos |
| POST | `/api/combates/{id}/turnos` | Ejecutar turno |
| PUT | `/api/combates/{id}/turnos/{turnoId}` | Modificar nota |
| DELETE | `/api/combates/{id}/turnos/{turnoId}` | Revertir ultimo turno |

## 9. Reglas importantes

- Los dos participantes de un combate deben ser distintos.
- Un personaje con batallas existentes no puede eliminarse.
- Un combate sin turnos puede modificarse o eliminarse.
- Un combate con turnos no permite cambiar sus participantes.
- Solo se puede eliminar el ultimo turno de un combate.
- Las operaciones de escritura usan una transaccion cuando afectan varios registros.

## 10. Solucion de problemas

### La API no inicia por falta de variables

Configure `COMBATE_API_READ_CONNECTION` y `COMBATE_API_ADMIN_CONNECTION` en la misma terminal donde ejecuta `dotnet run`.

### La base rechaza la conexion

Compruebe que el servicio MySQL este iniciado, que el usuario tenga acceso desde `localhost` y que la contraseña sea correcta.

### Swagger no aparece

Abra la URL que imprime `dotnet run`, agregando `/swagger`. Swagger no depende de un puerto fijo.

### Error al crear o modificar datos

Ejecute `scripts/04_api_turnos.sql` si trabajan sobre `juego_combate_morena` y verifique que la cuenta administradora tenga permisos `ALTER`, `UPDATE`, `CREATE` y `INSERT`.

### Error CORS desde el frontend

Agregue el origen exacto del frontend a `COMBATE_API_CORS_ORIGINS`. Incluya protocolo, host y puerto, por ejemplo `http://localhost:5173`.
