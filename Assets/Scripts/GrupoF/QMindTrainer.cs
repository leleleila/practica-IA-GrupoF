using System;
using System.Collections.Generic;
using System.Linq;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

namespace GrupoF
{
    public class QMindTrainer : IQMindTrainer
    {
        private QMindTrainerParams _params;
        private WorldInfo _worldInfo;
        INavigationAlgorithm _navigationAlgorithm;

        private QTableStorage _qStorage;
        private QTable _qTable;

        private CellInfo _agentPosition;
        private CellInfo _otherPosition;

        private float _return;
        private float _returnAveraged;
        private readonly System.Random _random = new();

        #region IQMindTrainer implementation

        public CellInfo AgentPosition => _agentPosition;
        public CellInfo OtherPosition => _otherPosition;

        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }

        public float Return => _return;
        public float ReturnAveraged => _returnAveraged;

        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        #endregion

        // Lista de acciones válidas
        private readonly HashSet<QAction> validActions = new();

        // Distancia del estado anterior
        private float previousDistance = -1;

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _params = qMindTrainerParams;
            _worldInfo = worldInfo;

            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);

            CurrentEpisode = 0;
            StartNewEpisode();
        }

        private void StartNewEpisode()
        {
            CurrentEpisode++;
            CurrentStep = 0;
            _return = 0f;
            _returnAveraged = 0f;

            previousDistance = -1;

            _agentPosition = _worldInfo.RandomCell();
            _otherPosition = _worldInfo.RandomCell();

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void EndEpisode()
        {
            _qTable.SaveToCsv();

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);

            if (_params.episodes > 0 && CurrentEpisode >= _params.episodes)
            {
                return;
            }

            StartNewEpisode();
        }

        public void DoStep(bool train)
        {
            // Estado actual del agente
            QState state = BuildStateKey(_agentPosition, _otherPosition, _worldInfo);
            string stateKey = state.ToKey();

            // Seleciona la acción a realizar
            UpdateValidActions(state);
            QAction action = ChooseAction(state, train);

            // Nuevos estados del agente y del oponente
            CellInfo newAgentPos = ApplyAction(_agentPosition, action);
            CellInfo newOtherPos = MoveOpponent(_otherPosition, newAgentPos.Walkable ? newAgentPos : _agentPosition);

            // Nuevo estado del agente
            QState nextState = BuildStateKey(newAgentPos, newOtherPos, _worldInfo);
            string nextStateKey = nextState.ToKey();

            // Calcula la recompensa
            float reward = ComputeReward(newAgentPos, newOtherPos, nextState);

            bool isTerminalState = IsTerminalState(_agentPosition, _otherPosition);
            if (train)
            {
                UpdateValidActions(nextState);
                UpdateQ(stateKey, action, reward, nextStateKey, isTerminalState);
            }

            // Actualiza las posiciones
            _agentPosition = newAgentPos;
            _otherPosition = newOtherPos;

            // Actualizamos estadísticas de recompensas
            CurrentStep++;
            _return += reward;
            _returnAveraged = (_returnAveraged * (CurrentStep - 1) + reward) / CurrentStep;

            // Comprobación de si estamos en el fin de episodio
            if (isTerminalState)
            {
                EndEpisode();
            }
        }

        #region Parte a implementar por el alumno

        private QState BuildStateKey(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            return new QState(agent, other, _worldInfo);
        }

        /// <summary>
        /// Ejemplo orientativo:
        ///    - Si train == false, puedes usar la mejor acción.
        ///    - Si train == true, con probabilidad epsilon elegir acción aleatoria,
        ///      y con probabilidad 1-epsilon la mejor según _qTable.GetBestAction(stateKey).
        /// </summary>
        private QAction ChooseAction(QState state, bool train)
        {
            // En exploración:
            if (train && _random.NextDouble() < _params.epsilon)
            {
                // Obtenemos una acción aleatoria entre las acciones válidas
                return validActions.ToList()[_random.Next(validActions.Count)];
            }
            // En explotación:
            else 
            {
                return _qTable.GetBestAction(state.ToKey(), validActions);
            }
        }

        /// <summary>
        /// Actualiza el conjunto de acciones válidas para el estado proporcionado, asegurandose de que, por ejemplo, solo pueda realizar
        /// la acción de 'Up' si en el estado, se permite el movimiento hacia arriba (en caso de que sea límite del mapa o un muro, no 
        /// permitirá el movimiento)
        /// </summary>
        /// <param name="state">Estado actual del agente</param>
        /// <returns></returns>
        private HashSet<QAction> UpdateValidActions(QState state)
        {
            validActions.Clear();

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

            validActions.Add(QAction.Stay);

            return validActions;
        }

        /// <summary>
        /// Actualización de Q-Learning:
        /// Q(s,a) = (1 - alpha) * Q(s,a) + alpha * (reward + gamma * max_a' Q(s',a')).
        /// Usa _qTable.GetQ, _qTable.SetQ y _qTable.GetMaxQ.
        /// </summary>
        private void UpdateQ(string stateKey, QAction action, float reward, string nextStateKey, bool reachedTerminalState)
        {
            float oldQ = _qTable.GetQ(stateKey, action);
            // Si el estado actual es terminal, ya no hay Q futuro, puest que hemos terminado
            float maxQNext = reachedTerminalState ? 0f : _qTable.GetMaxQ(nextStateKey, validActions, out _);
            float target = reward + _params.gamma * maxQNext;
            float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target;
            _qTable.SetQ(stateKey, action, newQ);
        }

        /// <summary>
        /// Función de recompensa.
        /// Ejemplo orientativo:
        ///   si agent == other -> recompensa positiva grande (captura)
        ///   si no -> pequeña penalización negativa por cada paso.
        /// </summary>
        private float ComputeReward(CellInfo agent, CellInfo other, QState state)
        {
            float reward;

            if (agent == other)
            {
                // El agente ha sido atrapado.
                reward = -10f;
            }
            else if (previousDistance == -1)
            {
                // Si es primer paso, la recompensa será 0 porque no tenemos punto de referencia
                reward = 0f;
            }
            else
            {
                // Recompensamos si aumenta la distancia
                // y penalizamos si disminuye o se queda quieto
                reward = (state.Distance - previousDistance) * 2f - 1f;
            }

            previousDistance = state.Distance;
            return reward;
        }

        /// <summary>
        /// Condición de final de episodio.
        /// Lo más simple: cuando agente y oponente están en la misma celda.
        /// También puedes definir una probabilidad para el parámetro v visto en clase.
        /// </summary>
        private bool IsTerminalState(CellInfo agent, CellInfo other)
        {
            return agent == other;
        }

        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            int nx = agentCell.x;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up: ny += 1; break;
                case QAction.Down: ny -= 1; break;
                case QAction.Right: nx += 1; break;
                case QAction.Left: nx -= 1; break;
                case QAction.Stay: return agentCell;
            }

            return _worldInfo[nx, ny];
        }

        private CellInfo MoveOpponent(CellInfo opponent, CellInfo target)
        {
            CellInfo[] path = _navigationAlgorithm.GetPath(opponent, target, 1);
            if (path.Length > 0)
                return path[0];

            return opponent;
        }
        #endregion
    }
}
