using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemy : BaseUnit
{
    [SerializeField] protected float enemyRange;
    [SerializeField] protected int enemyValue;
    [SerializeField] protected LayerMask targetLayer;

    protected BaseUnit nearestTurret;

    protected int damageDealt = 0;

    protected BaseUnit target;

    protected Rigidbody rb;

    protected Action CurrentState = delegate {};

    public static Action<int,BaseUnit> OnAnyEnemyKilled = delegate { };

    protected void ChangeState(Action NextState)
    {
        CurrentState = NextState;
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        CurrentState = Move;
    }

    protected virtual void Update()
    {
        CurrentState();
    }

    //States
    protected virtual void Move()
    {
        if (anim != null & anim.GetInteger("AnimState") != 1)
            anim.SetInteger("AnimState", 1);
        if (walkSound != null && audioSource.clip != walkSound)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.volume = 0.1f;
            audioSource.Play();
        }

        rb.MovePosition(transform.position + (transform.right * ((Speed / 10) * Time.deltaTime)));

        BaseUnit foundTarget = GetRaycastEnemy();
        if (foundTarget != null)
        {
            audioSource.clip = null;

            target = foundTarget;
            ChangeState(Attack);
        }
    }

    protected virtual void Attack()
    {
        if (anim != null)
            anim.SetInteger("AnimState", 2);

        if (attackCooldown <= 0)
        {
            attackCooldown = 1;

            if (target != null)
            {
                IHealth targetHp = target.GetComponent<IHealth>();
                targetHp.TakeDamage(Atk);
                damageDealt += atk;

                if (meleeSound != null) audioSource.PlayOneShot(meleeSound, 0.1f);
            }
            else
            {
                ChangeState(Move);
            }
        }
        else
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    protected virtual void Special()
    {
        //Do Action
    }

    protected virtual void Retreat()
    {
    }

    protected virtual void Idle()
    {

    }

    //Interfaces

    protected BaseUnit GetRaycastEnemy()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.right, out hit, enemyRange, targetLayer))
        {
            if (hit.transform != null && hit.transform.tag.Equals("Ally"))
            {
                return hit.transform.GetComponent<BaseUnit>();
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        OnAnyEnemyKilled(enemyValue, this);
    }
}
