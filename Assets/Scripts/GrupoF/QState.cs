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

        // Contabilizamos cuantas salidas tiene la casilla actual y cada posible dirección (para evitar callejones sin salida)
        public int CurrentExits { get; }
        public int ExitsUp { get; }
        public int ExitsDown { get; }
        public int ExitsRight { get; }
        public int ExitsLeft { get; }

        public QState(CellInfo agentCell, CellInfo otherCell, WorldInfo worldInfo)
        {
            Distance = agentCell.Distance(otherCell, CellInfo.DistanceType.Manhattan);

            CellInfo upCell = worldInfo[agentCell.x, agentCell.y + 1];
            CellInfo downCell = worldInfo[agentCell.x, agentCell.y - 1];
            CellInfo leftCell = worldInfo[agentCell.x - 1, agentCell.y];
            CellInfo rightCell = worldInfo[agentCell.x + 1, agentCell.y];

            CanMoveUp = upCell.Walkable;
            CanMoveDown = downCell.Walkable;
            CanMoveLeft = leftCell.Walkable;
            CanMoveRight = rightCell.Walkable;

            XDirection = otherCell.x - agentCell.x;
            YDirection = otherCell.y - agentCell.y;

            CurrentExits = CountCellExits(worldInfo, agentCell);

            ExitsUp = CountCellExits(worldInfo, upCell);
            ExitsDown = CountCellExits(worldInfo, downCell);
            ExitsRight = CountCellExits(worldInfo, rightCell);
            ExitsLeft = CountCellExits(worldInfo, leftCell);
        }

        /// <summary>
        /// Se comprueba cuantas salidas tiene cierta casilla disponibles (es decir, casillas por las que se pueda andar)
        /// </summary>
        /// <param name="worldInfo">Mapa del mundo que contiene todas las casillas</param>
        /// <param name="cell">Casilla de la cual se quiere comprobar su cantidad de salidas</param>
        /// <returns></returns>
        private int CountCellExits(WorldInfo worldInfo, CellInfo cell)
        {
            if (!cell.Walkable)
                return 0;

            int exits = 0;

            if (worldInfo[cell.x, cell.y + 1].Walkable)
            {
                exits++;
            }

            if (worldInfo[cell.x, cell.y - 1].Walkable)
            {
                exits++;
            }

            if (worldInfo[cell.x + 1, cell.y].Walkable)
            {
                exits++;
            }

            if (worldInfo[cell.x - 1, cell.y].Walkable)
            {
                exits++;
            }

            return exits;
        }

        public string ToKey()
        {
            return $"{CanMoveUp},{CanMoveDown},{CanMoveRight},{CanMoveLeft}"
                   + $"|{XDirection},{YDirection}"
                   + $"|{ExitsUp},{ExitsDown},{ExitsRight},{ExitsLeft}";
        }
    }
}