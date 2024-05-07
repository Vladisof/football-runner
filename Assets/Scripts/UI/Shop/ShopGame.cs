using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopGame : MonoBehaviour
    {
        public ConsumableDatabase consumableDatabase;

        public ShopItemList itemList;
        public ShopCharacterList characterList;
        public ShopAccessoriesList accessoriesList;
        public ShopThemeList themeList;

        [Header("UI")]
        public TextMeshProUGUI coinCounter;
        public TextMeshProUGUI premiumCounter;

        private ShopList _mOpenList;


        private void Start ()
        {
            PlayerData.Create();

            consumableDatabase.Load();
            CoroutineHandler.StartStaticCoroutine(CharacterDatabase.LoadDatabase());
            CoroutineHandler.StartStaticCoroutine(ThemeDatabase.LoadDatabase());

            _mOpenList = itemList;
            itemList.Open();
        }

        private void Update ()
        {
            coinCounter.text = PlayerData.instance.coins.ToString();
            premiumCounter.text = PlayerData.instance.premium.ToString();
        }

        public void OpenItemList()
        {
            _mOpenList.Close();
            itemList.Open();
            _mOpenList = itemList;
        }

        public void OpenCharacterList()
        {
            _mOpenList.Close();
            characterList.Open();
            _mOpenList = characterList;
        }

        public void OpenThemeList()
        {
            _mOpenList.Close();
            themeList.Open();
            _mOpenList = themeList;
        }

        public void OpenAccessoriesList()
        {
            _mOpenList.Close();
            accessoriesList.Open();
            _mOpenList = accessoriesList;
        }

        public void LoadScene(string scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }

        public void CloseScene()
        {
            SceneManager.UnloadSceneAsync("shop");
            LoadoutState logoutState = GameManager.instance.topState as LoadoutState;
            if(logoutState != null)
            {
                logoutState.Refresh();
            }
        }
    }
}
