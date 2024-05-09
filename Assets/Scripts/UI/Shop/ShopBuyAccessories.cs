using System.Collections.Generic;
using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace UI.Shop
{
  public class ShopBuyAccessories : ShopBuyList
  {
    public AssetReference headerPrefab;

    private readonly List<Characters.Characters> _mCharacterList = new List<Characters.Characters>();

    protected override void Populate()
    {
      MRefreshCallback = null;

      foreach (Transform t in listRoot)
      {
        Destroy(t.gameObject);
      }

      _mCharacterList.Clear();

      foreach (KeyValuePair<string, Characters.Characters> pair in CharactersDatabase.dictionary)
      {
        Characters.Characters c = pair.Value;

        if (c.accessories is
            {
              Length: > 0
            })
          _mCharacterList.Add(c);
      }

      headerPrefab.InstantiateAsync().Completed += (op) => { LoadedCharacter(op, 0); };
    }

    private void LoadedCharacter (AsyncOperationHandle<GameObject> op, int currentIndex)
    {
      if (op.Result == null)
      {
        Debug.LogWarning($"Unable to load shop header list {headerPrefab.Asset.name}.");
      } else
      {
        Characters.Characters c = _mCharacterList[currentIndex];

        GameObject header = op.Result;
        header.transform.SetParent(listRoot, false);
        ShopItemListBuy itmHeader = header.GetComponent<ShopItemListBuy>();
        itmHeader.nameText.text = c.characterName;


        prefabItem.InstantiateAsync().Completed += (innerOp) => { LoadedAccessory(innerOp, currentIndex, 0); };
      }
    }

    private void LoadedAccessory (AsyncOperationHandle<GameObject> op, int characterIndex, int accessoryIndex)
    {
      Characters.Characters c = _mCharacterList[characterIndex];

      if (op.Result == null)
      {
        Debug.LogWarning($"Unable to load shop accessory list {prefabItem.Asset.name}.");
      } else
      {
        CharacterAccessor accessor = c.accessories[accessoryIndex];

        GameObject newEntry = op.Result;
        newEntry.transform.SetParent(listRoot, false);

        ShopItemListBuy itm = newEntry.GetComponent<ShopItemListBuy>();

        string compoundName = c.characterName + ":" + accessor.accessoryName;

        itm.nameText.text = accessor.accessoryName;
        itm.priceText.text = accessor.cost.ToString();
        itm.icon.sprite = accessor.accessoryIcon;
        itm.buyButton.image.sprite = itm.buyButtonSprite;

        if (accessor.premiumCost > 0)
        {
          itm.premiumText.transform.parent.gameObject.SetActive(true);
          itm.premiumText.text = accessor.premiumCost.ToString();
        } else
        {
          itm.premiumText.transform.parent.gameObject.SetActive(false);
        }

        itm.buyButton.onClick.AddListener(delegate { Buy(compoundName, accessor.cost, accessor.premiumCost); });
        MRefreshCallback += delegate { RefreshButton(itm, accessor, compoundName); };
        RefreshButton(itm, accessor, compoundName);
      }

      accessoryIndex++;

      if (accessoryIndex == c.accessories.Length)
      {
        characterIndex++;

        if (characterIndex < _mCharacterList.Count)
        {
          headerPrefab.InstantiateAsync().Completed += (innerOp) => { LoadedCharacter(innerOp, characterIndex); };
        }
      } else
      {
        prefabItem.InstantiateAsync().Completed += (innerOp) =>
        {

          LoadedAccessory(innerOp, characterIndex, accessoryIndex);
        };
      }
    }

    private static void RefreshButton (ShopItemListBuy itm, CharacterAccessor accessor, string compoundName)
    {
      if (accessor.cost > PlayerSaveData.instance.coins)
      {
        itm.buyButton.interactable = false;

      }

      if (accessor.premiumCost > PlayerSaveData.instance.premium)
      {
        itm.buyButton.interactable = false;
      }

      if (!PlayerSaveData.instance.characterAccessories.Contains(compoundName))
      {
        return;
      }

      itm.buyButton.interactable = false;
      itm.buyButton.image.sprite = itm.disabledButtonSprite;
      itm.buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Owned";
    }



    private void Buy (string name, int cost, int premiumCost)
    {
      PlayerSaveData.instance.coins -= cost;
      PlayerSaveData.instance.premium -= premiumCost;
      PlayerSaveData.instance.AddAccessory(name);
      PlayerSaveData.instance.Save();

      Refresh();
    }
  }
}