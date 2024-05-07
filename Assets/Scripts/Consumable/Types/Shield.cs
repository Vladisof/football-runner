using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Shield : Consumable
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


    public override void Tick(CharacterInputController c)
    {
        base.Tick(c);

    }

    public override IEnumerator Started(CharacterInputController c)
    {
        yield return base.Started(c);
        c.characterCollider.isShieldEnabled = true;
        handler = () => OnTriggerObstacle(c);
        c.characterCollider.OnTriggeredObstacle += handler;


    }

    public override void Ended(CharacterInputController c)
    {
        base.Ended(c);
        c.characterCollider.OnTriggeredObstacle -= handler;

        c.characterCollider.isShieldEnabled = false;
    }

    void OnTriggerObstacle(CharacterInputController c)
    {

        m_SinceStart += duration;

        c.StartCoroutine(SpawnDestroyedVFX(c));
    }

    Action handler;

    IEnumerator SpawnDestroyedVFX(CharacterInputController c)
    {
        if (DestroyedParticleReference != null)
        {
            //Addressables 1.0.1-preview
            var op = DestroyedParticleReference.InstantiateAsync();
            yield return op;
            m_ParticleSpawned = op.Result.GetComponent<ParticleSystem>();

            m_ParticleSpawned.transform.SetParent(c.characterCollider.transform);
            m_ParticleSpawned.transform.localPosition = op.Result.transform.position;
        }
    }

}
