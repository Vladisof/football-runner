using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Consumable
{
    [CreateAssetMenu(fileName="Consumables", menuName = "RunnerFootball/Consumables Database")]
    public class ConsumablesDatabase : ScriptableObject
    {
        [FormerlySerializedAs("consumbales")]
        public Consumables[] consumables;

        private static Dictionary<Consumables.ConsumableType, Consumables> _consumablesDict;

        public void Load()
        {
            if (_consumablesDict != null)
            {
                return;
            }

            _consumablesDict = new Dictionary<Consumables.ConsumableType, Consumables>();

            foreach (Consumables t in consumables)
            {
                _consumablesDict.Add(t.GetConsumableType(), t);
            }
        }

        static public Consumables GetConsumbale(Consumables.ConsumableType type)
        {
            Consumable.Consumables c;
            return _consumablesDict.TryGetValue (type, out c) ? c : null;
        }
    }
}
