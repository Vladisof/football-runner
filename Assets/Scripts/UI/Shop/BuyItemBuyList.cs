using Consumable;
using UnityEngine;
namespace UI.Shop
{
	public class BuyItemBuyList : ShopBuyList
	{
		public static readonly Consumables.ConsumableType[] SConsumablesTypes = System.Enum.GetValues(typeof(Consumables.ConsumableType)) as Consumables.ConsumableType[];

		protected override void Populate()
		{
			MRefreshCallback = null;
			foreach (Transform t in listRoot)
			{
				Destroy(t.gameObject);
			}

			for(int i = 0; i < SConsumablesTypes.Length; ++i)
			{
				Consumables c = ConsumablesDatabase.GetConsumbale(SConsumablesTypes[i]);
				if(c != null)
				{
					prefabItem.InstantiateAsync().Completed += (op) =>
					{
						if (op.Result == null)
						{
							Debug.LogWarning($"Error itemList.");
							return;
						}
						GameObject newEntry = op.Result;
						newEntry.transform.SetParent(listRoot, false);

						ShopItemListBuy itm = newEntry.GetComponent<ShopItemListBuy>();

						itm.buyButton.image.sprite = itm.buyButtonSprite;

						itm.nameText.text = c.GetConsumableName();
						itm.priceText.text = c.GetPrice().ToString();

						if (c.GetPremiumCost() > 0)
						{
							itm.premiumText.transform.parent.gameObject.SetActive(true);
							itm.premiumText.text = c.GetPremiumCost().ToString();
						} else
						{
							itm.premiumText.transform.parent.gameObject.SetActive(false);
						}

						itm.icon.sprite = c.icon;

						itm.countText.gameObject.SetActive(true);

						itm.buyButton.onClick.AddListener(delegate() { Buy(c); });
						MRefreshCallback += delegate() { RefreshButton(itm, c); };
						RefreshButton(itm, c);
					};
				}
			}
		}

		private static void RefreshButton(ShopItemListBuy itemList, Consumables c)
		{
			PlayerSaveData.instance.consumables.TryGetValue(c.GetConsumableType(), out int count);
			itemList.countText.text = count.ToString();

			if (c.GetPrice() > PlayerSaveData.instance.coins)
			{
				itemList.buyButton.interactable = false;
			}

			if (c.GetPremiumCost() > PlayerSaveData.instance.premium)
			{
				itemList.buyButton.interactable = false;
			}
		}

		private void Buy(Consumables c)
		{
			PlayerSaveData.instance.coins -= c.GetPrice();
			PlayerSaveData.instance.premium -= c.GetPremiumCost();
			PlayerSaveData.instance.Add(c.GetConsumableType());
			PlayerSaveData.instance.Save();

			Refresh();
		}
	}
}
