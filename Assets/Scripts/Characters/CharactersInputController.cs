using System.Collections.Generic;
using Tracks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Characters
{
  public class CharactersInputController : MonoBehaviour
  {
    private static readonly int sDeadHash = Animator.StringToHash("Dead");
    private static readonly int sRunStartHash = Animator.StringToHash("runStart");
    private static readonly int sMovingHash = Animator.StringToHash("Moving");
    private static readonly int sJumpingHash = Animator.StringToHash("Jumping");
    private static readonly int sJumpingSpeedHash = Animator.StringToHash("JumpSpeed");
    private static readonly int sSlidingHash = Animator.StringToHash("Sliding");

    [FormerlySerializedAs("trackManager")]
    public TracksManager TracksManager;
    [FormerlySerializedAs("character")]
    public global::Characters.Characters Characters;
    [FormerlySerializedAs("characterCollider")]
    public CharactersCollider CharactersCollider;
    public GameObject blobShadow;
    public float laneChangeSpeed = 1.0f;

    public int maxLife = 3;

    public Consumable.Consumables inventory;

    public int coins
    {
      get => _mCoins;
      set => _mCoins = value;
    }

    public void AddCoin (int amount)
    {
      coins += amount * coinMultiplier;
    }

    public int coinMultiplier { get; set; }

    public int premium
    {
      get => _mPremium;
      set => _mPremium = value;
    }
    public int currentLife
    {
      get => _mCurrentLife;
      set => _mCurrentLife = value;
    }
    public List<Consumable.Consumables> consumables => _mActiveConsumables;
    public bool isJumping => _mJumping;
    public bool isSliding => _mSliding;

    [Header("Controls")]
    public float jumpLength = 2.0f;
    public float jumpHeight = 1.2f;

    public float slideLength = 2.0f;

    [Header("Sounds")]
    public AudioClip slideSound;
    public AudioClip powerUpUseSound;
    [FormerlySerializedAs("powerupSource")]
    public AudioSource powerUpSource;

    [HideInInspector]
    public int currentTutorialLevel;
    [HideInInspector]
    public bool tutorialWaitingForValidation;

    private int _mCoins;
    private int _mPremium;
    private int _mCurrentLife;

    private readonly List<Consumable.Consumables> _mActiveConsumables = new List<Consumable.Consumables>();

    private int _mObstacleLayer;

    private bool _mIsInvincible;
    private bool _mIsRunning;

    private float _mJumpStart;
    private bool _mJumping;

    private bool _mSliding;
    private float _mSlideStart;

    private AudioSource _mAudio;

    private int _mCurrentLane = k_StartingLane;
    private Vector3 _mTargetPosition = Vector3.zero;

    private readonly Vector3 _startingPosition = Vector3.forward * 2f;

    private const int k_StartingLane = 1;
    private const float k_GroundingSpeed = 80f;
    private const float k_ShadowRaycastDistance = 100f;
    private const float k_ShadowGroundOffset = 0.01f;
    private const float k_TrackSpeedToJumpAnimSpeedRatio = 0.6f;
    protected const float TRACK_SPEED_TO_SLIDE_ANIM_SPEED_RATIO = 0.9f;

    protected void Awake()
    {
      _mPremium = 0;
      _mCurrentLife = 0;
      _mSliding = false;
      _mSlideStart = 0.0f;
      _mIsRunning = false;
    }
    
#if !UNITY_STANDALONE
    protected Vector2 m_StartingTouch;
    protected bool m_IsSwiping = false;
#endif

    public void CheatInvincible (bool invincible)
    {
      _mIsInvincible = invincible;
    }

    public bool IsCheatInvincible()
    {
      return _mIsInvincible;
    }

    public void Init()
    {
      transform.position = _startingPosition;
      _mTargetPosition = Vector3.zero;

      _mCurrentLane = k_StartingLane;
      CharactersCollider.transform.localPosition = Vector3.zero;

      currentLife = maxLife;

      _mAudio = GetComponent<AudioSource>();

      _mObstacleLayer = 1 << LayerMask.NameToLayer("Obstacle");
    }

    public void Begin()
    {
      _mIsRunning = false;
      Characters.animator.SetBool(sDeadHash, false);

      CharactersCollider.Init();

      _mActiveConsumables.Clear();
    }

    public void End()
    {
      CleanConsumable();
    }

    public void CleanConsumable()
    {
      foreach (Consumable.Consumables t in _mActiveConsumables)
      {
        t.Ended(this);
        Addressables.ReleaseInstance(t.gameObject);
      }

      _mActiveConsumables.Clear();
    }

    public void StartRunning()
    {
      StartMoving();

      if (!Characters.animator)
      {
        return;
      }

      Characters.animator.Play(sRunStartHash);
      Characters.animator.SetBool(sMovingHash, true);
    }

    public void StartMoving()
    {
      _mIsRunning = true;
    }

    public void StopMoving()
    {
      _mIsRunning = false;
      TracksManager.StopMove();

      if (Characters.animator)
      {
        Characters.animator.SetBool(sMovingHash, false);
      }
    }

    private bool TutorialMoveCheck (int tutorialLevel)
    {
      tutorialWaitingForValidation = currentTutorialLevel != tutorialLevel;

      return (!TracksManager.instance.isTutorial || currentTutorialLevel >= tutorialLevel);
    }

    protected void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
      // Use key input in editor or standalone
      // disabled if it's tutorial and not thecurrent right tutorial level (see func TutorialMoveCheck)

      if (Input.GetKeyDown(KeyCode.LeftArrow) && TutorialMoveCheck(0))
      {
        ChangeLane(-1);
      } else if (Input.GetKeyDown(KeyCode.RightArrow) && TutorialMoveCheck(0))
      {
        ChangeLane(1);
      } else if (Input.GetKeyDown(KeyCode.UpArrow) && TutorialMoveCheck(1))
      {
        Jump();
      } else if (Input.GetKeyDown(KeyCode.DownArrow) && TutorialMoveCheck(2))
      {
        if (!_mSliding)
          Slide();
      }
#else
        // Use touch input on mobile
        if (Input.touchCount == 1)
        {
			if(m_IsSwiping)
			{
				Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;

				// Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
				diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

				if(diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
				{
					if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
					{
						if(TutorialMoveCheck(2) && diff.y < 0)
						{
							Slide();
						}
						else if(TutorialMoveCheck(1))
						{
							Jump();
						}
					}
					else if(TutorialMoveCheck(0))
					{
						if(diff.x < 0)
						{
							ChangeLane(-1);
						}
						else
						{
							ChangeLane(1);
						}
					}
						
					m_IsSwiping = false;
				}
            }

        	// Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
			// a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
			if(Input.GetTouch(0).phase == TouchPhase.Began)
			{
				m_StartingTouch = Input.GetTouch(0).position;
				m_IsSwiping = true;
			}
			else if(Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				m_IsSwiping = false;
			}
        }
#endif

      Vector3 verticalTargetPosition = _mTargetPosition;

      if (_mSliding)
      {
        float correctSlideLength = slideLength * (1.0f + TracksManager.speedRatio);
        float ratio = (TracksManager.worldDistance - _mSlideStart) / correctSlideLength;

        if (ratio >= 1.0f)
        {
          StopSliding();
        }
      }

      if (_mJumping)
      {
        if (TracksManager.isMoving)
        {
          float correctJumpLength = jumpLength * (1.0f + TracksManager.speedRatio);
          float ratio = (TracksManager.worldDistance - _mJumpStart) / correctJumpLength;

          if (ratio >= 1.0f)
          {
            _mJumping = false;
            Characters.animator.SetBool(sJumpingHash, false);
          } else
          {
            verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
          }
        } else if (!AudioListener.pause)
        {
          verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, k_GroundingSpeed * Time.deltaTime);

          if (Mathf.Approximately(verticalTargetPosition.y, 0f))
          {
            Characters.animator.SetBool(sJumpingHash, false);
            _mJumping = false;
          }
        }
      }

      CharactersCollider.transform.localPosition = Vector3.MoveTowards(CharactersCollider.transform.localPosition, verticalTargetPosition, laneChangeSpeed * Time.deltaTime);

      if (Physics.Raycast(CharactersCollider.transform.position + Vector3.up, Vector3.down, out RaycastHit hit, k_ShadowRaycastDistance, _mObstacleLayer))
      {
        blobShadow.transform.position = hit.point + Vector3.up * k_ShadowGroundOffset;
      } else
      {
        Vector3 shadowPosition = CharactersCollider.transform.position;
        shadowPosition.y = k_ShadowGroundOffset;
        blobShadow.transform.position = shadowPosition;
      }
    }

    private void Jump()
    {
      if (!_mIsRunning)
        return;

      if (_mJumping)
      {
        return;
      }

      if (_mSliding)
        StopSliding();

      float correctJumpLength = jumpLength * (1.0f + TracksManager.speedRatio);
      _mJumpStart = TracksManager.worldDistance;
      float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (TracksManager.speed / correctJumpLength);

      Characters.animator.SetFloat(sJumpingSpeedHash, animSpeed);
      Characters.animator.SetBool(sJumpingHash, true);
      _mAudio.PlayOneShot(Characters.jumpSound);
      _mJumping = true;
    }

    private void StopJumping()
    {
      if (_mJumping)
      {
        Characters.animator.SetBool(sJumpingHash, false);
        _mJumping = false;
      }
    }

    private void Slide()
    {
      if (!_mIsRunning)
        return;

      if (_mSliding)
      {
        return;
      }

      if (_mJumping)
        StopJumping();

      float correctSlideLength = slideLength * (1.0f + TracksManager.speedRatio);
      _mSlideStart = TracksManager.worldDistance;
      float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (TracksManager.speed / correctSlideLength);

      Characters.animator.SetFloat(sJumpingSpeedHash, animSpeed);
      Characters.animator.SetBool(sSlidingHash, true);
      _mAudio.PlayOneShot(slideSound);
      _mSliding = true;

      CharactersCollider.Slide(true);
    }

    public void StopSliding()
    {
      if (_mSliding)
      {
        Characters.animator.SetBool(sSlidingHash, false);
        _mSliding = false;

        CharactersCollider.Slide(false);
      }
    }

    private void ChangeLane (int direction)
    {
      if (!_mIsRunning)
        return;

      int targetLane = _mCurrentLane + direction;

      if (targetLane < 0 || targetLane > 2)
        return;

      _mCurrentLane = targetLane;
      _mTargetPosition = new Vector3((_mCurrentLane - 1) * TracksManager.laneOffset, 0, 0);
    }

    public void UseInventory()
    {
      if (inventory == null || !inventory.CanBeUsed(this))
      {
        return;
      }

      UseConsumable(inventory);
      inventory = null;
    }

    public void UseConsumable (Consumable.Consumables c)
    {
      CharactersCollider.audio.PlayOneShot(powerUpUseSound);

      foreach (Consumable.Consumables t in _mActiveConsumables)
      {
        if (t.GetType() != c.GetType())
        {
          continue;
        }

        t.ResetTime();
        Addressables.ReleaseInstance(c.gameObject);
        return;
      }

      c.transform.SetParent(transform, false);
      c.gameObject.SetActive(false);

      _mActiveConsumables.Add(c);
      StartCoroutine(c.Started(this));
    }
  }
}