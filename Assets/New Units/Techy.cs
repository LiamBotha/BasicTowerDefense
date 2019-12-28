using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Techy : SmartEnemy
{

    [SerializeField] ParticleSystem explosionParticle;
    [SerializeField] AudioClip explosionSound;

    bool exploding = false;

    protected override void Attack()
    {
        if(exploding != true)
        {
            exploding = true;
            if (explosionParticle != null)
            {
                explosionParticle.Play();
            }
            if (explosionSound != null)
            {
                audioSource.PlayOneShot(explosionSound, 0.2f);
            }

            Collider[] others = Physics.OverlapSphere(transform.position, 2.5f);

            foreach (var hit in others)
            {
                IHealth hitHp = hit.GetComponent<IHealth>();
                if (hitHp != null)
                {
                    hitHp.TakeDamage(atk);
                    damageDealt += atk;
                }
            }

            anim.SetInteger("AnimState", 3);
            Destroy(gameObject, 0.8f);
        }
    } // fix running away while exploding
}
