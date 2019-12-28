using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    protected int enemyCurrentMp = 100;
    protected int enemyMaxMp = 100;
    protected readonly int attackCost = 20;
    protected readonly int mpRegen = 5;
    protected float regenCooldown = 1;

    [SerializeField] ParticleSystem attackParticle;

    protected override void Walk()
    {
        base.Walk();
        if (anim.gameObject.activeSelf && anim != null)
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
    }

    protected override void Attack()
    {
        if(enemyCurrentMp > attackCost && attackCooldown <= 0)
        {
            attackCooldown = 1;
            UseManaTargeted();
        }
        else if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }


        if(regenCooldown <= 0 && enemyCurrentMp < enemyMaxMp)
        {
            enemyCurrentMp += mpRegen;
            regenCooldown = 1;
        }
        else
        {
            regenCooldown -= Time.deltaTime;
        }
    }

    void UseManaTargeted()
    {
        if (anim != null)
            anim.SetInteger("AnimState", 2);
        if (enemyCurrentMp > attackCost)
        {
            if(target != null)
            {
                IHealth targetHp = target.GetComponent<IHealth>();
                enemyCurrentMp -= attackCost;

                if (attackParticle != null)
                    attackParticle.Play();
                if (rangedSound != null) audioSource.PlayOneShot(rangedSound, 0.02f);
                targetHp.TakeDamage(Atk);
            }
            else
            {
                ChangeState(Walk);
            }
        }
    }

}
