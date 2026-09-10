namespace Aplicacion.Dominio;

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
    }
}