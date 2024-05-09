using System.Collections;
using Characters;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Consumable
{
    public abstract class Consumables : MonoBehaviour
    {
        public float duration;

        public enum ConsumableType
        {
            NONE,
            COIN_MAG,
            SCORE_MULTIPLAYER,
            INVINCIBILITY,
            CHARACTER_LIFE,
            SHIELD,
            MAX_COUNT
        }

        public Sprite icon;
        public AudioClip activatedSound;
        public AssetReference ActivatedParticleReference;
        public bool canBeSpawned = true;

        public bool active => _mActive;
        public float timeActive => MSinceStart;

        private bool _mActive = true;
        protected float MSinceStart;
        protected ParticleSystem MParticleSpawned;

        public abstract ConsumableType GetConsumableType();
        public abstract string GetConsumableName();
        public abstract int GetPrice();
        public abstract int GetPremiumCost();

        public void ResetTime()
        {
            MSinceStart = 0;
        }

        public virtual bool CanBeUsed(CharactersInputController c)
        {
            return true;
        }

        public virtual IEnumerator Started(CharactersInputController c)
        {
            MSinceStart = 0;

            if (activatedSound != null)
            {
                c.powerUpSource.clip = activatedSound;
                c.powerUpSource.Play();
            }

            if (ActivatedParticleReference == null)
            {
                yield break;
            }

            var op = ActivatedParticleReference.InstantiateAsync();
            yield return op;
            MParticleSpawned = op.Result.GetComponent<ParticleSystem>();
            if (!MParticleSpawned.main.loop)
                StartCoroutine(TimedRelease(MParticleSpawned.gameObject, MParticleSpawned.main.duration));

            MParticleSpawned.transform.SetParent(c.CharactersCollider.transform);
            MParticleSpawned.transform.localPosition = op.Result.transform.position;
        }

        static IEnumerator TimedRelease(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            Addressables.ReleaseInstance(obj);
        }

        public virtual void Tick(CharactersInputController c)
        {
            MSinceStart += Time.deltaTime;

            if (!(MSinceStart >= duration))
            {
                return;
            }

            _mActive = false;
        }

        public virtual void Ended(CharactersInputController c)
        {
            if (MParticleSpawned != null)
            {
                if (MParticleSpawned.main.loop)
                    Addressables.ReleaseInstance(MParticleSpawned.gameObject);
            }

            if (activatedSound != null && c.powerUpSource.clip == activatedSound)
                c.powerUpSource.Stop();

            foreach (Consumables t in c.consumables)
            {
                if (!t.active || t.activatedSound == null)
                {
                    continue;
                }

                c.powerUpSource.clip = t.activatedSound;
                c.powerUpSource.Play();
            }
        }
    }
}
