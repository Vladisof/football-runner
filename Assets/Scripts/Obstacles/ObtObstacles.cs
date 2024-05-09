using System.Collections;
using Tracks;
using UnityEngine;

namespace Obstacles
{
  [RequireComponent(typeof(AudioSource))]
  public abstract class ObtObstacles : MonoBehaviour
  {
    public AudioClip impactedSound;

    public abstract IEnumerator Spawn (TracksSegment segment, float t);

    protected static void RegisterObstacle (TracksSegment segment, ObtObstacles spawned, float? pos = null)
    {
      segment.SpawnedObstacles.Add(spawned);

      if (pos == null)
      {
        return;
      }

      if (segment.SpawnedObstacleAtPos.ContainsKey((float)pos))
      {
        segment.SpawnedObstacleAtPos[(float)pos].Add(spawned);
      } else
      {
        segment.SpawnedObstacleAtPos.Add((float)pos, new System.Collections.Generic.List<ObtObstacles>()
        {
          spawned
        });

      }
    }

    public virtual void Impacted()
    {
      Animation anim = GetComponentInChildren<Animation>();
      AudioSource audioSource = GetComponent<AudioSource>();

      if (anim != null)
      {
        anim.Play();
      }

      if (audioSource == null || impactedSound == null)
      {
        return;
      }

      audioSource.Stop();
      audioSource.loop = false;
      audioSource.clip = impactedSound;
      audioSource.Play();
    }
  }
}