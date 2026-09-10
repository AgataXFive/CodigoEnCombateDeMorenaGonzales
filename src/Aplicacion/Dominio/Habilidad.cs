namespace Aplicacion.Dominio;

public class Habilidad
{
    public string Movimiento { get; private set; }

    public Habilidad(string movimiento)
    {
        if (string.IsNullOrWhiteSpace(movimiento))
            throw new ArgumentException("El movimiento es obligatorio.", nameof(movimiento));

        Movimiento = movimiento.Trim();
    }
}