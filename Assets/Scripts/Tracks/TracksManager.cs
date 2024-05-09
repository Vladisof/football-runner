using System.Collections;
using System.Collections.Generic;
using Characters;
using Consumable;
using Obstacles;
using Sounds;
using Themes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using GameObject = UnityEngine.GameObject;

namespace Tracks
{
  public class TracksManager : MonoBehaviour
  {
    public static TracksManager instance => _sInstance;
    private static TracksManager _sInstance;

    private static readonly int sStartHash = Animator.StringToHash("Start");

    public delegate int MultiplierModifier (int current);
    public MultiplierModifier modifyMultiply;

    [FormerlySerializedAs("characterController"), Header("Character & Movements")]
    public CharactersInputController CharactersController;
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;
    public int speedStep = 4;
    public float laneOffset = 1.0f;
    public float bonusSpeedEachCombo = 0.01f;

    public bool invincible = false;

    [FormerlySerializedAs("consumableDatabase"), Header("Objects")]
    public ConsumablesDatabase ConsumablesDatabase;
    public MeshFilter skyMeshFilter;

    [Header("Parallax")]
    public Transform parallaxRoot;
    public float parallaxRatio = 0.5f;

    [FormerlySerializedAs("tutorialThemeData"), Header("Tutorial")]
    public ThemesData TutorialThemesData;

    public System.Action<TracksSegment> newSegmentCreated;
    public System.Action<TracksSegment> currentSegmentChanged;

    public int trackSeed
    {
      get => _mTrackSeed;
      set => _mTrackSeed = value;
    }

    public float timeToStart => _mTimeToStart;

    public int score => _mScore;
    public int multiplier => _mMultiplier;
    public float currentSegmentDistance => _mCurrentSegmentDistance;
    public float worldDistance => _mTotalWorldDistance;
    public float speed => getFinalSpeed;
    public float speedRatio => (_mSpeed - minSpeed) / (maxSpeed - minSpeed);
    public int currentZone => _mCurrentZone;

    public TracksSegment currentSegment => _mSegments[0];
    public List<TracksSegment> segments => _mSegments;
    public ThemesData currentThemes => _mCurrentThemesData;

    public bool isMoving => _mIsMoving;
    public bool isRerun
    {
      get => _mRerun;
      set => _mRerun = value;
    }

    public bool isTutorial
    {
      get => _mIsTutorial;
      set => _mIsTutorial = value;
    }
    public bool isLoaded
    {
      get;
      private set;
    }
    public bool firstObstacle { get; set; }

    private float _mTimeToStart = -1.0f;

    private int _mTrackSeed = -1;

    private float _mCurrentSegmentDistance;
    private float _mTotalWorldDistance;
    private bool _mIsMoving;
    private float _mSpeed;

    private float getFinalSpeed => Mathf.Clamp(_mSpeed + bonusSpeed, minSpeed, maxSpeed);
    private float _mTimeSincePowerUp;
    private float _mTimeSinceLastPremium;

    private int _mMultiplier;

    private readonly List<TracksSegment> _mSegments = new List<TracksSegment>();
    private readonly List<TracksSegment> _mPastSegments = new List<TracksSegment>();
    private int _mSafeSegmentLeft;

    private ThemesData _mCurrentThemesData;
    private int _mCurrentZone;
    private float _mCurrentZoneDistance;
    private const int k_MPreviousSegment = -1;

    private int _mScore;
    private float _mScoreAccum;
    private bool _mRerun;

    private bool _mIsTutorial;

    private Vector3 _mCameraOriginalPos = Vector3.zero;

    private const float k_FloatingOriginThreshold = 10000f;

    private const float k_CountdownToStartLength = 5f;
    private const float k_CountdownSpeed = 1.5f;
    private const float k_StartingSegmentDistance = 2f;
    private const int k_StartingSafeSegments = 2;
    private const int k_StartingCoinPoolSize = 256;
    private const int k_DesiredSegmentCount = 10;
    private const float k_SegmentRemovalDistance = -30f;
    private const float k_Acceleration = 0.2f;
    
    protected void Awake()
    {
      _mScoreAccum = 0.0f;
      _sInstance = this;
    }

    public void StartMove (bool isRestart = true)
    {
      CharactersController.StartMoving();
      _mIsMoving = true;

      if (isRestart)
        _mSpeed = minSpeed;
    }

