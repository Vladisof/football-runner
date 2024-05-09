using Characters;
using Consumable;
using GameManager;
using Themes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UI.Shop
{
    public class ShopBuyGame : MonoBehaviour
    {
        [FormerlySerializedAs("consumableDatabase")]
        public ConsumablesDatabase ConsumablesDatabase;

        [FormerlySerializedAs("itemList")]
        public BuyItemBuyList ItemBuyList;
        [FormerlySerializedAs("characterList")]
        public BuyCharacterBuyList CharacterBuyList;
        [FormerlySerializedAs("Accessories"),FormerlySerializedAs("accessoriesList")]
        public ShopBuyAccessories BuyAccessories;
        [FormerlySerializedAs("themeList")]
        public ShopBuyThemeList BuyThemeList;

        [Header("UI")]
        public TextMeshProUGUI coinCounter;
        public TextMeshProUGUI premiumCounter;

        private ShopBuyList _mOpenBuyList;


        private void Start ()
        {
            PlayerSaveData.Create();

            ConsumablesDatabase.Load();
            HandlerCoroutineHandler.StartStaticCoroutine(CharactersDatabase.LoadDatabase());
            HandlerCoroutineHandler.StartStaticCoroutine(ThemesDatabases.LoadDatabase());

            _mOpenBuyList = ItemBuyList;
            ItemBuyList.Open();
        }

        private void Update ()
        {
            coinCounter.text = PlayerSaveData.instance.coins.ToString();
            premiumCounter.text = PlayerSaveData.instance.premium.ToString();
        }

        public void OpenItemList()
        {
            _mOpenBuyList.Close();
            ItemBuyList.Open();
            _mOpenBuyList = ItemBuyList;
        }

        public void OpenCharacterList()
        {
            _mOpenBuyList.Close();
            CharacterBuyList.Open();
            _mOpenBuyList = CharacterBuyList;
        }

        public void OpenThemeList()
        {
            _mOpenBuyList.Close();
            BuyThemeList.Open();
            _mOpenBuyList = BuyThemeList;
        }

        public void OpenAccessoriesList()
        {
            _mOpenBuyList.Close();
            BuyAccessories.Open();
            _mOpenBuyList = BuyAccessories;
        }

        public void CloseScene()
        {
            SceneManager.UnloadSceneAsync("shop");
            LoadOutState logoutState = GamesManager.instance.topState as LoadOutState;
            if(logoutState != null)
            {
                logoutState.Refresh();
            }
        }
    }
}
