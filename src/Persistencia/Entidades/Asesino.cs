namespace Persistencia.Entidades;

public class Asesino : Personaje
{
    public int Sigilo { get; private set; }
    public Habilidad Habilidad { get; private set; }

    public Asesino(string nombre, int nivel, int poder, string movimiento, int sigilo) :
    base(nombre, nivel, poder)
    {
        if (sigilo <= 0)
            throw new ArgumentOutOfRangeException(nameof(sigilo), "El sigilo debe ser mayor que cero.");

        Sigilo = sigilo;
        Habilidad = new Habilidad(movimiento);
        Vida = Math.Max(1, poder);
        Ataque = Math.Max(1, poder / 2);
        Defensa = 1;
    }

    public Asesino(string nombre, int nivel, int poder, int sigilo, int vida, int ataque, int defensa)
        : base(nombre, nivel, poder, vida, ataque, defensa)
    {
        if (sigilo <= 0)
            throw new ArgumentOutOfRangeException(nameof(sigilo), "El sigilo debe ser mayor que cero.");
        Sigilo = sigilo;
        Habilidad = new Habilidad("Ataque furtivo");
    }

    public override void Atacar(Personaje objetivo)
    {
        ValidarAccion(objetivo);

        int danio = Math.Max(1, Ataque + Sigilo + Poder - objetivo.Defensa);
        objetivo.RecibirDanio(danio);
    }

    public override void UsarHabilidad(Personaje objetivo)
    {
        ValidarAccion(objetivo);
        objetivo.RecibirDanio(Math.Max(1, Ataque + Sigilo * 2 + Poder - objetivo.Defensa));
    }

    protected override int ObtenerBonusDeAtaque() => Sigilo;
    protected override int ObtenerBonusDeDefensa() => 1;

    public string UsarHabilidad()
    {
        if (Derrotado)
            throw new InvalidOperationException($"{Nombre} está derrotado y no puede usar habilidades.");

        return Habilidad.Movimiento;
    }
}