    public void StopMove()
    {
      _mIsMoving = false;
    }

    private IEnumerator WaitToStart()
    {
      CharactersController.Characters.animator.Play(sStartHash);
      _mTimeToStart = k_CountdownToStartLength;

      while (_mTimeToStart >= 0)
      {
        yield return null;
        _mTimeToStart -= Time.deltaTime * k_CountdownSpeed;
      }

      _mTimeToStart = -1;

      if (_mRerun)
      {
        CharactersController.CharactersCollider.SetInvincible();
      }

      CharactersController.StartRunning();
      StartMove();
    }

    public IEnumerator Begin()
    {
      if (!_mRerun)
      {
        firstObstacle = true;

        if (Camera.main != null)
        {
          _mCameraOriginalPos = Camera.main.transform.position;
          Camera.main.fieldOfView = 77.0f;

          if (_mTrackSeed != -1)
            Random.InitState(_mTrackSeed);
          else
            Random.InitState((int)System.DateTime.Now.Ticks);

          _mCurrentSegmentDistance = k_StartingSegmentDistance;
          _mTotalWorldDistance = 0.0f;

          CharactersController.gameObject.SetActive(true);

          var op = Addressables.InstantiateAsync(PlayerSaveData.instance.characters[PlayerSaveData.instance.usedCharacter], Vector3.zero, Quaternion.identity);
          yield return op;

          if (op.Result == null)
          {
            Debug.LogWarning($"Unable to load character {PlayerSaveData.instance.characters[PlayerSaveData.instance.usedCharacter]}.");
            yield break;
          }

          Characters.Characters player = op.Result.GetComponent<Characters.Characters>();

          player.SetupAccessor(PlayerSaveData.instance.usedAccessory);

          CharactersController.Characters = player;
          CharactersController.TracksManager = this;

          CharactersController.Init();
          CharactersController.CheatInvincible(invincible);

          player.transform.SetParent(CharactersController.CharactersCollider.transform, false);
          Camera.main.transform.SetParent(CharactersController.transform, true);
        }

        _mCurrentThemesData = _mIsTutorial ? TutorialThemesData : ThemesDatabases.GetThemeData(PlayerSaveData.instance.themes[PlayerSaveData.instance.usedTheme]);

        _mCurrentZone = 0;
        _mCurrentZoneDistance = 0;

        skyMeshFilter.sharedMesh = _mCurrentThemesData.skyMesh;
        RenderSettings.fogColor = _mCurrentThemesData.fogColor;
        RenderSettings.fog = true;

        gameObject.SetActive(true);
        CharactersController.gameObject.SetActive(true);
        CharactersController.coins = 0;
        CharactersController.premium = 0;

        _mScore = 0;
        _mScoreAccum = 0;
        bonusSpeed = 0;
        _mSafeSegmentLeft = _mIsTutorial ? 0 : k_StartingSafeSegments;

        Money.coinPool = new PoolObj(currentThemes.collectiblePrefab, k_StartingCoinPoolSize);

        PlayerSaveData.instance.StartRunMissions(this);
      }

      CharactersController.Begin();
      StartCoroutine(WaitToStart());
      isLoaded = true;
    }

    public void End()
    {
      foreach (TracksSegment seg in _mSegments)
      {
        Addressables.ReleaseInstance(seg.gameObject);
        _spawnedSegments--;
      }

      foreach (TracksSegment t in _mPastSegments)
      {
        Addressables.ReleaseInstance(t.gameObject);
      }

      _mSegments.Clear();
      _mPastSegments.Clear();

      CharactersController.End();

      gameObject.SetActive(false);
      Addressables.ReleaseInstance(CharactersController.Characters.gameObject);
      CharactersController.Characters = null;

      if (Camera.main != null)
      {
        Camera.main.transform.SetParent(null);
        Camera.main.transform.position = _mCameraOriginalPos;
      }

      CharactersController.gameObject.SetActive(false);

      for (int i = 0; i < parallaxRoot.childCount; ++i)
      {
        _parallaxRootChildren--;
        Destroy(parallaxRoot.GetChild(i).gameObject);
      }

      if (CharactersController.inventory == null)
      {
        return;
      }

      PlayerSaveData.instance.Add(CharactersController.inventory.GetConsumableType());
      CharactersController.inventory = null;
    }
    private int _parallaxRootChildren;
    
