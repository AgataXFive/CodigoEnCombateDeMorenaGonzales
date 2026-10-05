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

    public override void Atacar(Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(objetivo);

        if (Derrotado)
            throw new InvalidOperationException($"{Nombre} está derrotado y no puede atacar.");

        int danio = Math.Max(1, Ataque + Punteria - objetivo.Defensa);
        objetivo.RecibirDanio(danio);
    }

    protected override int ObtenerBonusDeAtaque() => Punteria;
    protected override int ObtenerBonusDeDefensa() => 1;

	public void PoderPunteria()
	{
		Punteria *= Nivel;
		Console.WriteLine("Poder de punteria: " + Punteria);
	}
}
