using UnityEngine;
using System.Collections.Generic;
using Characters;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

public class ShopAccessoriesList : ShopList
{
    public AssetReference headerPrefab;

    List<Characters.Characters> m_CharacterList = new List<Characters.Characters>();

    //엑세서리 상점 UI는 다음과 같이 구현되어 있음.
    //
    //헤더 (캐릭터 이름 표시)
    //- 엑세서리 1
    //- 엑세서리 2
    //헤더 (캐릭터 이름 표시)
    //- 엑세서리 1
    //- 엑세서리 2

    //알고리즘 설명
    //1-1. 캐릭터 DB에서 모든 캐릭터를 불러옴.
    //1-2. 불러온 캐릭터 리스트에서 엑세서리 데이터가 없거나 엑세서리 개수가 0개면 리스트에 추가하지 않음.

    ///2-1. 헤더 프리팹을 비동기로 생성함. 생성이 완료되면 <see cref="LoadedCharacter"></see> 
    ///함수에서 헤더 프리팹을 설정 및 엑세서리 UI 프리팹을 비동기 생성.
    ///2-2. 엑세서리 UI 프리팹의 생성이 완료되면 characterIndex와 accessoryIndex을 이용해 현재 캐릭터의 모든 엑세서리를 표시했는지 체크함.
    ///2-3. 모든 엑세서리를 표시한 경우, characterIndex++를 실행한 후, 모든 캐릭터를 표시했는지 체크하고, 
    ///그렇지 않다면 <see cref="LoadedCharacter"></see>에 characterIndex를 매개로 실행함. 
    ///2-4. 모든 엑세서리가 표시된 게 아닌 경우, (표시할 UI가 남은 경우) 
    ///다음 엑세서리 UI 프리팹을 생성하고 생성 완료 콜백으로 재귀 호출을 실행함.
    public override void Populate()
    {
		m_RefreshCallback = null;

        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        m_CharacterList.Clear();
        foreach (KeyValuePair<string, Characters.Characters> pair in CharactersDatabase.dictionary)
        {
            Characters.Characters c = pair.Value;

            if (c.accessories !=null && c.accessories.Length > 0)
                m_CharacterList.Add(c);
        }

        headerPrefab.InstantiateAsync().Completed += (op) =>
        {
            LoadedCharacter(op, 0);
        };
    }

    void LoadedCharacter(AsyncOperationHandle<GameObject> op, int currentIndex)
    {
        if (op.Result == null || !(op.Result is GameObject))
        {
            Debug.LogWarning(string.Format("Unable to load header {0}.", headerPrefab.RuntimeKey));
        }
        else
        {
            Characters.Characters c = m_CharacterList[currentIndex];

            GameObject header = op.Result;
            header.transform.SetParent(listRoot, false);
            ShopItemListItem itmHeader = header.GetComponent<ShopItemListItem>();
            itmHeader.nameText.text = c.characterName;


            /// <see cref="ShopList"></see> 부모 클래스에서 상속.
            prefabItem.InstantiateAsync().Completed += (innerOp) =>
            {
	            LoadedAccessory(innerOp, currentIndex, 0);
            };
        }
    }

    //재귀 함수
    void LoadedAccessory(AsyncOperationHandle<GameObject> op, int characterIndex, int accessoryIndex)
    {
	    Characters.Characters c = m_CharacterList[characterIndex];
	    if (op.Result == null || !(op.Result is GameObject))
	    {
		    Debug.LogWarning(string.Format("Unable to load shop accessory list {0}.", prefabItem.Asset.name));
	    }
	    else
	    {
		    CharacterAccessor accessor = c.accessories[accessoryIndex];

		    GameObject newEntry = op.Result;
		    newEntry.transform.SetParent(listRoot, false);

		    ShopItemListItem itm = newEntry.GetComponent<ShopItemListItem>();

		    string compoundName = c.characterName + ":" + accessor.accessoryName;

		    itm.nameText.text = accessor.accessoryName;
		    itm.pricetext.text = accessor.cost.ToString();
		    itm.icon.sprite = accessor.accessoryIcon;
		    itm.buyButton.image.sprite = itm.buyButtonSprite;

		    if (accessor.premiumCost > 0)
		    {
			    itm.premiumText.transform.parent.gameObject.SetActive(true);
			    itm.premiumText.text = accessor.premiumCost.ToString();
		    }
		    else
		    {
			    itm.premiumText.transform.parent.gameObject.SetActive(false);
		    }

		    itm.buyButton.onClick.AddListener(delegate()
		    {
			    Buy(compoundName, accessor.cost, accessor.premiumCost);
		    });

		    m_RefreshCallback += delegate() { RefreshButton(itm, accessor, compoundName); };
		    RefreshButton(itm, accessor, compoundName);
	    }

	    accessoryIndex++;

	    if (accessoryIndex == c.accessories.Length)
	    {//we finish the current character accessory, load the next character

		    characterIndex++;
		    if (characterIndex < m_CharacterList.Count)
		    {
			    headerPrefab.InstantiateAsync().Completed += (innerOp) =>
			    {
				    LoadedCharacter(innerOp, characterIndex);
			    };
		    }
	    }
	    else
	    {
		    prefabItem.InstantiateAsync().Completed += (innerOp) =>
		    {
                //재귀 호출
			    LoadedAccessory(innerOp, characterIndex, accessoryIndex);
		    };
	    }
    }

	protected void RefreshButton(ShopItemListItem itm, CharacterAccessor accessor, string compoundName)
	{
		if (accessor.cost > PlayerData.instance.coins)
		{
			itm.buyButton.interactable = false;
			
		}

		if (accessor.premiumCost > PlayerData.instance.premium)
		{
			itm.buyButton.interactable = false;
		}

		if (PlayerData.instance.characterAccessories.Contains(compoundName))
		{
			itm.buyButton.interactable = false;
			itm.buyButton.image.sprite = itm.disabledButtonSprite;
			itm.buyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Owned";
		}
	}



	public void Buy(string name, int cost, int premiumCost)
    {
        PlayerData.instance.coins -= cost;
		PlayerData.instance.premium -= premiumCost;
		PlayerData.instance.AddAccessory(name);
        PlayerData.instance.Save();

#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "store";
        var level = PlayerData.instance.rank.ToString();
        var itemId = name;
        var itemType = "non_consumable";
        var itemQty = 1;

        AnalyticsEvent.ItemAcquired(
            AcquisitionType.Soft,
            transactionContext,
            itemQty,
            itemId,
            itemType,
            level,
            transactionId
        );

        if (cost > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Soft, // Currency type
                transactionContext,
                cost,
                itemId,
                PlayerData.instance.coins, // Balance
                itemType,
                level,
                transactionId
            );
        }

        if (premiumCost > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Premium, // Currency type
                transactionContext,
                premiumCost,
                itemId,
                PlayerData.instance.premium, // Balance
                itemType,
                level,
                transactionId
            );
        }
#endif

        Refresh();
    }
}
