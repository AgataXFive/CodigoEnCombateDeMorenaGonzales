namespace Persistencia.Entidades;

public class Personaje
{
    public string Nombre { get; private set; }
    public int Nivel { get; private set; }
    public int Poder { get; protected set; }
    public int Vida { get; protected set; }
    public int Ataque { get; protected set; }
    public int Defensa { get; protected set; }
    public bool Derrotado => Vida <= 0;

    public Personaje(string nombre, int nivel, int poder)
        : this(nombre, nivel, poder, poder, poder, 0)
    {
    }

    public Personaje(string nombre, int nivel, int poder, int vida, int ataque, int defensa)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (nivel <= 0)
            throw new ArgumentOutOfRangeException(nameof(nivel), "El nivel debe ser mayor que cero.");
        if (poder <= 0)
            throw new ArgumentOutOfRangeException(nameof(poder), "El poder debe ser mayor que cero.");
        if (vida < 0)
            throw new ArgumentOutOfRangeException(nameof(vida), "La vida no puede ser negativa.");
        if (ataque <= 0)
            throw new ArgumentOutOfRangeException(nameof(ataque), "El ataque debe ser mayor que cero.");
        if (defensa < 0)
            throw new ArgumentOutOfRangeException(nameof(defensa), "La defensa no puede ser negativa.");

        Nombre = nombre.Trim();
        Nivel = nivel;
        Poder = poder;
        Vida = vida;
        Ataque = ataque;
        Defensa = defensa;
    }

    public virtual void Atacar(Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(objetivo);

        if (Derrotado)
            throw new InvalidOperationException($"{Nombre} está derrotado y no puede atacar.");
        if (objetivo.Derrotado)
            throw new InvalidOperationException($"{objetivo.Nombre} ya está derrotado.");

        int danio = Math.Max(1, Ataque + ObtenerBonusDeAtaque() - objetivo.Defensa - ObtenerBonusDeDefensaObjetivo(objetivo));
        objetivo.RecibirDanio(danio);
    }

    public virtual void Defenderse()
    {
        if (Derrotado)
            throw new InvalidOperationException($"{Nombre} está derrotado y no puede defenderse.");
    }

    protected virtual int ObtenerBonusDeAtaque() => 0;

    protected virtual int ObtenerBonusDeDefensaObjetivo(Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(objetivo);
        return objetivo.ObtenerBonusDeDefensa();
    }

    protected virtual int ObtenerBonusDeDefensa() => 0;

    public virtual void RecibirDanio(int cantidad)
    {
        if (cantidad <= 0)
            return;

        Vida = Math.Max(0, Vida - cantidad);
    }

    public void Mostrar()
    {
        Console.WriteLine("Nombre: " + Nombre);
        Console.WriteLine("Nivel: " + Nivel);
        Console.WriteLine("Poder: " + Poder);
        Console.WriteLine("Vida: " + Vida);
    }
}