    private int _spawnedSegments;

    private void Update()
    {
      while (_spawnedSegments < (_mIsTutorial ? 4 : k_DesiredSegmentCount))
      {
        StartCoroutine(SpawnNewSegment());
        _spawnedSegments++;
      }

      if (parallaxRoot != null && currentThemes.cloudPrefabs.Length > 0)
      {
        while (_parallaxRootChildren < currentThemes.cloudNumber)
        {
          float lastZ = parallaxRoot.childCount == 0 ? 0 : parallaxRoot.GetChild(parallaxRoot.childCount - 1).position.z + currentThemes.cloudMinimumDistance.z;

          GameObject cloud = currentThemes.cloudPrefabs[Random.Range(0, currentThemes.cloudPrefabs.Length)];

          if (cloud == null)
          {
            continue;
          }

          GameObject obj = Instantiate(cloud, parallaxRoot, false);

          obj.transform.localPosition = Vector3.up * (currentThemes.cloudMinimumDistance.y + (Random.value - 0.5f) * currentThemes.cloudSpread.y)
            + Vector3.forward * (lastZ + (Random.value - 0.5f) * currentThemes.cloudSpread.z)
            + Vector3.right * (currentThemes.cloudMinimumDistance.x + (Random.value - 0.5f) * currentThemes.cloudSpread.x);

          obj.transform.localScale = obj.transform.localScale * (1.0f + (Random.value - 0.5f) * 0.5f);
          obj.transform.localRotation = Quaternion.AngleAxis(Random.value * 360.0f, Vector3.up);
          _parallaxRootChildren++;
        }
      }

      if (!_mIsMoving)
        return;

      float scaledSpeed = getFinalSpeed * Time.deltaTime;
      _mScoreAccum += scaledSpeed;
      _mCurrentZoneDistance += scaledSpeed;

      int intScore = Mathf.FloorToInt(_mScoreAccum);

      if (intScore != 0)
        AddScore(intScore);

      _mScoreAccum -= intScore;

      _mTotalWorldDistance += scaledSpeed;
      _mCurrentSegmentDistance += scaledSpeed;

      if (_mCurrentSegmentDistance > _mSegments[0].worldLength)
      {
        _mCurrentSegmentDistance -= _mSegments[0].worldLength;
        
        _mPastSegments.Add(_mSegments[0]);
        _mSegments.RemoveAt(0);
        _spawnedSegments--;

        currentSegmentChanged?.Invoke(_mSegments[0]);
      }

      Transform characterTransform = CharactersController.transform;

      _mSegments[0].GetPointAtInWorldUnit(_mCurrentSegmentDistance, out Vector3 currentPos, out Quaternion currentRot);
      
      bool needRecenter = currentPos.sqrMagnitude > k_FloatingOriginThreshold;
      
      if (parallaxRoot != null)
      {
        Vector3 difference = (currentPos - characterTransform.position) * parallaxRatio;
        int count = parallaxRoot.childCount;

        for (int i = 0; i < count; i++)
        {
          Transform cloud = parallaxRoot.GetChild(i);
          cloud.position += difference - (needRecenter ? currentPos : Vector3.zero);
        }
      }

      if (needRecenter)
      {
        int count = _mSegments.Count;

        for (int i = 0; i < count; i++)
        {
          _mSegments[i].transform.position -= currentPos;
        }

        count = _mPastSegments.Count;

        for (int i = 0; i < count; i++)
        {
          _mPastSegments[i].transform.position -= currentPos;
        }
        
        _mSegments[0].GetPointAtInWorldUnit(_mCurrentSegmentDistance, out currentPos, out currentRot);
      }

      characterTransform.rotation = currentRot;
      characterTransform.position = currentPos;

      if (parallaxRoot != null && currentThemes.cloudPrefabs.Length > 0)
      {
        for (int i = 0; i < parallaxRoot.childCount; ++i)
        {
          Transform child = parallaxRoot.GetChild(i);

          if (!((child.localPosition - currentPos).z < -50))
          {
            continue;
          }

          _parallaxRootChildren--;
          Destroy(child.gameObject);
        }
      }
      
      for (int i = 0; i < _mPastSegments.Count; ++i)
      {
        if (!((_mPastSegments[i].transform.position - currentPos).z < k_SegmentRemovalDistance))
        {
          continue;
        }

        _mPastSegments[i].Cleanup();
        _mPastSegments.RemoveAt(i);
        i--;
      }

      PowerUpSpawnUpdate();

      if (!_mIsTutorial)
      {
        if (_mSpeed < maxSpeed)
          _mSpeed += k_Acceleration * Time.deltaTime;
        else
          _mSpeed = maxSpeed;
      }

      _mMultiplier = 1 + Mathf.FloorToInt((getFinalSpeed - minSpeed) / (maxSpeed - minSpeed) * speedStep);

      if (modifyMultiply != null)
      {
        foreach (MultiplierModifier part in modifyMultiply.GetInvocationList())
        {
          _mMultiplier = part(_mMultiplier);
        }
      }

      CharactersController.coinMultiplier = _mMultiplier;

      if (!_mIsTutorial)
      {
        int currentTarget = (PlayerSaveData.instance.rank + 1) * 300;

        if (_mTotalWorldDistance > currentTarget)
        {
          PlayerSaveData.instance.rank += 1;
          PlayerSaveData.instance.Save();
        }

        PlayerSaveData.instance.UpdateMissions(this);
      }

      SoundPlayer.instance.UpdateVolumes(speedRatio);
    }

