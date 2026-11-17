using System.Collections.Generic;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

namespace GrupoF
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo;
        private QTableStorage _qStorage;
        private QTable _qTable;

        // Lista de acciones válidas
        private readonly HashSet<QAction> validActions = new();

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            QState state = BuildStateKey(currentPosition, otherPosition, _worldInfo);
            string stateKey = state.ToKey();

            UpdateValidActions(state);
            QAction bestAction = _qTable.GetBestAction(stateKey, validActions);

            CellInfo nextPosition = ApplyAction(currentPosition, bestAction);

            return nextPosition;
        }

        private QState BuildStateKey(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            return new QState(agent, other, _worldInfo);
        }

        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            return action switch
            {
                QAction.Up => _worldInfo[agentCell.x, agentCell.y + 1],
                QAction.Down => _worldInfo[agentCell.x, agentCell.y - 1],
                QAction.Right => _worldInfo[agentCell.x + 1, agentCell.y],
                QAction.Left => _worldInfo[agentCell.x - 1, agentCell.y],
                _ => _worldInfo[agentCell.x, agentCell.y],
            };
        }

        private HashSet<QAction> UpdateValidActions(QState state)
        {
            validActions.Clear();

            validActions.Add(QAction.Stay);

            if (state.CanMoveUp)
            {
                validActions.Add(QAction.Up);
            }

            if (state.CanMoveDown)
            {
                validActions.Add(QAction.Down);
            }

            if (state.CanMoveLeft)
            {
                validActions.Add(QAction.Left);
            }

            if (state.CanMoveRight)
            {
                validActions.Add(QAction.Right);
            }

            return validActions;
        }
    }
}