using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace UI.Shop
{
	public class ShopItemListBuy : MonoBehaviour
	{
		public Image icon;
		public TextMeshProUGUI nameText;
		[FormerlySerializedAs("pricetext")]
		public TextMeshProUGUI priceText;
		public TextMeshProUGUI premiumText;
		public Button buyButton;

		public TextMeshProUGUI countText;

		public Sprite buyButtonSprite;
		public Sprite disabledButtonSprite;
	}
}
