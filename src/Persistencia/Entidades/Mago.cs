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

    public Mago(string nombre, int nivel, int poder, int mana, int vida, int ataque, int defensa)
        : base(nombre, nivel, poder, vida, ataque, defensa)
    {
        if (mana < 0)
            throw new ArgumentOutOfRangeException(nameof(mana), "El mana no puede ser negativo.");
        Mana = mana;
    }

    public override void Atacar(Personaje objetivo)
    {
        ValidarAccion(objetivo);
        if (Mana <= 0)
            throw new InvalidOperationException($"{Nombre} no tiene mana suficiente para lanzar un hechizo.");

        int danio = Math.Max(1, Ataque + Poder - objetivo.Defensa);
        objetivo.RecibirDanio(danio);
        Mana = Math.Max(0, Mana - 1);
    }

    public override void UsarHabilidad(Personaje objetivo)
    {
        ValidarAccion(objetivo);
        if (Mana <= 0)
            throw new InvalidOperationException($"{Nombre} no tiene mana suficiente para usar su habilidad.");

        objetivo.RecibirDanio(Math.Max(1, Ataque + Poder * 2 - objetivo.Defensa));
        Mana = Math.Max(0, Mana - 3);
    }

    protected override int ObtenerBonusDeAtaque() => Poder;
    protected override int ObtenerBonusDeDefensa() => 1;

    public void PoderMagico()
    {
        Mana += Poder;
        Console.WriteLine("Poder magico: " + Mana);
    }
}