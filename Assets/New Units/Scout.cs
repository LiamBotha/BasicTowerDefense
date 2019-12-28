using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scout : SmartEnemy
{
    private bool isInvisible;

    private IEnumerator TurnInvisible()
    {
        if(!isInvisible && currentMp > 5) // Rebalance cost of ability
        {

            isInvisible = true;
            gameObject.tag = "Untagged";
            gameObject.layer = LayerMask.NameToLayer("Default"); // cant be detected if not on attack layer
            --currentMp;

            yield return new WaitForSeconds(5f);

            isInvisible = false;
            gameObject.tag = "Enemy";
            gameObject.layer = LayerMask.NameToLayer("Enemies");
        }
    }

    public override void TakeDamage(int val)
    {
        base.TakeDamage(val);

        StartCoroutine(TurnInvisible());
    }
}
