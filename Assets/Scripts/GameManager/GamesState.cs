using System.Collections;
using System.Collections.Generic;
using Characters;
using Consumable;
using Obstacles;
using Sounds;
using TMPro;
using Tracks;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace GameManager
{
  public class GamesState : SwState
  {
    static readonly int sDeadHash = Animator.StringToHash("Dead");

    public Canvas canvas;
    [FormerlySerializedAs("trackManager")]
    public TracksManager TracksManager;

    public AudioClip gameTheme;

    [Header("UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI premiumText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scoreMultiplierText;
    public TextMeshProUGUI coinMultiplierText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI comboText;
    public GameObject comboUI;
    [FormerlySerializedAs("powerupZone")]
    public RectTransform powerUpZone;
    public RectTransform lifeRectTransform;

    public RectTransform pauseMenu;
    public RectTransform wholeUI;
    public Button pauseButton;

    public Image inventoryIcon;

    public GameObject gameOverPopup;
    public Button premiumForLifeButton;
    public TextMeshProUGUI premiumCurrencyOwned;

    [FormerlySerializedAs("PowerupIconPrefab"),Header("Prefabs")]
    public GameObject PowerUpIconPrefab;

    [Header("Tutorial")]
    public TextMeshProUGUI tutorialValidatedObstacles;
    [FormerlySerializedAs("sideSlideTuto")]
    public GameObject sideSlideTo;
    [FormerlySerializedAs("upSlideTuto")]
    public GameObject upSlideTo;
    [FormerlySerializedAs("downSlideTuto")]
    public GameObject downSlideTo;
    [FormerlySerializedAs("finishTuto")]
    public GameObject finishTo;

    public bool comboOnlyOneEachObstaclePos;
    public bool comboOnlyAllLaneObstacle;
    public bool comboOnlySlideAndJump;

    public Modifiers CurrentModifiers = new Modifiers();

    private bool _mFinished;
    private readonly List<PowerUpIconGame> _mPowerUpIcons = new List<PowerUpIconGame>();
    private Image [] _mLifeHearts;

    private RectTransform _mCountdownRectTransform;
    private bool _mWasMoving;

    private bool _mGameOverSelectionDone;

    private readonly int _maxLives = 3;

    private bool _mIsTutorial;
    private int _mTutorialClearedObstacle = 0;
    private bool _mCountObstacles = true;
    private bool _mDisplayTutorial;
    private int _mCurrentSegmentObstacleIndex = 0;
    private int _mTutorialCurrentSegmentObstacleIndex = 0;
    private TracksSegment _mNextValidSegment = null;
    private readonly int _obstacleToClear = 3;

    public override void Enter ()
    {
      _mCountdownRectTransform = countdownText.GetComponent<RectTransform>();

      _mLifeHearts = new Image[_maxLives];

      for (int i = 0; i < _maxLives; ++i)
      {
        _mLifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
      }

      if (SoundPlayer.instance.GetStem(0) != gameTheme)
      {
        SoundPlayer.instance.SetStem(0, gameTheme);
        HandlerCoroutineHandler.StartStaticCoroutine(SoundPlayer.instance.RestartAllStems());
      }

      _mGameOverSelectionDone = false;

      StartGame();
    }

    public override void Exit (SwState to)
    {
      canvas.gameObject.SetActive(false);

      ClearPowerUp();
    }

    private void StartGame()
    {
      canvas.gameObject.SetActive(true);
      pauseMenu.gameObject.SetActive(false);
      wholeUI.gameObject.SetActive(true);
      pauseButton.gameObject.SetActive(!TracksManager.isTutorial);
      gameOverPopup.SetActive(false);

      sideSlideTo.SetActive(false);
      upSlideTo.SetActive(false);
      downSlideTo.SetActive(false);
      finishTo.SetActive(false);
      tutorialValidatedObstacles.gameObject.SetActive(false);

      _avoidByJump = false;
      _avoidBySlide = false;

      SetComboUIActive(false);
      _combo = 0;

      if (!TracksManager.isRerun)
      {
        TracksManager.CharactersController.currentLife = TracksManager.CharactersController.maxLife;
      }

      Modifiers.OnRunStart();

      TracksManager.CharactersController.CharactersCollider.OnHitObstacle += ResetCombo;
      _mIsTutorial = !PlayerSaveData.instance.tutorialDone;
      TracksManager.isTutorial = _mIsTutorial;

      if (_mIsTutorial)
      {
        tutorialValidatedObstacles.gameObject.SetActive(true);
        tutorialValidatedObstacles.text = $"0/{_obstacleToClear}";

        _mDisplayTutorial = true;

        TracksManager.newSegmentCreated = segment =>
        {
          if (TracksManager.currentZone != 0 && !_mCountObstacles && _mNextValidSegment == null)
          {
            _mNextValidSegment = segment;
          }
        };

        TracksManager.currentSegmentChanged = segment =>
        {
          _mCurrentSegmentObstacleIndex = 0;
          _mTutorialCurrentSegmentObstacleIndex = 0;

          if (_mCountObstacles || TracksManager.currentSegment != _mNextValidSegment)
          {
            return;
          }

          TracksManager.CharactersController.currentTutorialLevel += 1;
          _mCountObstacles = true;
          _mNextValidSegment = null;
          _mDisplayTutorial = true;

          tutorialValidatedObstacles.text = $"{_mTutorialClearedObstacle}/{_obstacleToClear}";
        };
      } else
      {
        TracksManager.newSegmentCreated = segment =>
        {
          if (TracksManager.currentZone != 0 && !_mCheckObstacle && _mNextValidSegment == null)
          {
            _mNextValidSegment = segment;
          }
        };

        TracksManager.currentSegmentChanged = segment =>
        {
          _mCurrentSegmentObstacleIndex = 0;

          if (_mCheckObstacle || TracksManager.currentSegment != _mNextValidSegment)
          {
            return;
          }

          _mNextValidSegment = null;

          _mCheckObstacle = true;

        };
      }

      _mFinished = false;
      _mPowerUpIcons.Clear();

      StartCoroutine(TracksManager.Begin());
    }

    private bool _mCheckObstacle = true;

    public override string GetName()
    {
      return "Game";
    }

    public override void Tick()
    {
      if (_mFinished)
      {
        return;
      }

      if (TracksManager.isLoaded)
      {
        CharactersInputController chrCtrl = TracksManager.CharactersController;

        if (chrCtrl.currentLife <= 0)
        {
          pauseButton.gameObject.SetActive(false);
          chrCtrl.CleanConsumable();
          chrCtrl.Characters.animator.SetBool(sDeadHash, true);
          chrCtrl.CharactersCollider.koParticle.gameObject.SetActive(true);
          StartCoroutine(WaitForGameOver());
        }
        
        List<Consumables> toRemove = new List<Consumables>();
        List<PowerUpIconGame> toRemoveIcon = new List<PowerUpIconGame>();

        foreach (Consumables t in chrCtrl.consumables)
        {
          PowerUpIconGame iconGame = null;

          foreach (PowerUpIconGame t1 in _mPowerUpIcons)
          {
            if (t1.LinkedConsumables == t)
            {
              iconGame = t1;
              break;
            }
          }

          t.Tick(chrCtrl);

          if (!t.active)
          {
            toRemove.Add(t);
            toRemoveIcon.Add(iconGame);
          } else if (iconGame == null)
          {
            GameObject o = Instantiate(PowerUpIconPrefab);

            iconGame = o.GetComponent<PowerUpIconGame>();

            iconGame.LinkedConsumables = t;
            iconGame.transform.SetParent(powerUpZone, false);

            _mPowerUpIcons.Add(iconGame);
          }
        }

        for (int i = 0; i < toRemove.Count; ++i)
        {
          toRemove[i].Ended(TracksManager.CharactersController);

          Addressables.ReleaseInstance(toRemove[i].gameObject);

          if (toRemoveIcon[i] != null)
            Destroy(toRemoveIcon[i].gameObject);

          chrCtrl.consumables.Remove(toRemove[i]);
          _mPowerUpIcons.Remove(toRemoveIcon[i]);
        }

        HandleAvoid();


        UpdateUI();

        Modifiers.OnRunTick();
      }
    }

    private void OnApplicationPause (bool pauseStatus)
    {
      if (pauseStatus)
        Pause();
    }

    private void OnApplicationFocus (bool focusStatus)
    {
      if (!focusStatus)
        Pause();
    }

    public void Pause (bool displayMenu = true)
    {
      if (_mFinished || AudioListener.pause == true)
        return;

      AudioListener.pause = true;
      Time.timeScale = 0;

      pauseButton.gameObject.SetActive(false);
      pauseMenu.gameObject.SetActive(displayMenu);
      wholeUI.gameObject.SetActive(false);
      _mWasMoving = TracksManager.isMoving;
      TracksManager.StopMove();
    }

    public void Resume()
    {
      Time.timeScale = 1.0f;
      pauseButton.gameObject.SetActive(true);
      pauseMenu.gameObject.SetActive(false);
      wholeUI.gameObject.SetActive(true);

      if (_mWasMoving)
      {
        TracksManager.StartMove(false);
      }

      AudioListener.pause = false;
    }

    public void QuitToLoadout()
    {
      Time.timeScale = 1.0f;
      AudioListener.pause = false;
      TracksManager.End();
      TracksManager.isRerun = false;
      PlayerSaveData.instance.Save();
      manager.SwitchState("Loadout");
    }

    private void UpdateUI()
    {
      coinText.text = TracksManager.CharactersController.coins.ToString();
      coinMultiplierText.text = "x " + TracksManager.multiplier;

      premiumText.text = TracksManager.CharactersController.premium.ToString();

      for (int i = 0; i < 3; ++i)
      {

        _mLifeHearts[i].color = TracksManager.CharactersController.currentLife > i ? Color.white : Color.black;
      }

      scoreText.text = TracksManager.score.ToString();
      scoreMultiplierText.text = "x " + TracksManager.multiplier;



      distanceText.text = Mathf.FloorToInt(TracksManager.worldDistance).ToString() + "m";

      if (TracksManager.timeToStart >= 0)
      {
        countdownText.gameObject.SetActive(true);
        countdownText.text = Mathf.Ceil(TracksManager.timeToStart).ToString();
        _mCountdownRectTransform.localScale = Vector3.one * (1.0f - (TracksManager.timeToStart - Mathf.Floor(TracksManager.timeToStart)));
      } else
      {
        _mCountdownRectTransform.localScale = Vector3.zero;
      }
      
      if (TracksManager.CharactersController.inventory != null)
      {
        inventoryIcon.transform.parent.gameObject.SetActive(true);
        inventoryIcon.sprite = TracksManager.CharactersController.inventory.icon;
      } else
        inventoryIcon.transform.parent.gameObject.SetActive(false);
    }

    private IEnumerator WaitForGameOver()
    {
      _mFinished = true;
      TracksManager.StopMove();
      
      Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

      yield return new WaitForSeconds(2.0f);

      if (Modifiers.OnRunEnd())
      {
        if (TracksManager.isRerun)
          manager.SwitchState("GameOver");
        else
          OpenGameOverPopup();
      }
    }

    private void ClearPowerUp()
    {
      foreach (PowerUpIconGame t in _mPowerUpIcons)
      {
        if (t != null)
          Destroy(t.gameObject);
      }

      TracksManager.CharactersController.powerUpSource.Stop();

      _mPowerUpIcons.Clear();
    }

    private void OpenGameOverPopup()
    {
      premiumForLifeButton.interactable = PlayerSaveData.instance.premium >= 3;

      premiumCurrencyOwned.text = PlayerSaveData.instance.premium.ToString();

      ClearPowerUp();

      gameOverPopup.SetActive(true);
    }

    public void GameOver()
    {
      manager.SwitchState("GameOver");
    }

    public void PremiumForLife()
    {
      
      if (_mGameOverSelectionDone)
        return;

      _mGameOverSelectionDone = true;
      PlayerSaveData.instance.premium -= 3;
      TracksManager.CharactersController.premium -= Mathf.Min(TracksManager.CharactersController.premium, 3);

      SecondWind();
    }

    private void SecondWind()
    {
      TracksManager.CharactersController.currentLife = 1;
      TracksManager.isRerun = true;
      StartGame();
    }

    private TracksSegment _currentSeg;
    private float _debugRatio;
    private float _debugNePos;

    private bool _shouldSlide;
    private bool _shouldJump;
    private bool _avoidByJump;
    private bool _avoidBySlide;

    private int _combo;

    [FormerlySerializedAs("minComboForDisplay"),SerializeField, Min(0)]
    int _minComboForDisplay;

    private void SetComboUIActive (bool value)
    {
      comboUI.SetActive(value);


    }

    private void UpdateComboUI()
    {
      if (_combo >= _minComboForDisplay)
      {

        SetComboUIActive(true);
        comboText.text = _combo.ToString();

      } else
      {
        SetComboUIActive(false);


      }

    }

    private void AddCombo()
    {
      SetComboCount(_combo + 1);
      UpdateComboUI();
    }

    private void SetComboCount (int count)
    {
      _combo = count;
      TracksManager.bonusSpeed = TracksManager.bonusSpeedEachCombo * _combo;
    }

    private void ResetCombo()
    {
      SetComboCount(0);
      UpdateComboUI();
    }

    private void HandleAvoid()
    {
      if (TracksManager.segments.Count == 0)
        return;

      if (TracksManager.currentSegment.SpawnedObstacles.Count == 0)
        return;

      float ratio = TracksManager.currentSegmentDistance / TracksManager.currentSegment.worldLength;

      if (_mIsTutorial)
        TutorialCheckObstacleClear(ratio, GetNextObstaclePos(_mTutorialCurrentSegmentObstacleIndex), ref _mTutorialCurrentSegmentObstacleIndex);

      else
        TestObstaclePass(ratio, GetNextObstaclePos(_mCurrentSegmentObstacleIndex), ref _mCurrentSegmentObstacleIndex);
    }

    private float GetNextObstaclePos (int obstacleIndex)
    {
      return obstacleIndex < TracksManager.currentSegment.obstaclePositions.Length ? TracksManager.currentSegment.obstaclePositions[obstacleIndex] : float.MaxValue;

    }

    private void TestObstaclePass (float ratio, float nextObstaclePosition, ref int obstacleIndex)
    {
      if (_mCheckObstacle && ratio > nextObstaclePosition + 0.01f)
      {
        float detectingObstaclePos = TracksManager.currentSegment.obstaclePositions[obstacleIndex];

        obstacleIndex += 1;

        if (!TracksManager.CharactersController.CharactersCollider.WasHitObstacle)
        {
          bool addedCombo = false;
          _shouldSlide = TracksManager.CharactersController.CharactersCollider.shouldHaveSlided;
          _shouldJump = TracksManager.CharactersController.CharactersCollider.shouldHaveJumped;

          foreach (var value in TracksManager.currentSegment.SpawnedObstacleAtPos[detectingObstaclePos])
          {
            if (addedCombo)
              break;
          
            if (comboOnlyAllLaneObstacle)
            {
              if ((value as AllLanesObtObstacles) == false)
                continue;
            }
          

            if (comboOnlySlideAndJump)
            {
              if ((_shouldSlide || _shouldJump) == false)
              {

                continue;
              }
            }
          
            AddCombo();
            addedCombo = true;
          
            if (comboOnlyOneEachObstaclePos)
              break;

          }

        }

        TracksManager.CharactersController.CharactersCollider.WasHitObstacle = false;

      } else
      {
        _shouldSlide = TracksManager.CharactersController.CharactersCollider.shouldHaveSlided;
        _shouldJump = TracksManager.CharactersController.CharactersCollider.shouldHaveJumped;
      }


    }


    private void TutorialCheckObstacleClear (float ratio, float nextObstaclePosition, ref int obstacleIndex)
    {
      if (AudioListener.pause && !TracksManager.CharactersController.tutorialWaitingForValidation)
      {
        _mDisplayTutorial = false;
        DisplayTutorial(false);
      }


      if (_mCountObstacles && ratio > nextObstaclePosition + 0.05f)
      {
        obstacleIndex += 1;

        if (!TracksManager.CharactersController.CharactersCollider.tutorialHitObstacle)
        {
          _mTutorialClearedObstacle += 1;
          tutorialValidatedObstacles.text = $"{_mTutorialClearedObstacle}/{_obstacleToClear}";
        }

        TracksManager.CharactersController.CharactersCollider.tutorialHitObstacle = false;

        if (_mTutorialClearedObstacle != _obstacleToClear)
        {
          return;
        }

        _mTutorialClearedObstacle = 0;
        _mCountObstacles = false;
        _mNextValidSegment = null;
        TracksManager.ChangeZone();

        tutorialValidatedObstacles.text = "Passed!";

        if (TracksManager.currentZone != 0)
        {
          return;
        }

        TracksManager.CharactersController.currentTutorialLevel = 3;
        DisplayTutorial(true);
      } else if (_mDisplayTutorial && ratio > nextObstaclePosition - 0.1f)
        DisplayTutorial(true);
    }

    private void DisplayTutorial (bool value)
    {
      if (value)
        Pause(false);
      else
      {
        Resume();
      }

      switch (TracksManager.CharactersController.currentTutorialLevel)
      {
        case 0:
          sideSlideTo.SetActive(value);
          TracksManager.CharactersController.tutorialWaitingForValidation = value;
          break;
        case 1:
          upSlideTo.SetActive(value);
          TracksManager.CharactersController.tutorialWaitingForValidation = value;
          break;
        case 2:
          downSlideTo.SetActive(value);
          TracksManager.CharactersController.tutorialWaitingForValidation = value;
          break;
        case 3:
          finishTo.SetActive(true);
          TracksManager.CharactersController.StopSliding();
          TracksManager.CharactersController.tutorialWaitingForValidation = value;
          break;
      }
    }


    public void FinishTutorial()
    {
      PlayerSaveData.instance.tutorialDone = true;
      PlayerSaveData.instance.Save();

      QuitToLoadout();
    }
  }
}