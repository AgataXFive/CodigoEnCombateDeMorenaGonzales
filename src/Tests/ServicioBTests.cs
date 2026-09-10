using Aplicacion.Dominio;

namespace Tests;

public class ServicioBTests
{
	[Fact]
	public void Personajes_EjecutanSusPoderes()
	{
		var guerrero = new Guerrero("Guerrero", 2, 10, 5);
		var mago = new Mago("Mago", 2, 10, 5);
		var arquero = new Arquero("Arquero", 2, 10, 5);

		guerrero.PoderFuerza();
		mago.PoderMagico();
		arquero.PoderPunteria();

		Assert.Equal(10, guerrero.Fuerza);
		Assert.Equal(15, mago.Mana);
		Assert.Equal(10, arquero.Punteria);
	}

	[Fact]
	public void Asesino_ConservaSuHabilidad()
	{
		var asesino = new Asesino("Asesino", 3, 12, "Ataque furtivo", 8);

		Assert.Equal("Ataque furtivo", asesino.Habilidad.Movimiento);
	}
}
