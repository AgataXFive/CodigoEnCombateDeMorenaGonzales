namespace Aplicacion.Dominio;

public class Arquero : Personaje
{
	public int Punteria { get; private set; }

	public Arquero(string nombre, int nivel, int poder, int punteria) :
	base(nombre, nivel, poder)
	{
		if (punteria <= 0)
			throw new ArgumentOutOfRangeException(nameof(punteria), "La punteria debe ser mayor que cero.");

		Punteria = punteria;
	}

	public void PoderPunteria()
	{
		Punteria *= Nivel;
		Console.WriteLine("Poder de punteria: " + Punteria);
	}
}
