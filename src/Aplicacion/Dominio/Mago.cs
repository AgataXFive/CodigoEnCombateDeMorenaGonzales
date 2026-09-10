namespace Aplicacion.Dominio;

public class Mago : Personaje
{
    public int Mana { get; private set; }

    public Mago(string nombre, int nivel, int poder, int mana):
    base(nombre, nivel, poder)
    {
        if (mana <= 0)
            throw new ArgumentOutOfRangeException(nameof(mana), "El mana debe ser mayor que cero.");

        Mana = mana;
    }

    public void PoderMagico()
    {
        Mana += Poder;
        Console.WriteLine("Poder magico: " + Mana);
    }
}