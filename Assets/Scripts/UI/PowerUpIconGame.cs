using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace UI
{
    public class PowerUpIconGame : MonoBehaviour
    {
        [FormerlySerializedAs("linkedConsumable"),HideInInspector]
        public Consumable.Consumables LinkedConsumables;

        public Image icon;
        public Slider slider;

        private void Start ()
        { 
            icon.sprite = LinkedConsumables.icon;
        }

        private void Update()
        {
            slider.value = 1.0f - LinkedConsumables.timeActive / LinkedConsumables.duration;
        }
    }
}
