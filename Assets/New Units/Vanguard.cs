using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vanguard : SmartEnemy
{
    private float teleportCountdown = 2;

    private bool isTeleporting;

    [SerializeField] Transform[] teleportLanes;

    GameManager manager;

    protected override void Start()
    {
        base.Start();

        manager = FindObjectOfType<GameManager>();
        teleportLanes = GameObject.Find("Spawners").GetComponentsInChildren<Transform>();
    }

    protected override void Move()
    {
        if(manager.GetLeastTurrets() != currentLane)
        {
            ChangeState(Special);
        }

        base.Move();
    }

    protected override void Attack()
    {
        if (currentMp > 5)
        {
            if (target != null)
            {
                Debug.Log(target.name);
                IHealth targetHp = target.GetComponent<IHealth>();
                currentMp -= 5;

                targetHp.TakeDamage(Atk); // Replace with projectile after fixing
                damageDealt += atk;

                if (rangedSound != null) audioSource.PlayOneShot(rangedSound, 0.2f);
            }
            else
            {
                ChangeState(Move);
            }
        }
    }

    protected override void Special()
    {
        isTeleporting = true;
        --currentMp;

        if (teleportCountdown <= 0)
        {
            isTeleporting = false;
            teleportCountdown = 2;
            Teleport(); // TODO Balance time
        }
        else
        {
            teleportCountdown -= Time.deltaTime;
            if(tookDamage)
            {               
                tookDamage = false;
                isTeleporting = false;
                return;
            }
        }
    }

    private void Teleport()
    {
        int val = manager.GetLeastTurrets();

        currentLane = val;

        transform.position = teleportLanes[val + 1].position;
        transform.rotation = teleportLanes[val + 1].rotation;
        ChangeState(Move);
    }

    public override void TakeDamage(int val)
    {
        base.TakeDamage(val);

        if(isTeleporting)
        {
            tookDamage = true;
        }
    }
}
