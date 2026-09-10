namespace Aplicacion.Dominio;

public class Batalla
{
    public Personaje Combatiente1 { get; private set; }
    public Personaje Combatiente2 { get; private set; }

    public Batalla(Personaje combatiente1, Personaje combatiente2)
    {
        ArgumentNullException.ThrowIfNull(combatiente1);
        ArgumentNullException.ThrowIfNull(combatiente2);

        Combatiente1 = combatiente1;
        Combatiente2 = combatiente2;
    }

    public Personaje DeterminarGanador()
    {
        return Combatiente1.Poder >= Combatiente2.Poder
            ? Combatiente1
            : Combatiente2;
    }
}