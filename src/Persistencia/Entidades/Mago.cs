namespace Persistencia.Entidades;

public class Mago : Personaje
{
    public int Mana { get; private set; }

    public Mago(string nombre, int nivel, int poder, int mana):
    base(nombre, nivel, poder)
    {
        if (mana < 0)
            throw new ArgumentOutOfRangeException(nameof(mana), "El mana no puede ser negativo.");

        Mana = mana;
        Vida = Math.Max(1, poder);
        Ataque = Math.Max(1, poder / 2);
        Defensa = 1;
    }

    public override void Atacar(Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(objetivo);

        if (Derrotado)
            throw new InvalidOperationException($"{Nombre} está derrotado y no puede atacar.");
        if (Mana <= 0)
            throw new InvalidOperationException($"{Nombre} no tiene mana suficiente para lanzar un hechizo.");

        int danio = Math.Max(1, Ataque + Poder - objetivo.Defensa);
        objetivo.RecibirDanio(danio);
        Mana = Math.Max(0, Mana - 1);
    }

    protected override int ObtenerBonusDeAtaque() => Poder;
    protected override int ObtenerBonusDeDefensa() => 1;

    public void PoderMagico()
    {
        Mana += Poder;
        Console.WriteLine("Poder magico: " + Mana);
    }
}