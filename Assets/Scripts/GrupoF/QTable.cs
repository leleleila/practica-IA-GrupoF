using System;
using System.Collections.Generic;
using QMind;

namespace GrupoF
{
    public class QTable
    {
        private readonly QTableStorage _storage;
        private readonly string[] _actionNames;

        public QTable(QTableStorage storage)
        {
            _storage = storage;
            _actionNames = Enum.GetNames(typeof(QAction));
        }

        private void EnsureState(string stateKey)
        {
            if (!_storage.Data.ContainsKey(stateKey))
            {
                _storage.Data[stateKey] = new float[_actionNames.Length];
            }
        }

        /// <summary>
        /// TODO(alumno):
        /// Devuelve el valor Q(s, a) correspondiente al estado y acción indicados.
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Convierte la acción en un índice del array:
        ///        int index = (int)action;
        ///  3. Devuelve el valor almacenado en:
        ///        _storage.Data[stateKey][index]
        /// </summary>
        public float GetQ(string stateKey, QAction action)
        {
            EnsureState(stateKey);
            int index = (int)action;
            return _storage.Data[stateKey][index];
        }

        /// <summary>
        /// TODO(alumno):
        /// Asigna el valor Q(s, a) para el estado y acción indicados.
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Convierte la acción en un índice del array:
        ///        int index = (int)action;
        ///  3. Guarda el valor recibido en:
        ///        _storage.Data[stateKey][index] = value;
        /// </summary>
        public void SetQ(string stateKey, QAction action, float value)
        {
            EnsureState(stateKey);
            int index = (int)action;
            _storage.Data[stateKey][index] = value;
        }

        /// <summary>
        /// TODO(alumno):
        /// Devuelve el valor máximo max_a Q(s, a) para el estado indicado.
        /// 
        /// Este método se usa en la actualización de Q-Learning:
        ///   maxQNext = GetMaxQ(nextStateKey)
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Obtén el array de Q-values:
        ///        var qValues = _storage.Data[stateKey];
        ///  3. Recorre el array buscando el valor máximo y devuélvelo.
        /// </summary>
        public float GetMaxQ(string stateKey, HashSet<QAction> validActions, out QAction action)
        {
            // Usamos un parámetro con la keyword 'out' para delvolver la acción correspondiente al valor máximo Q (solo teniendo en cuenta aquellas
            // aquellas acciones válidas, determindas en el conjunto validActions, asingadas en la función UpdateValidActions en QMindTrainer/QMindTester)
            // aprovechando el bucle for y no tener que volverlo a buscar en el método GetBestAction
            
            EnsureState(stateKey);
            float[] qValues = _storage.Data[stateKey];

            float currentMaxQValue = float.NegativeInfinity;
            action = default;

            foreach (QAction validAction in validActions)
            {
                int index = (int)validAction;
                float actionQValue = qValues[index];
                if (actionQValue > currentMaxQValue)
                {
                    currentMaxQValue = actionQValue;
                    action = validAction;
                }
            }

            return currentMaxQValue;
        }

        /// <summary>
        /// TODO(alumno):
        /// Devuelve la mejor acción para el estado indicado:
        ///    argmax_a Q(s, a)
        /// 
        /// Este método se usa para:
        ///   - Política greedy (explotar lo aprendido).
        ///   - Parte "explotar" de la política ε-greedy.
        /// 
        /// Pasos recomendados:
        ///  1. Asegúrate de que el estado existe llamando a EnsureState(stateKey).
        ///  2. Obtén el array de Q-values:
        ///        var qValues = _storage.Data[stateKey];
        ///  3. Recorre el array buscando el índice del valor máximo.
        ///  4. Convierte ese índice a QAction:
        ///        return (QAction)bestIndex;
        /// </summary>
        public QAction GetBestAction(string stateKey, HashSet<QAction> validActions)
        {
            EnsureState(stateKey);

            // En vez de iterar otra vez por el array para ver que valor Q conicide con el de la lista de acciones
            // podemos hacer que la propia función de getmaxq devuelva la acción que tiene el valor Q maximo usando
            // la keyword de c# de 'out' y ahorramos proceso de cpu omitiendo una iteración redundante
            GetMaxQ(stateKey, validActions, out QAction bestAction);
            return bestAction;
        }

        public void SaveToCsv()
        {
            _storage.Save();
        }
    }
}