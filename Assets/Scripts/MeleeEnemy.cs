using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    private float abilityCooldown = 1.5f;
    private readonly int abilityCost = 15;
    private float defaultSpeed;

    private float attackSpeed = 1;

    protected override void Start()
    {
        defaultSpeed = speed;
        base.Start();
    }

    protected override void Walk()
    {
        base.Walk();
        if (anim != null && anim.gameObject.activeSelf)
            anim.SetInteger("AnimState", 1);

        if (walkSound != null && audioSource.clip != walkSound)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.volume = 0.1f;
            audioSource.Play();
        }

        if (IsRaycastRange())
        {
            target = GetRaycastEnemy();
            ChangeState(Attack);
        }

        if (abilityCooldown <= 0)
        {
            StartCoroutine(UseAbility());
        }
        else
        {
            abilityCooldown -= Time.deltaTime;
        }
    }

    protected override void Attack()
    {
        if(attackCooldown <= 0)
        {
            attackCooldown = attackSpeed;

            if(target != null)
            {
                if (anim != null)
                    anim.SetInteger("AnimState", 2);

                IHealth targetHp = target.GetComponent<IHealth>();
                targetHp.TakeDamage(Atk);

                if (meleeSound != null) audioSource.PlayOneShot(meleeSound, 0.02f);
            }
            else
            {
                ChangeState(Walk);
            }
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    private IEnumerator UseAbility()
    {      
        Speed += 40;
        abilityCooldown = 3;
        TakeDamage(abilityCost);

        yield return new WaitForSeconds(0.5f);

        Speed = defaultSpeed;
    }
}