    public float bonusSpeed;

    private void PowerUpSpawnUpdate()
    {
      _mTimeSincePowerUp += Time.deltaTime;
      _mTimeSinceLastPremium += Time.deltaTime;
    }

    public void ChangeZone()
    {
      _mCurrentZone += 1;

      if (_mCurrentZone >= _mCurrentThemesData.zones.Length)
        _mCurrentZone = 0;

      _mCurrentZoneDistance = 0;
    }

    private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);

    private IEnumerator SpawnNewSegment()
    {
      if (!_mIsTutorial)
      {
        if (_mCurrentThemesData.zones[_mCurrentZone].length < _mCurrentZoneDistance)
          ChangeZone();
      }

      int segmentUse = Random.Range(0, _mCurrentThemesData.zones[_mCurrentZone].prefabList.Length);

      if (segmentUse == k_MPreviousSegment)
        segmentUse = (segmentUse + 1) % _mCurrentThemesData.zones[_mCurrentZone].prefabList.Length;

      AsyncOperationHandle segmentToUseOp = _mCurrentThemesData.zones[_mCurrentZone].prefabList[segmentUse].InstantiateAsync(_offScreenSpawnPos, Quaternion.identity);
      yield return segmentToUseOp;

      if (segmentToUseOp.Result == null || !(segmentToUseOp.Result is GameObject))
      {
        Debug.LogWarning($"Unable to load segment {_mCurrentThemesData.zones[_mCurrentZone].prefabList[segmentUse].Asset.name}.");
        yield break;
      }

      TracksSegment newSegment = (segmentToUseOp.Result as GameObject)?.GetComponent<TracksSegment>();

      Vector3 currentExitPoint;
      Quaternion currentExitRotation;

      if (_mSegments.Count > 0)
      {
        _mSegments[^1].GetPointAt(1.0f, out currentExitPoint, out currentExitRotation);
      } else
      {
        currentExitPoint = transform.position;
        currentExitRotation = transform.rotation;
      }

      if (newSegment != null)
      {
        newSegment.transform.rotation = currentExitRotation;

        newSegment.GetPointAt(0.0f, out Vector3 entryPoint, out Quaternion _);


        Vector3 pos = currentExitPoint + (newSegment.transform.position - entryPoint);
        newSegment.transform.position = pos;
        newSegment.manager = this;

        newSegment.transform.localScale = new Vector3((Random.value > 0.5f ? -1 : 1), 1, 1);
        newSegment.objectRoot.localScale = new Vector3(1.0f / newSegment.transform.localScale.x, 1, 1);

        if (_mSafeSegmentLeft <= 0)
        {
          SpawnObstacle(newSegment);
        } else
          _mSafeSegmentLeft -= 1;

        _mSegments.Add(newSegment);

        newSegmentCreated?.Invoke(newSegment);
      }

    }


