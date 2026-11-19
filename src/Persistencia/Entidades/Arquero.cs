namespace Persistencia.Entidades;

public class Arquero : Personaje
{
	public int Punteria { get; private set; }

	public Arquero(string nombre, int nivel, int poder, int punteria) :
	base(nombre, nivel, poder)
	{
		if (punteria <= 0)
			throw new ArgumentOutOfRangeException(nameof(punteria), "La punteria debe ser mayor que cero.");

		Punteria = punteria;
        Vida = Math.Max(1, poder);
        Ataque = Math.Max(1, poder / 2);
        Defensa = 1;
	}

	public Arquero(string nombre, int nivel, int poder, int punteria, int vida, int ataque, int defensa)
		: base(nombre, nivel, poder, vida, ataque, defensa)
	{
		if (punteria <= 0)
			throw new ArgumentOutOfRangeException(nameof(punteria), "La punteria debe ser mayor que cero.");
		Punteria = punteria;
	}

    public override void Atacar(Personaje objetivo)
    {
		ValidarAccion(objetivo);

        int danio = Math.Max(1, Ataque + Punteria - objetivo.Defensa);
        objetivo.RecibirDanio(danio);
    }

	public override void UsarHabilidad(Personaje objetivo)
	{
		ValidarAccion(objetivo);
		objetivo.RecibirDanio(Math.Max(1, Ataque + Punteria * 2 - objetivo.Defensa));
	}

    protected override int ObtenerBonusDeAtaque() => Punteria;
    protected override int ObtenerBonusDeDefensa() => 1;

	public void PoderPunteria()
	{
		Punteria *= Nivel;
		Console.WriteLine("Poder de punteria: " + Punteria);
	}
}
