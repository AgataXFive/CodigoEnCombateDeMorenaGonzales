using Persistencia.Entidades;

namespace Tests;

public class ServicioATests
{
	[Fact]
	public void Personaje_RechazaNombreVacio()
	{
		Assert.Throws<ArgumentException>(() => new Personaje(" ", 1, 10));
	}

	[Fact]
	public void Personaje_RechazaValoresNoPositivos()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new Personaje("Aragorn", 0, 10));
		Assert.Throws<ArgumentOutOfRangeException>(() => new Personaje("Aragorn", 1, 0));
	}

	[Fact]
	public void Batalla_DevuelveElPersonajeConMayorPoder()
	{
		var guerrero = new Guerrero("Guerrero", 2, 20, 10);
		var mago = new Mago("Mago", 2, 15, 10);
		var batalla = new Batalla(guerrero, mago);

		Assert.Same(guerrero, batalla.DeterminarGanador());
	}

	[Fact]
	public void Personaje_AtacaYReduceLaVidaDelObjetivo()
	{
		var guerrero = new Guerrero("Guerrero", 2, 20, 10);
		var mago = new Mago("Mago", 2, 15, 10);

		guerrero.Atacar(mago);

		Assert.True(mago.Vida < 15);
	}

	[Fact]
	public void Personaje_Derrotado_NoPuedeAtacar()
	{
		var guerrero = new Guerrero("Guerrero", 2, 20, 10);
		var mago = new Mago("Mago", 2, 15, 10);

		mago.RecibirDanio(999);

		Assert.True(mago.Derrotado);
		Assert.Throws<InvalidOperationException>(() => mago.Atacar(guerrero));
	}
}
