using Aplicacion.Interfaces;
using Persistencia.Entidades;

namespace Aplicacion.Servicios;

public class BatallaServicio : IBatallaServicio
{
    public Personaje DeterminarGanador(Personaje combatiente1, Personaje combatiente2)
    {
        ArgumentNullException.ThrowIfNull(combatiente1);
        ArgumentNullException.ThrowIfNull(combatiente2);

        return combatiente1.Poder >= combatiente2.Poder
            ? combatiente1
            : combatiente2;
    }

    public Personaje? DeterminarGanadorPorVida(Personaje combatiente1, Personaje combatiente2)
    {
        ArgumentNullException.ThrowIfNull(combatiente1);
        ArgumentNullException.ThrowIfNull(combatiente2);

        if (combatiente1.Derrotado && combatiente2.Derrotado)
            return null;

        if (combatiente1.Derrotado)
            return combatiente2;

        if (combatiente2.Derrotado)
            return combatiente1;

        return combatiente1.Vida >= combatiente2.Vida
            ? combatiente1
            : combatiente2;
    }
}
