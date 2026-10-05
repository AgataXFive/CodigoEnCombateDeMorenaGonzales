using Aplicacion.Servicios;
using Persistencia.Entidades;
using Persistencia.Repositorios;

var personajeRepositorio = new PersonajeRepositorio();
var habilidadRepositorio = new HabilidadRepositorio();

var personajeServicio = new PersonajeServicio(personajeRepositorio);
var habilidadServicio = new HabilidadServicio(habilidadRepositorio);
var batallaServicio = new BatallaServicio();

var guerrero = new Guerrero("Aragorn", 3, 20, 12);
var mago = new Mago("Gandalf", 4, 18, 15);
var arquero = new Arquero("Legolas", 3, 17, 10);
var asesino = new Asesino("Lara", 5, 19, "Ataque furtivo", 14);

var habilidad = new Habilidad("Bola de fuego");

personajeServicio.Registrar(guerrero);
personajeServicio.Registrar(mago);
personajeServicio.Registrar(arquero);
personajeServicio.Registrar(asesino);
habilidadServicio.Registrar(habilidad);

Console.WriteLine("Personajes registrados:");
foreach (var personaje in personajeServicio.ObtenerTodos())
{
    Console.WriteLine($"- {personaje.Nombre} ({personaje.GetType().Name})");
}

Console.WriteLine();
Console.WriteLine("Batalla demo:");
Console.WriteLine($"Ganador por poder: {batallaServicio.DeterminarGanador(guerrero, mago).Nombre}");

mago.Atacar(guerrero);
Console.WriteLine($"Después del ataque, {guerrero.Nombre} tiene {guerrero.Vida} de vida.");
Console.WriteLine($"Habilidad disponible: {asesino.UsarHabilidad()}");
Console.WriteLine($"Habilidad almacenada: {habilidadServicio.ObtenerPorMovimiento("Bola de fuego")?.Movimiento}");
