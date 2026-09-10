namespace Aplicacion.Dominio;

public class Personaje
{
    public string Nombre { get; private set; }
    public int Nivel { get; private set; }
    public int Poder { get; private set; }

    public Personaje(string nombre, int nivel, int poder)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (nivel <= 0)
            throw new ArgumentOutOfRangeException(nameof(nivel), "El nivel debe ser mayor que cero.");
        if (poder <= 0)
            throw new ArgumentOutOfRangeException(nameof(poder), "El poder debe ser mayor que cero.");

        Nombre = nombre.Trim();
        Nivel = nivel;
        Poder = poder;
    }

    public void Mostrar()
    {
        Console.WriteLine("Nombre: " + Nombre);
        Console.WriteLine("Nivel: " + Nivel);
        Console.WriteLine("Poder: " + Poder);
    }
}