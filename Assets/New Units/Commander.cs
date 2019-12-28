using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander : SmartEnemy
{
    BaseUnit coverUnit;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Move()
    {
        Debug.Log("Moving");

        if(timePassed  < 15)
        {
            TakeDamage(hp);
        }

        if(coverUnit != null)
        {
            base.Move();
        }
        else
        {
            Search();
        }
    }

    protected void Search()
    {
        RaycastHit hit;

        if (Physics.BoxCast(transform.position, new Vector3(1, 1, 3), transform.right, out hit, Quaternion.identity, 2f))
        {
            if (hit.transform.tag.Equals("Enemy"))
            {
                coverUnit = hit.transform.GetComponent<BaseUnit>();
            }
        }
    }

    protected override void Retreat()
    {
        Debug.Log("Retreating " + Vector3.Distance(transform.position, Vector3.zero));
        if(timePassed > 15)
        {
            TakeDamage(hp);
        }

        if (Vector3.Distance(transform.position, Vector3.zero) >= 9) // Find correct turret range
        {
            ChangeState(Move); // Rewrite to feature AI and match flow of states
        }

        rb.MovePosition(transform.position + (-transform.right * ((Speed / 5) * Time.deltaTime))); // Speed is faster as divided by 5 instead of 10 
    }

    protected override void isLastUnit()
    {
        if (this != null)
            Destroy(gameObject, 1f);
    }

    public override void TakeDamage(int val)
    {
        base.TakeDamage(val);

        ChangeState(Retreat); // Enemy retreats if hit
    }


}
