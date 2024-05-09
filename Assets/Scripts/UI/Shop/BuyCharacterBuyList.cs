using System.Collections.Generic;
using Characters;
using TMPro;
using UnityEngine;

namespace UI.Shop
{
  public class BuyCharacterBuyList : ShopBuyList
  {
    protected override void Populate()
    {
      MRefreshCallback = null;

      foreach (Transform t in listRoot)
      {
        Destroy(t.gameObject);
      }

      foreach (KeyValuePair<string, Characters.Characters> pair in CharactersDatabase.dictionary)
      {
        Characters.Characters c = pair.Value;

        if (c != null)
        {
          prefabItem.InstantiateAsync().Completed += (op) =>
          {
            if (op.Result == null || !(op.Result is GameObject))
            {
              Debug.LogWarning($"Unable to load character shop list {prefabItem.Asset.name}.");
              return;
            }

            GameObject newEntry = op.Result;
            newEntry.transform.SetParent(listRoot, false);

            ShopItemListBuy itm = newEntry.GetComponent<ShopItemListBuy>();

            itm.icon.sprite = c.icon;
            itm.nameText.text = c.characterName;
            itm.priceText.text = c.cost.ToString();

            itm.buyButton.image.sprite = itm.buyButtonSprite;

            if (c.premiumCost > 0)
            {
              itm.premiumText.transform.parent.gameObject.SetActive(true);
              itm.premiumText.text = c.premiumCost.ToString();
            } else
            {
              itm.premiumText.transform.parent.gameObject.SetActive(false);
            }

            itm.buyButton.onClick.AddListener(delegate { Buy(c); });

            MRefreshCallback += delegate { RefreshButton(itm, c); };
            RefreshButton(itm, c);
          };
        }
      }
    }

    private static void RefreshButton (ShopItemListBuy itm, Characters.Characters c)
    {
      if (c.cost > PlayerSaveData.instance.coins)
      {
        itm.buyButton.interactable = false;
      }

      if (c.premiumCost > PlayerSaveData.instance.premium)
      {
        itm.buyButton.interactable = false;
      }

      if (!PlayerSaveData.instance.characters.Contains(c.characterName))
      {
        return;
      }

      itm.buyButton.interactable = false;
      itm.buyButton.image.sprite = itm.disabledButtonSprite;
      itm.buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Owned";
    }



    private void Buy (Characters.Characters c)
    {
      PlayerSaveData.instance.coins -= c.cost;
      PlayerSaveData.instance.premium -= c.premiumCost;
      PlayerSaveData.instance.AddCharacter(c.characterName);
      PlayerSaveData.instance.Save();
      
      Populate();
    }
  }
}