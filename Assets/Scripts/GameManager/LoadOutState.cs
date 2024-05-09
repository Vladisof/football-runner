using System.Collections;
using System.Collections.Generic;
using Characters;
using Consumable;
using Sounds;
using Themes;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameManager
{
  public class LoadOutState : SwState
  {
    public Canvas inventoryCanvas;

    [Header("Char UI")]
    public TextMeshProUGUI charNameDisplay;
    public RectTransform charSelect;
    public Transform charPosition;

    [Header("Theme UI")]
    public TextMeshProUGUI themeNameDisplay;
    public RectTransform themeSelect;
    public Image themeIcon;

    [FormerlySerializedAs("powerupSelect"), Header("PowerUp UI")]
    public RectTransform powerUpSelect;
    [FormerlySerializedAs("powerupIcon")]
    public Image powerUpIcon;
    [FormerlySerializedAs("powerupCount")]
    public TextMeshProUGUI powerUpCount;
    public Sprite noItemIcon;

    [Header("Accessory UI")]
    public RectTransform accessoriesSelector;
    [FormerlySerializedAs("accesoryNameDisplay")]
    public TextMeshProUGUI accessoryNameDisplay;
    public Image accessoryIconDisplay;

    [FormerlySerializedAs("LeaderSheap"),FormerlySerializedAs("leaderboard"),Header("Other Data")]
    public LeaderSheep LeaderSheep;
    public UI.Missions missionPopup;
    public Button runButton;

    public GameObject tutorialBlocker;
    public GameObject tutorialPrompt;

    public GameObject backgroundMenu;

    public AudioClip menuTheme;


    [FormerlySerializedAs("consumableIcon"), Header("Prefabs")]
    public ConsumablesIcon ConsumablesIcon;

    Consumables.ConsumableType _mPowerUpToUse = Consumables.ConsumableType.NONE;

    private GameObject _mCharacter;
    private readonly List<int> _mOwnedAccessories = new List<int>();
    private int _mUsedAccessory = -1;
    private int _mUsedPowerUpIndex;
    private bool _mIsLoadingCharacter;

    private Modifiers _mCurrentModifiers = new Modifiers();

    private const float k_CharacterRotationSpeed = 45f;
    private const string k_ShopSceneName = "shop";
    private const float k_OwnedAccessoriesCharacterOffset = -0.1f;
    private int _uiLayer;
    private readonly Quaternion _flippedYAxisRotation = Quaternion.Euler(0f, 180f, 0f);

    public override void Enter()
    {
      tutorialBlocker.SetActive(!PlayerSaveData.instance.tutorialDone);
      tutorialPrompt.SetActive(false);

      inventoryCanvas.gameObject.SetActive(true);
      missionPopup.gameObject.SetActive(false);

      charNameDisplay.text = "";
      themeNameDisplay.text = "";

      _uiLayer = LayerMask.NameToLayer("UI");

      backgroundMenu.SetActive(true);

      Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

      if (SoundPlayer.instance.GetStem(0) != menuTheme)
      {
        SoundPlayer.instance.SetStem(0, menuTheme);
        StartCoroutine(SoundPlayer.instance.RestartAllStems());
      }

      runButton.interactable = false;
      runButton.GetComponentInChildren<TextMeshProUGUI>().text = "Loading...";

      if (_mPowerUpToUse != Consumables.ConsumableType.NONE)
      {
        if (!PlayerSaveData.instance.consumables.ContainsKey(_mPowerUpToUse) || PlayerSaveData.instance.consumables[_mPowerUpToUse] == 0)
          _mPowerUpToUse = Consumables.ConsumableType.NONE;
      }

      Refresh();
    }

    public override void Exit (SwState to)
    {
      missionPopup.gameObject.SetActive(false);
      inventoryCanvas.gameObject.SetActive(false);

      if (_mCharacter != null)
        Addressables.ReleaseInstance(_mCharacter);

      GamesState gs = to as GamesState;

      backgroundMenu.SetActive(false);

      if (gs == null)
      {
        return;
      }

      gs.CurrentModifiers = _mCurrentModifiers;

      _mCurrentModifiers = new Modifiers();

      if (_mPowerUpToUse != Consumables.ConsumableType.NONE)
      {
        PlayerSaveData.instance.Consume(_mPowerUpToUse);
        Consumables inv = Instantiate(ConsumablesDatabase.GetConsumbale(_mPowerUpToUse));
        inv.gameObject.SetActive(false);
        gs.TracksManager.CharactersController.inventory = inv;
      }
    }

    public void Refresh()
    {
      PopulatePowerUp();

      StartCoroutine(PopulateCharacters());
      StartCoroutine(PopulateTheme());
    }

    public override string GetName()
    {
      return "Loadout";
    }

    public override void Tick()
    {
      if (!runButton.interactable)
      {
        bool intractable = ThemesDatabases.loaded && CharactersDatabase.loaded;

        if (intractable)
        {
          runButton.interactable = true;
          runButton.GetComponentInChildren<TextMeshProUGUI>().text = "Run!";
          
          tutorialPrompt.SetActive(true);
        }
      }

      if (_mCharacter != null)
      {
        _mCharacter.transform.Rotate(0, k_CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
      }

      charSelect.gameObject.SetActive(PlayerSaveData.instance.characters.Count > 1);
      themeSelect.gameObject.SetActive(PlayerSaveData.instance.themes.Count > 1);
    }

    public void GoToStore()
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene(k_ShopSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public void ChangeCharacter (int dir)
    {
      PlayerSaveData.instance.usedCharacter += dir;

      if (PlayerSaveData.instance.usedCharacter >= PlayerSaveData.instance.characters.Count)
        PlayerSaveData.instance.usedCharacter = 0;
      else if (PlayerSaveData.instance.usedCharacter < 0)
        PlayerSaveData.instance.usedCharacter = PlayerSaveData.instance.characters.Count - 1;

      StartCoroutine(PopulateCharacters());
    }

    public void ChangeAccessory (int dir)
    {
      _mUsedAccessory += dir;

      if (_mUsedAccessory >= _mOwnedAccessories.Count)
        _mUsedAccessory = -1;
      else if (_mUsedAccessory < -1)
        _mUsedAccessory = _mOwnedAccessories.Count - 1;

      if (_mUsedAccessory != -1)
        PlayerSaveData.instance.usedAccessory = _mOwnedAccessories[_mUsedAccessory];
      else
        PlayerSaveData.instance.usedAccessory = -1;

      SetupAccessory();
    }

    public void ChangeTheme (int dir)
    {
      PlayerSaveData.instance.usedTheme += dir;

      if (PlayerSaveData.instance.usedTheme >= PlayerSaveData.instance.themes.Count)
        PlayerSaveData.instance.usedTheme = 0;
      else if (PlayerSaveData.instance.usedTheme < 0)
        PlayerSaveData.instance.usedTheme = PlayerSaveData.instance.themes.Count - 1;

      StartCoroutine(PopulateTheme());
    }

    private IEnumerator PopulateTheme()
    {
      ThemesData t = null;

      while (t == null)
      {
        t = ThemesDatabases.GetThemeData(PlayerSaveData.instance.themes[PlayerSaveData.instance.usedTheme]);
        yield return null;
      }

      themeNameDisplay.text = t.themeName;
      themeIcon.sprite = t.themeIcon;
    }

    private IEnumerator PopulateCharacters()
    {
      accessoriesSelector.gameObject.SetActive(false);
      PlayerSaveData.instance.usedAccessory = -1;
      _mUsedAccessory = -1;

      if (!_mIsLoadingCharacter)
      {
        _mIsLoadingCharacter = true;
        GameObject newChar = null;

        while (newChar == null)
        {
          Characters.Characters c = CharactersDatabase.GetCharacter(PlayerSaveData.instance.characters[PlayerSaveData.instance.usedCharacter]);

          if (c != null)
          {
            _mOwnedAccessories.Clear();

            for (int i = 0; i < c.accessories.Length; ++i)
            {
              string compoundName = c.characterName + ":" + c.accessories[i].accessoryName;

              if (PlayerSaveData.instance.characterAccessories.Contains(compoundName))
              {
                _mOwnedAccessories.Add(i);
              }
            }

            Vector3 pos = charPosition.transform.position;

            pos.x = _mOwnedAccessories.Count > 0 ? k_OwnedAccessoriesCharacterOffset : 0.0f;

            charPosition.transform.position = pos;

            accessoriesSelector.gameObject.SetActive(_mOwnedAccessories.Count > 0);

            AsyncOperationHandle op = Addressables.InstantiateAsync(c.characterName);
            yield return op;

            if (op.Result == null || !(op.Result is GameObject))
            {
              yield break;
            }

            newChar = op.Result as GameObject;
            HelpRender.SetRendererLayerRecursive(newChar, _uiLayer);

            if (newChar != null)
            {
              newChar.transform.SetParent(charPosition, false);
              newChar.transform.rotation = _flippedYAxisRotation;

              if (_mCharacter != null)
                Addressables.ReleaseInstance(_mCharacter);

              _mCharacter = newChar;
            }

            charNameDisplay.text = c.characterName;

            _mCharacter.transform.localPosition = Vector3.right * 1000;
            yield return new WaitForEndOfFrame();
            _mCharacter.transform.localPosition = Vector3.zero;

            SetupAccessory();
          } else
            yield return new WaitForSeconds(1.0f);
        }

        _mIsLoadingCharacter = false;
      }
    }

    private void SetupAccessory()
    {
      Characters.Characters c = _mCharacter.GetComponent<Characters.Characters>();
      c.SetupAccessor(PlayerSaveData.instance.usedAccessory);

      if (PlayerSaveData.instance.usedAccessory == -1)
      {
        accessoryNameDisplay.text = "None";
        accessoryIconDisplay.enabled = false;
      } else
      {
        accessoryIconDisplay.enabled = true;
        accessoryNameDisplay.text = c.accessories[PlayerSaveData.instance.usedAccessory].accessoryName;
        accessoryIconDisplay.sprite = c.accessories[PlayerSaveData.instance.usedAccessory].accessoryIcon;
      }
    }

    void PopulatePowerUp()
    {
      powerUpIcon.gameObject.SetActive(true);

      if (PlayerSaveData.instance.consumables.Count > 0)
      {
        Consumables c = ConsumablesDatabase.GetConsumbale(_mPowerUpToUse);

        powerUpSelect.gameObject.SetActive(true);

        if (c != null)
        {
          powerUpIcon.sprite = c.icon;
          powerUpCount.text = PlayerSaveData.instance.consumables[_mPowerUpToUse].ToString();
        } else
        {
          powerUpIcon.sprite = noItemIcon;
          powerUpCount.text = "";
        }
      } else
      {
        powerUpSelect.gameObject.SetActive(false);
      }
    }

    public void ChangeConsumable (int dir)
    {
      bool found = false;

      do
      {
        _mUsedPowerUpIndex += dir;

        if (_mUsedPowerUpIndex >= (int)Consumables.ConsumableType.MAX_COUNT)
        {
          _mUsedPowerUpIndex = 0;
        } else if (_mUsedPowerUpIndex < 0)
        {
          _mUsedPowerUpIndex = (int)Consumables.ConsumableType.MAX_COUNT - 1;
        }

        if (PlayerSaveData.instance.consumables.TryGetValue((Consumables.ConsumableType)_mUsedPowerUpIndex, out int count) && count > 0)
        {
          found = true;
        }

      } while (_mUsedPowerUpIndex != 0 && !found);

      _mPowerUpToUse = (Consumables.ConsumableType)_mUsedPowerUpIndex;
      PopulatePowerUp();
    }

    public void UnEquipPowerUp()
    {
      _mPowerUpToUse = Consumables.ConsumableType.NONE;
    }


    public void StartGame()
    {
      if (PlayerSaveData.instance.tutorialDone)
      {
        if (PlayerSaveData.instance.fTueLevel == 1)
        {
          PlayerSaveData.instance.fTueLevel = 2;
          PlayerSaveData.instance.Save();
        }
      }

      manager.SwitchState("Game");
    }

  }
}