namespace Aplicacion.Dominio;

public class Guerrero : Personaje
{
    public int Fuerza { get; private set; }

    public Guerrero(string nombre, int nivel, int poder, int fuerza) :
    base(nombre, nivel, poder)
    {
        if (fuerza <= 0)
            throw new ArgumentOutOfRangeException(nameof(fuerza), "La fuerza debe ser mayor que cero.");

        Fuerza = fuerza;
    }

    public void PoderFuerza()
    {
        Fuerza *= Nivel;
        Console.WriteLine("Poder de fuerza: " + Fuerza);
    }
}