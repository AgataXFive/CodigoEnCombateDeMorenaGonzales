namespace Persistencia.Entidades;

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

    public Personaje? DeterminarGanadorPorVida()
    {
        if (Combatiente1.Derrotado && Combatiente2.Derrotado)
            return null;

        if (Combatiente1.Derrotado)
            return Combatiente2;

        if (Combatiente2.Derrotado)
            return Combatiente1;

        return Combatiente1.Vida >= Combatiente2.Vida
            ? Combatiente1
            : Combatiente2;
    }
}