using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : SmartEnemy
{
    [SerializeField] ParticleSystem explosionParticle;
    [SerializeField] AudioClip explosionSound;

    protected override void Attack()
    {
        if (currentMp > 10 && attackCooldown <= 0)
        {
            attackCooldown = 1;
            UseAOEAttack();
        }
        else if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            ++currentMp;
        }
    }

    private void UseAOEAttack()
    {
        if (target != null)
        {
            if (explosionParticle != null)
            {
                explosionParticle.transform.position = target.transform.position;
                explosionParticle.Play();
            }
            if (explosionSound != null)
            {
                audioSource.PlayOneShot(explosionSound, 0.2f);
            }

            target.TakeDamage(atk);
            --currentMp;

            Collider[] others = Physics.OverlapSphere(target.transform.position, 2f);

            foreach (var hit in others)
            {
                IHealth hitHp = hit.GetComponent<IHealth>();
                if (hitHp != null)
                {
                    hitHp.TakeDamage(atk - 3);
                    damageDealt += atk;
                }
            }

            target.TakeDamage(atk);
            damageDealt += atk;

            if (rangedSound != null) audioSource.PlayOneShot(rangedSound, 0.2f);
        }
        else
        {
            ChangeState(Move);
        }
    }
}
