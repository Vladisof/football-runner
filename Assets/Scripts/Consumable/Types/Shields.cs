using System;
using System.Collections;
using Characters;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Consumable.Types
{
    public class Shields : Consumables
    {
        public AssetReference DestroyedParticleReference;


        public override string GetConsumableName()
        {
            return "Shield";
        }

        public override ConsumableType GetConsumableType()
        {
            return ConsumableType.SHIELD;
        }

        public override int GetPremiumCost()
        {
            return 2;
        }

        public override int GetPrice()
        {
            return 500;

        }


        public override IEnumerator Started(CharactersInputController c)
        {
            yield return base.Started(c);
            c.CharactersCollider.isShieldEnabled = true;
            _handler = () => OnTriggerObstacle(c);
            c.CharactersCollider.OnTriggeredObstacle += _handler;


        }

        public override void Ended(CharactersInputController c)
        {
            base.Ended(c);
            c.CharactersCollider.OnTriggeredObstacle -= _handler;

            c.CharactersCollider.isShieldEnabled = false;
        }

        void OnTriggerObstacle(CharactersInputController c)
        {

            MSinceStart += duration;

            c.StartCoroutine(SpawnDestroyedVFX(c));
        }

        Action _handler;

        IEnumerator SpawnDestroyedVFX(CharactersInputController c)
        {
            if (DestroyedParticleReference != null)
            {
                //Addressables 1.0.1-preview
                var op = DestroyedParticleReference.InstantiateAsync();
                yield return op;
                MParticleSpawned = op.Result.GetComponent<ParticleSystem>();

                MParticleSpawned.transform.SetParent(c.CharactersCollider.transform);
                MParticleSpawned.transform.localPosition = op.Result.transform.position;
            }
        }

    }
}
