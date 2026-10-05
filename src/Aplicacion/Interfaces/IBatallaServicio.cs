using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

public interface IBatallaServicio
{
    Personaje DeterminarGanador(Personaje combatiente1, Personaje combatiente2);
    Personaje? DeterminarGanadorPorVida(Personaje combatiente1, Personaje combatiente2);
}