    private void SpawnObstacle (TracksSegment segment)
    {
      if (segment.possibleObstacles.Length != 0)
      {
        for (int i = 0; i < segment.obstaclePositions.Length; ++i)
        {
          AssetReference assetRef = segment.possibleObstacles[Random.Range(0, segment.possibleObstacles.Length)];
          StartCoroutine(SpawnFromAssetReference(assetRef, segment, i));
        }
      }

      StartCoroutine(SpawnCoinAndPowerUp(segment));
    }

    private IEnumerator SpawnFromAssetReference (AssetReference reference, TracksSegment segment, int posIndex)
    {
      AsyncOperationHandle op = Addressables.LoadAssetAsync<GameObject>(reference);
      yield return op;
      GameObject obj = op.Result as GameObject;

      if (obj == null)
      {
        yield break;
      }

      ObtObstacles obtObstacles = obj.GetComponent<ObtObstacles>();

      if (obtObstacles != null)
        yield return obtObstacles.Spawn(segment, segment.obstaclePositions[posIndex]);
    }

    private IEnumerator SpawnCoinAndPowerUp (TracksSegment segment)
    {
      if (_mIsTutorial)
      {
        yield break;
      }

      const float INCREMENT = 1.5f;
      float currentWorldPos = 0.0f;
      int currentLane = Random.Range(0, 3);

      float powerChance = Mathf.Clamp01(Mathf.Floor(_mTimeSincePowerUp) * 0.5f * 0.001f);
      float premiumChance = Mathf.Clamp01(Mathf.Floor(_mTimeSinceLastPremium) * 0.5f * 0.0001f);

      while (currentWorldPos < segment.worldLength)
      {
        segment.GetPointAtInWorldUnit(currentWorldPos, out Vector3 pos, out Quaternion rot);


        bool laneValid = true;
        int testedLane = currentLane;

        while (Physics.CheckSphere(pos + ((testedLane - 1) * laneOffset * (rot * Vector3.right)), 0.4f, 1 << 9))
        {
          testedLane = (testedLane + 1) % 3;

          if (currentLane != testedLane)
          {
            continue;
          }

          laneValid = false;
          break;
        }

        currentLane = testedLane;

        if (laneValid)
        {
          pos = pos + ((currentLane - 1) * laneOffset * (rot * Vector3.right));


          GameObject toUse = null;

          if (Random.value < powerChance)
          {
            int picked = Random.Range(0, ConsumablesDatabase.consumables.Length);
            
            if (ConsumablesDatabase.consumables[picked].canBeSpawned)
            {
              _mTimeSincePowerUp = 0.0f;
              powerChance = 0.0f;

              AsyncOperationHandle op = Addressables.InstantiateAsync(ConsumablesDatabase.consumables[picked].gameObject.name, pos, rot);
              yield return op;

              if (op.Result == null || !(op.Result is GameObject))
              {
                Debug.LogWarning($"Unable to load consumable {ConsumablesDatabase.consumables[picked].gameObject.name}.");
                yield break;
              }

              toUse = op.Result as GameObject;

              if (toUse != null)
              {
                toUse.transform.SetParent(segment.transform, true);
              }
            }
          } else if (Random.value < premiumChance)
          {
            _mTimeSinceLastPremium = 0.0f;
            premiumChance = 0.0f;

            AsyncOperationHandle op = Addressables.InstantiateAsync(currentThemes.premiumCollectible.name, pos, rot);
            yield return op;

            if (op.Result == null || !(op.Result is GameObject))
            {
              Debug.LogWarning($"Unable to load collectable {currentThemes.premiumCollectible.name}.");
              yield break;
            }

            toUse = op.Result as GameObject;

            if (toUse != null)
            {
              toUse.transform.SetParent(segment.transform, true);
            }
          } else
          {
            toUse = Money.coinPool.Get(pos, rot);
            toUse.transform.SetParent(segment.collectibleTransform, true);
          }

          if (toUse != null)
          {
            Vector3 oldPos = toUse.transform.position;
            toUse.transform.position += Vector3.back;
            toUse.transform.position = oldPos;
          }
        }

        currentWorldPos += INCREMENT;
      }
    }

    private void AddScore (int amount)
    {
      _mScore += amount * _mMultiplier;
    }
  }
}