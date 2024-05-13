using System.IO;
using Obstacles;
using Tracks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Missions
{
  public abstract class MissionsBase
  {
    public enum MissionsType
    {
      SINGL_RUN,
      PICKUP,
      OBSTACL_JUMP,
      SLID,
      MULTIPLIER,
      MAXED
    }

    public float progress;
    public float max;
    public int reward;

    public bool isComplete => progress / max >= 1.0f;

    public void Serialize (BinaryWriter w)
    {
      w.Write(progress);
      w.Write(max);
      w.Write(reward);
    }

    public void Deserialize (BinaryReader r)
    {
      progress = r.ReadSingle();
      max = r.ReadSingle();
      reward = r.ReadInt32();
    }

    public abstract void Created();

    public abstract MissionsType GetMissionType();

    public abstract string GetMissionDesc();

    public abstract void RunStart (TracksManager manager);

    public abstract void Update (TracksManager manager);

    public static MissionsBase GetNewMissionFromType (MissionsType type)
    {
      return type switch
      {
        MissionsType.SINGL_RUN => new SingleMissions(),
        MissionsType.PICKUP => new PickupMissions(),
        MissionsType.OBSTACL_JUMP => new BarrierJumpMissions(),
        MissionsType.SLID => new SlidingMissions(),
        MissionsType.MULTIPLIER => new MultiplierMission(),
        _ => null
      };

    }
  }

  public sealed class SingleMissions : MissionsBase
  {
    public override void Created()
    {
      float [] maxValues =
      {
        500,
        1000,
        1500,
        2000
      };

      int chosenVal = Random.Range(0, maxValues.Length);

      reward = chosenVal + 1;
      max = maxValues[chosenVal];
      progress = 0;
    }

    public override string GetMissionDesc()
    {
      return "Run " + ((int)max) + "m in a single run";
    }

    public override MissionsType GetMissionType()
    {
      return MissionsType.SINGL_RUN;
    }

    public override void RunStart (TracksManager manager)
    {
      progress = 0;
    }

    public override void Update (TracksManager manager)
    {
      progress = manager.worldDistance;
    }
  }

  public class PickupMissions : MissionsBase
  {
    int _previousCoinAmount;

    public override void Created()
    {
      float [] maxValues =
      {
        1000,
        2000,
        3000,
        4000
      };

      int chosen = Random.Range(0, maxValues.Length);

      max = maxValues[chosen];
      reward = chosen + 1;
      progress = 0;
    }

    public override string GetMissionDesc()
    {
      return "Pickup " + max + " Coins";
    }

    public override MissionsType GetMissionType()
    {
      return MissionsType.PICKUP;
    }

    public override void RunStart (TracksManager manager)
    {
      _previousCoinAmount = 0;
    }

    public override void Update (TracksManager manager)
    {
      int coins = manager.CharactersController.coins - _previousCoinAmount;
      progress += coins;

      _previousCoinAmount = manager.CharactersController.coins;
    }
  }

  public class BarrierJumpMissions : MissionsBase
  {
    ObtObstacles _mPrevious;
    Collider [] _mHits;

    private const int k_HitColliderCount = 8;
    private readonly Vector3 _characterColliderSizeOffset = new Vector3(-0.3f, 2f, -0.3f);

    public override void Created()
    {
      float [] maxValues =
      {
        20,
        50,
        75,
        100
      };

      int chosen = Random.Range(0, maxValues.Length);

      max = maxValues[chosen];
      reward = chosen + 1;
      progress = 0;
    }

    public override string GetMissionDesc()
    {
      return "Jump over " + ((int)max) + " barriers";
    }

    public override MissionsType GetMissionType()
    {
      return MissionsType.OBSTACL_JUMP;
    }

    public override void RunStart (TracksManager manager)
    {
      _mPrevious = null;
      _mHits = new Collider[k_HitColliderCount];
    }

    public override void Update (TracksManager manager)
    {
      if (manager.CharactersController.isJumping)
      {
        Vector3 boxSize = manager.CharactersController.CharactersCollider.collider.size + _characterColliderSizeOffset;
        Vector3 boxCenter = manager.CharactersController.transform.position - Vector3.up * (boxSize.y * 0.5f);

        int count = Physics.OverlapBoxNonAlloc(boxCenter, boxSize * 0.5f, _mHits);

        for (int i = 0; i < count; ++i)
        {
          ObtObstacles obs = _mHits[i].GetComponent<ObtObstacles>();

          if (obs != null && obs is AllLanesObtObstacles)
          {
            if (obs != _mPrevious)
            {
              progress += 1;
            }

            _mPrevious = obs;
          }
        }
      }
    }
  }

  public class SlidingMissions : MissionsBase
  {
    float _mPreviousWorldDist;

    public override void Created()
    {
      float [] maxValues =
      {
        20,
        30,
        75,
        150
      };

      int chosen = Random.Range(0, maxValues.Length);

      reward = chosen + 1;
      max = maxValues[chosen];
      progress = 0;
    }

    public override string GetMissionDesc()
    {
      return "Slide for " + ((int)max) + "m";
    }

    public override MissionsType GetMissionType()
    {
      return MissionsType.SLID;
    }

    public override void RunStart (TracksManager manager)
    {
      _mPreviousWorldDist = manager.worldDistance;
    }

    public override void Update (TracksManager manager)
    {
      if (manager.CharactersController.isSliding)
      {
        float dist = manager.worldDistance - _mPreviousWorldDist;
        progress += dist;
      }

      _mPreviousWorldDist = manager.worldDistance;
    }
  }

  public sealed class MultiplierMission : MissionsBase
  {

    public override void Created()
    {
      float [] maxValue =
      {
        3,
        5,
        8,
        10
      };

      int chosen = Random.Range(0, maxValue.Length);

      max = maxValue[chosen];
      reward = (chosen + 1);

      progress = 0;
    }

    public override string GetMissionDesc()
    {
      return "Reach a x" + ((int)max) + " multiplier";
    }

    public override MissionsType GetMissionType()
    {
      return MissionsType.MULTIPLIER;
    }

    public override void RunStart (TracksManager manager)
    {
      progress = 0;
    }

    public override void Update (TracksManager manager)
    {
      if (manager.multiplier > progress)
        progress = manager.multiplier;
    }
  }
}