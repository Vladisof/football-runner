using System;
using System.Collections;
using System.Collections.Generic;
using Obstacles;
using Tracks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Characters
{
  [RequireComponent(typeof(AudioSource))]
  public class CharactersCollider : MonoBehaviour
  {
    static int s_HitHash = Animator.StringToHash("Hit");
    static int s_BlinkingValueHash;

    public struct DeathEvent
    {
      public string character;
      public string obstacleType;
      public string themeUsed;
      public int coins;
      public int premium;
      public int score;
      public float worldDistance;
    }

    public CharactersInputController controller;

    public ParticleSystem koParticle;

    [Header("Sound")]
    public AudioClip coinSound;
    public AudioClip premiumSound;

    public DeathEvent deathData { get { return m_DeathData; } }
    public new BoxCollider collider { get { return m_Collider; } }

    public new AudioSource audio { get { return m_Audio; } }

    [HideInInspector]
    public List<GameObject> magnetCoins = new List<GameObject>();

    public bool tutorialHitObstacle
    {
      get => m_TutorialHitObstacle;
      set => m_TutorialHitObstacle = value;
    }

    private bool m_TutorialHitObstacle;

    public bool WasHitObstacle { get { return m_WasHitObstacle; } set { m_WasHitObstacle = value; } }
    private bool m_WasHitObstacle;

    private bool m_Invincible;
    private DeathEvent m_DeathData;
    private BoxCollider m_Collider;
    private AudioSource m_Audio;

    protected float MStartingColliderHeight;

    private readonly Vector3 k_SlidingColliderScale = new Vector3(1.0f, 0.5f, 1.0f);
    private readonly Vector3 k_NotSlidingColliderScale = new Vector3(1.0f, 2.0f, 1.0f);

    private const float k_MagnetSpeed = 10f;
    private const int k_CoinsLayerIndex = 8;
    private const int k_ObstacleLayerIndex = 9;
    private const int k_PowerupLayerIndex = 10;
    private const float k_DefaultInvinsibleTime = 2f;

    public Action OnHitObstacle;

    public bool shouldHaveJumped { get; private set; }

    public bool shouldHaveSlided { get; private set; }

    private Vector3 _startColliderSize;

    protected void Start()
    {
      m_Collider = GetComponent<BoxCollider>();
      _startColliderSize = m_Collider.size;
      m_Audio = GetComponent<AudioSource>();
      MStartingColliderHeight = m_Collider.bounds.size.y;
    }

    public void Init()
    {
      koParticle.gameObject.SetActive(false);

      s_BlinkingValueHash = Shader.PropertyToID("_BlinkingValue");
      m_Invincible = false;
    }

    [FormerlySerializedAs("obstacleMask"), SerializeField]
    LayerMask _obstacleMask;

    public void Slide (bool sliding)
    {
      if (sliding)
      {
        m_Collider.size = Vector3.Scale(m_Collider.size, k_SlidingColliderScale);
        m_Collider.center = m_Collider.center - new Vector3(0.0f, m_Collider.size.y * 0.5f, 0.0f);
      } else
      {
        m_Collider.center = m_Collider.center + new Vector3(0.0f, m_Collider.size.y * 0.5f, 0.0f);
        m_Collider.size = Vector3.Scale(m_Collider.size, k_NotSlidingColliderScale);
      }
    }


    private bool CheckObstacle (Vector3 direction, Vector3 size)
    {
      RaycastHit [] results = new RaycastHit[5];

      int count = Physics.BoxCastNonAlloc(transform.position + m_Collider.center, m_Collider.size / 2, direction, results, Quaternion.identity, 3, _obstacleMask,
        QueryTriggerInteraction.Collide);

      return count > 0;
    }



    protected void Update()
    {
      foreach (GameObject t in magnetCoins)
      {
        t.transform.position = Vector3.MoveTowards(t.transform.position, transform.position, k_MagnetSpeed * Time.deltaTime);
      }


      shouldHaveSlided = CheckObstacle(Vector3.up, Vector3.Scale(_startColliderSize, k_SlidingColliderScale));

      shouldHaveJumped = CheckObstacle(Vector3.down, _startColliderSize);
    }

    private void OnDrawGizmos()
    {


      if (controller.isSliding)
      {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + m_Collider.center + (Vector3.up * m_Collider.size.y), m_Collider.size);

      } else
      {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(transform.position + m_Collider.center + (Vector3.down * m_Collider.size.y), m_Collider.size);

      }
    }

    public bool isShieldEnabled;

    public Action OnTriggeredObstacle;

    protected void OnTriggerEnter (Collider c)
    {
      if (c.gameObject.layer == k_CoinsLayerIndex)
      {
        if (magnetCoins.Contains(c.gameObject))
          magnetCoins.Remove(c.gameObject);

        if (c.GetComponent<Money>().isPremium)
        {
          Addressables.ReleaseInstance(c.gameObject);
          PlayerSaveData.instance.premium += 1;
          controller.premium += 1;
          m_Audio.PlayOneShot(premiumSound);
        } else
        {
          Money.coinPool.Free(c.gameObject);
          PlayerSaveData.instance.coins += 1 * controller.coinMultiplier;
          controller.AddCoin(1);
          m_Audio.PlayOneShot(coinSound);
        }
      } else if (c.gameObject.layer == k_ObstacleLayerIndex)
      {
        if (m_Invincible || controller.IsCheatInvincible())
          return;

        OnTriggeredObstacle?.Invoke();

        if (isShieldEnabled)
        {
          return;

        }

        controller.StopMoving();

        c.enabled = false;

        ObtObstacles ob = c.gameObject.GetComponent<ObtObstacles>();

        if (ob != null)
        {
          ob.Impacted();
        } else
        {
          Addressables.ReleaseInstance(c.gameObject);
        }

        m_WasHitObstacle = true;

        OnHitObstacle?.Invoke();
        
        if (TracksManager.instance.isTutorial)
        {
          m_TutorialHitObstacle = true;
        } else
        {
          controller.currentLife -= 1;
        }

        controller.Characters.animator.SetTrigger(s_HitHash);

        if (controller.currentLife > 0)
        {
          m_Audio.PlayOneShot(controller.Characters.hitSound);
          SetInvincible();
        }
        else
        {
          m_Audio.PlayOneShot(controller.Characters.deathSound);

          m_DeathData.character = controller.Characters.characterName;
          m_DeathData.themeUsed = controller.TracksManager.currentThemes.themeName;
          m_DeathData.obstacleType = ob.GetType().ToString();
          m_DeathData.coins = controller.coins;
          m_DeathData.premium = controller.premium;
          m_DeathData.score = controller.TracksManager.score;
          m_DeathData.worldDistance = controller.TracksManager.worldDistance;

        }
      } else if (c.gameObject.layer == k_PowerupLayerIndex)
      {
        Consumable.Consumables consumables = c.GetComponent<Consumable.Consumables>();

        if (consumables != null)
        {
          controller.UseConsumable(consumables);
        }
      }
    }

    public void SetInvincibleExplicit (bool invincible)
    {
      m_Invincible = invincible;
    }

    public void SetInvincible (float timer = k_DefaultInvinsibleTime)
    {
      StartCoroutine(InvincibleTimer(timer));
    }

    private IEnumerator InvincibleTimer (float timer)
    {
      m_Invincible = true;

      float time = 0;
      float currentBlink = 1.0f;
      float lastBlink = 0.0f;
      const float BLINK_PERIOD = 0.1f;

      while (time < timer && m_Invincible)
      {
        Shader.SetGlobalFloat(s_BlinkingValueHash, currentBlink);

        yield return null;
        time += Time.deltaTime;
        lastBlink += Time.deltaTime;

        if (BLINK_PERIOD < lastBlink)
        {
          lastBlink = 0;
          currentBlink = 1.0f - currentBlink;
        }
      }

      Shader.SetGlobalFloat(s_BlinkingValueHash, 0.0f);

      m_Invincible = false;
    }
  }
}