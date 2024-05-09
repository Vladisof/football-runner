using System.Collections;
using System.Collections.Generic;
using Characters;
using Consumable;
using TMPro;
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
    public TrackManager trackManager;

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

    public Modifier currentModifier = new Modifier();

    private bool _mFinished;
    private readonly List<PowerupIcon> _mPowerUpIcons = new List<PowerupIcon>();
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
    private TrackSegment _mNextValidSegment = null;
    private readonly int _obstacleToClear = 3;

    public override void Enter ()
    {
      _mCountdownRectTransform = countdownText.GetComponent<RectTransform>();

      _mLifeHearts = new Image[_maxLives];

      for (int i = 0; i < _maxLives; ++i)
      {
        _mLifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
      }

      if (MusicPlayer.instance.GetStem(0) != gameTheme)
      {
        MusicPlayer.instance.SetStem(0, gameTheme);
        CoroutineHandler.StartStaticCoroutine(MusicPlayer.instance.RestartAllStems());
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
      pauseButton.gameObject.SetActive(!trackManager.isTutorial);
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

      if (!trackManager.isRerun)
      {
        trackManager.CharactersController.currentLife = trackManager.CharactersController.maxLife;
      }

      currentModifier.OnRunStart(this);

      trackManager.CharactersController.CharactersCollider.OnHitObstacle += ResetCombo;
      _mIsTutorial = !PlayerData.instance.tutorialDone;
      trackManager.isTutorial = _mIsTutorial;

      if (_mIsTutorial)
      {
        tutorialValidatedObstacles.gameObject.SetActive(true);
        tutorialValidatedObstacles.text = $"0/{_obstacleToClear}";

        _mDisplayTutorial = true;

        trackManager.newSegmentCreated = segment =>
        {
          if (trackManager.currentZone != 0 && !_mCountObstacles && _mNextValidSegment == null)
          {
            _mNextValidSegment = segment;
          }
        };


        trackManager.currentSegementChanged = segment =>
        {
          _mCurrentSegmentObstacleIndex = 0;
          _mTutorialCurrentSegmentObstacleIndex = 0;

          if (!_mCountObstacles && trackManager.currentSegment == _mNextValidSegment)
          {
            trackManager.CharactersController.currentTutorialLevel += 1;
            _mCountObstacles = true;
            _mNextValidSegment = null;
            _mDisplayTutorial = true;

            tutorialValidatedObstacles.text = $"{_mTutorialClearedObstacle}/{_obstacleToClear}";
          }
        };
      } else
      {
        trackManager.newSegmentCreated = segment =>
        {
          if (trackManager.currentZone != 0 && !_mCheckObstacle && _mNextValidSegment == null)
          {
            _mNextValidSegment = segment;
          }
        };

        trackManager.currentSegementChanged = segment =>
        {
          _mCurrentSegmentObstacleIndex = 0;

          if (!_mCheckObstacle && trackManager.currentSegment == _mNextValidSegment)
          {
            _mNextValidSegment = null;

            _mCheckObstacle = true;
          }


        };
      }

      _mFinished = false;
      _mPowerUpIcons.Clear();

      StartCoroutine(trackManager.Begin());
    }

    bool _mCheckObstacle = true;

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

      if (trackManager.isLoaded)
      {
        CharactersInputController chrCtrl = trackManager.CharactersController;

        if (chrCtrl.currentLife <= 0)
        {
          pauseButton.gameObject.SetActive(false);
          chrCtrl.CleanConsumable();
          chrCtrl.Characters.animator.SetBool(sDeadHash, true);
          chrCtrl.CharactersCollider.koParticle.gameObject.SetActive(true);
          StartCoroutine(WaitForGameOver());
        }
        
        List<Consumable.Consumables> toRemove = new List<Consumable.Consumables>();
        List<PowerupIcon> toRemoveIcon = new List<PowerupIcon>();

        foreach (Consumables t in chrCtrl.consumables)
        {
          PowerupIcon icon = null;

          foreach (PowerupIcon t1 in _mPowerUpIcons)
          {
            if (t1.LinkedConsumables == t)
            {
              icon = t1;
              break;
            }
          }

          t.Tick(chrCtrl);

          if (!t.active)
          {
            toRemove.Add(t);
            toRemoveIcon.Add(icon);
          } else if (icon == null)
          {
            GameObject o = Instantiate(PowerUpIconPrefab);

            icon = o.GetComponent<PowerupIcon>();

            icon.LinkedConsumables = t;
            icon.transform.SetParent(powerUpZone, false);

            _mPowerUpIcons.Add(icon);
          }
        }

        for (int i = 0; i < toRemove.Count; ++i)
        {
          toRemove[i].Ended(trackManager.CharactersController);

          Addressables.ReleaseInstance(toRemove[i].gameObject);

          if (toRemoveIcon[i] != null)
            Destroy(toRemoveIcon[i].gameObject);

          chrCtrl.consumables.Remove(toRemove[i]);
          _mPowerUpIcons.Remove(toRemoveIcon[i]);
        }

        HandleAvoid();


        UpdateUI();

        currentModifier.OnRunTick(this);
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
      _mWasMoving = trackManager.isMoving;
      trackManager.StopMove();
    }

    public void Resume()
    {
      Time.timeScale = 1.0f;
      pauseButton.gameObject.SetActive(true);
      pauseMenu.gameObject.SetActive(false);
      wholeUI.gameObject.SetActive(true);

      if (_mWasMoving)
      {
        trackManager.StartMove(false);
      }

      AudioListener.pause = false;
    }

    public void QuitToLoadout()
    {
      Time.timeScale = 1.0f;
      AudioListener.pause = false;
      trackManager.End();
      trackManager.isRerun = false;
      PlayerData.instance.Save();
      manager.SwitchState("Loadout");
    }

    private void UpdateUI()
    {
      coinText.text = trackManager.CharactersController.coins.ToString();
      coinMultiplierText.text = "x " + trackManager.multiplier;

      premiumText.text = trackManager.CharactersController.premium.ToString();

      for (int i = 0; i < 3; ++i)
      {

        _mLifeHearts[i].color = trackManager.CharactersController.currentLife > i ? Color.white : Color.black;
      }

      scoreText.text = trackManager.score.ToString();
      scoreMultiplierText.text = "x " + trackManager.multiplier;



      distanceText.text = Mathf.FloorToInt(trackManager.worldDistance).ToString() + "m";

      if (trackManager.timeToStart >= 0)
      {
        countdownText.gameObject.SetActive(true);
        countdownText.text = Mathf.Ceil(trackManager.timeToStart).ToString();
        _mCountdownRectTransform.localScale = Vector3.one * (1.0f - (trackManager.timeToStart - Mathf.Floor(trackManager.timeToStart)));
      } else
      {
        _mCountdownRectTransform.localScale = Vector3.zero;
      }
      
      if (trackManager.CharactersController.inventory != null)
      {
        inventoryIcon.transform.parent.gameObject.SetActive(true);
        inventoryIcon.sprite = trackManager.CharactersController.inventory.icon;
      } else
        inventoryIcon.transform.parent.gameObject.SetActive(false);
    }

    private IEnumerator WaitForGameOver()
    {
      _mFinished = true;
      trackManager.StopMove();
      
      Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

      yield return new WaitForSeconds(2.0f);

      if (currentModifier.OnRunEnd(this))
      {
        if (trackManager.isRerun)
          manager.SwitchState("GameOver");
        else
          OpenGameOverPopup();
      }
    }

    private void ClearPowerUp()
    {
      foreach (PowerupIcon t in _mPowerUpIcons)
      {
        if (t != null)
          Destroy(t.gameObject);
      }

      trackManager.CharactersController.powerUpSource.Stop();

      _mPowerUpIcons.Clear();
    }

    private void OpenGameOverPopup()
    {
      premiumForLifeButton.interactable = PlayerData.instance.premium >= 3;

      premiumCurrencyOwned.text = PlayerData.instance.premium.ToString();

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
      PlayerData.instance.premium -= 3;
      trackManager.CharactersController.premium -= Mathf.Min(trackManager.CharactersController.premium, 3);

      SecondWind();
    }

    private void SecondWind()
    {
      trackManager.CharactersController.currentLife = 1;
      trackManager.isRerun = true;
      StartGame();
    }

    private TrackSegment _currentSeg;
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
      trackManager.bonusSpeed = trackManager.bonusSpeedEachCombo * _combo;
    }

    private void ResetCombo()
    {
      SetComboCount(0);
      UpdateComboUI();
    }

    private void HandleAvoid()
    {
      if (trackManager.segments.Count == 0)
        return;

      if (trackManager.currentSegment.SpawnedObstacles.Count == 0)
        return;

      float ratio = trackManager.currentSegmentDistance / trackManager.currentSegment.worldLength;

      if (_mIsTutorial)
        TutorialCheckObstacleClear(ratio, GetNextObstaclePos(_mTutorialCurrentSegmentObstacleIndex), ref _mTutorialCurrentSegmentObstacleIndex);

      else
        TestObstaclePass(ratio, GetNextObstaclePos(_mCurrentSegmentObstacleIndex), ref _mCurrentSegmentObstacleIndex);
    }

    private float GetNextObstaclePos (int obstacleIndex)
    {
      return obstacleIndex < trackManager.currentSegment.obstaclePositions.Length ? trackManager.currentSegment.obstaclePositions[obstacleIndex] : float.MaxValue;

    }

    private void TestObstaclePass (float ratio, float nextObstaclePosition, ref int obstacleIndex)
    {
      if (_mCheckObstacle && ratio > nextObstaclePosition + 0.01f)
      {
        float detectingObstaclePos = trackManager.currentSegment.obstaclePositions[obstacleIndex];

        obstacleIndex += 1;

        if (!trackManager.CharactersController.CharactersCollider.WasHitObstacle)
        {
          bool addedCombo = false;
          _shouldSlide = trackManager.CharactersController.CharactersCollider.shouldHaveSlided;
          _shouldJump = trackManager.CharactersController.CharactersCollider.shouldHaveJumped;

          foreach (var value in trackManager.currentSegment.SpawnedObstacleAtPos[detectingObstaclePos])
          {
            if (addedCombo)
              break;
          
            if (comboOnlyAllLaneObstacle)
            {
              if ((value as AllLaneObstacle) == false)
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

        trackManager.CharactersController.CharactersCollider.WasHitObstacle = false;

      } else
      {
        _shouldSlide = trackManager.CharactersController.CharactersCollider.shouldHaveSlided;
        _shouldJump = trackManager.CharactersController.CharactersCollider.shouldHaveJumped;
      }


    }


    private void TutorialCheckObstacleClear (float ratio, float nextObstaclePosition, ref int obstacleIndex)
    {
      if (AudioListener.pause && !trackManager.CharactersController.tutorialWaitingForValidation)
      {
        _mDisplayTutorial = false;
        DisplayTutorial(false);
      }


      if (_mCountObstacles && ratio > nextObstaclePosition + 0.05f)
      {
        obstacleIndex += 1;

        if (!trackManager.CharactersController.CharactersCollider.tutorialHitObstacle)
        {
          _mTutorialClearedObstacle += 1;
          tutorialValidatedObstacles.text = $"{_mTutorialClearedObstacle}/{_obstacleToClear}";
        }

        trackManager.CharactersController.CharactersCollider.tutorialHitObstacle = false;

        if (_mTutorialClearedObstacle != _obstacleToClear)
        {
          return;
        }

        _mTutorialClearedObstacle = 0;
        _mCountObstacles = false;
        _mNextValidSegment = null;
        trackManager.ChangeZone();

        tutorialValidatedObstacles.text = "Passed!";

        if (trackManager.currentZone != 0)
        {
          return;
        }

        trackManager.CharactersController.currentTutorialLevel = 3;
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

      switch (trackManager.CharactersController.currentTutorialLevel)
      {
        case 0:
          sideSlideTo.SetActive(value);
          trackManager.CharactersController.tutorialWaitingForValidation = value;
          break;
        case 1:
          upSlideTo.SetActive(value);
          trackManager.CharactersController.tutorialWaitingForValidation = value;
          break;
        case 2:
          downSlideTo.SetActive(value);
          trackManager.CharactersController.tutorialWaitingForValidation = value;
          break;
        case 3:
          finishTo.SetActive(true);
          trackManager.CharactersController.StopSliding();
          trackManager.CharactersController.tutorialWaitingForValidation = value;
          break;
      }
    }


    public void FinishTutorial()
    {
      PlayerData.instance.tutorialDone = true;
      PlayerData.instance.Save();

      QuitToLoadout();
    }
  }
}