using NavigationDJIA.World;
using UnityEngine;

/// <summary>
/// TODO(alumno):
/// Define el "estado" que usará la Tabla Q para identificar cada situación del agente.
/// 
/// El estado debe contener toda la información necesaria para que el agente pueda
/// tomar decisiones informadas. Tú decides qué características incluir según lo
/// que consideres relevante para resolver el problema.
/// 
/// Ejemplos típicos de información que puede formar un estado:
///   - Posición del agente en la grid.
///   - Posición del otro personaje (enemigo).
///   - Distancia relativa entre agente y enemigo.
///   - Si hay muros en direcciones cercanas.
///   - Cualquier otro dato que consideres útil.
/// 
/// En este ejercicio te damos un ejemplo simple basado únicamente en las posiciones
/// del agente y del oponente. Puedes usarlo tal cual o ampliarlo.
/// 
/// IMPORTANTE: 
///  El estado debe poder convertirse a una clave única (string) mediante ToKey(),
///  ya que esa clave se usará como índice en la TablaQ y en el archivo CSV.
/// </summary>

namespace GrupoF
{
    public sealed class QState
    {

        // La distancia entre el agente y el jugador
        public float Distance { get; }

        // A que direcciones se puede mover actualmente
        public bool CanMoveUp { get; }
        public bool CanMoveDown { get; }
        public bool CanMoveRight { get; }
        public bool CanMoveLeft { get; }

        // Almacenamos en que dirección relativa se encuentra el jugador
        private int XDirection { get; }
        private int YDirection { get; }

        public QState(CellInfo agent, CellInfo other, WorldInfo worldInfo)
        {
            Distance = agent.Distance(other, CellInfo.DistanceType.Manhattan);

            CanMoveUp = worldInfo[agent.x, agent.y + 1].Walkable;
            CanMoveDown = worldInfo[agent.x, agent.y - 1].Walkable;
            CanMoveRight = worldInfo[agent.x + 1, agent.y].Walkable;
            CanMoveLeft = worldInfo[agent.x - 1, agent.y].Walkable;

            XDirection = other.x - agent.x;
            YDirection = other.y - agent.y;
        }

        public string ToKey()
        {
            return $"{CanMoveUp},{CanMoveDown},{CanMoveRight},{CanMoveLeft}"
            + $"|{XDirection},{YDirection}";
        }
    }
}