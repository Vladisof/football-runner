using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PowerupIcon : MonoBehaviour
{
    [FormerlySerializedAs("linkedConsumable"),HideInInspector]
    public Consumable.Consumables LinkedConsumables;

    public Image icon;
    public Slider slider;

	void Start ()
    { 
        icon.sprite = LinkedConsumables.icon;
	}

    void Update()
    {
        slider.value = 1.0f - LinkedConsumables.timeActive / LinkedConsumables.duration;
    }
}
