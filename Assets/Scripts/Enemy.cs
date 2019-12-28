using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : BaseUnit
{
    [SerializeField] protected float enemyRange;
    [SerializeField] protected int enemyValue;
    [SerializeField] protected LayerMask targetLayer;

    private Lane enemyLane; // may use later to find nearest enemy rather than raycast.
    protected BaseUnit target;

    protected Rigidbody rb;

    private Action CurrentState = delegate { };

    public static event Action<int,BaseUnit> OnAnyEnemyKilled = delegate { };

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        CurrentState = Walk;
    }

    private void Update()
    {
        CurrentState();
    }

    private void OnDestroy()
    {
        OnAnyEnemyKilled(enemyValue,this);
    }

    protected void ChangeState(Action NextState)
    {
        CurrentState = NextState;
    }

    protected virtual void Walk()
    {
        rb.MovePosition(transform.position + (transform.right * ((Speed / 10) * Time.deltaTime)));
    }

    protected virtual void Attack()
    {

    } //REDO How it finds Turrets

    //protected bool IsInRange()
    //{
    //    RaycastHit hit;

    //    if(Physics.BoxCast(transform.position, new Vector3(0.1f, 2, 3.5f), transform.right, out hit, Quaternion.identity, enemyRange,targetLayer))
    //    {
    //        if(hit.transform != null && hit.transform.tag.Equals("Ally"))
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    protected bool IsRaycastRange()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.right,out hit,enemyRange,targetLayer))
        {
            if (hit.transform != null && hit.transform.tag.Equals("Ally"))
            {
                return true;
            }
        }

        return false;
    }

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
}
