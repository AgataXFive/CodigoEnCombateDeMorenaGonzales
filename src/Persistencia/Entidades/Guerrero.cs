namespace Persistencia.Entidades;

public class Guerrero : Personaje
{
    public int Fuerza { get; private set; }

    public Guerrero(string nombre, int nivel, int poder, int fuerza) :
    base(nombre, nivel, poder)
    {
        if (fuerza <= 0)
            throw new ArgumentOutOfRangeException(nameof(fuerza), "La fuerza debe ser mayor que cero.");

        Fuerza = fuerza;
        Vida = Math.Max(1, poder);
        Ataque = Math.Max(1, poder / 2);
        Defensa = Math.Max(0, fuerza / 2);
    }

    public Guerrero(string nombre, int nivel, int poder, int fuerza, int vida, int ataque, int defensa) :
        base(nombre, nivel, poder, vida, ataque, defensa)
    {
        if (fuerza <= 0)
            throw new ArgumentOutOfRangeException(nameof(fuerza), "La fuerza debe ser mayor que cero.");

        Fuerza = fuerza;
    }

    public override void Atacar(Personaje objetivo)
    {
        if (objetivo is null)
            throw new ArgumentNullException(nameof(objetivo));

        int danio = Math.Max(1, Ataque + Fuerza - objetivo.Defensa);
        objetivo.RecibirDanio(danio);
    }

    public override void Defenderse()
    {
        base.Defenderse();
        Defensa += Fuerza / 2;
    }

    protected override int ObtenerBonusDeAtaque() => Fuerza;
    protected override int ObtenerBonusDeDefensa() => Fuerza / 2;

    public void PoderFuerza()
    {
        Fuerza *= Nivel;
        Console.WriteLine("Poder de fuerza: " + Fuerza);
    }
}