using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : BaseUnit
{
    private float atkCooldown;

    private bool leftEmpty = false;
    private bool rightEmpty = false;
    private bool topEmpty = false;
    private bool bottomEmpty = false;

    private bool attackedLeft = false;
    private bool attackedRight = false;
    private bool attackedTop = false;
    private bool attackedBottom = false;

    private int numOfEmptyLanes = 0;
    private int unattackedLanes = 0;

    public LayerMask enemyLayer;

    public event Action OnPlayerBaseDestroyed = delegate { };

    private void Update()
    {
        if (atkCooldown <= 0)
        {
            atkCooldown = Speed;
            Attack();
        }
        else
        {
            atkCooldown -= Time.deltaTime;
        }
    }

    void Attack()
    {
        if(numOfEmptyLanes == 1)
        {
            GameObject target;

            if (leftEmpty)
            {
                target = FindNearestEnemy(-transform.right);
            }
            else if (rightEmpty)
            {
                target = FindNearestEnemy(transform.right);
            }
            else if (topEmpty)
            {
                target = FindNearestEnemy(transform.forward);
            }
            else if (bottomEmpty)
            {
                target = FindNearestEnemy(-transform.forward);
            }
            else
            {
                target = null;
            }

            if(target != null)
            {
                //target.TakeDamage(Atk);
                FireProjectile(target);
            }
        }
        else if(numOfEmptyLanes > 1)
        {
            GameObject target;
            if (leftEmpty && attackedLeft == false)
            {
                target = FindNearestEnemy(-transform.right);
                attackedLeft = true;
                --unattackedLanes;

                Debug.Log("Left");
            }
            else if (rightEmpty && attackedRight == false)
            {
                target = FindNearestEnemy(transform.right);
                attackedRight = true;
                --unattackedLanes;

                Debug.Log("Right");
            }
            else if (topEmpty && attackedTop == false)
            {
                target = FindNearestEnemy(transform.forward);
                attackedTop = true;
                --unattackedLanes;

                Debug.Log("Top");
            }
            else if (bottomEmpty && attackedBottom == false)
            {
                target = FindNearestEnemy(-transform.forward);
                attackedBottom = true;
                --unattackedLanes;

                Debug.Log("Bottom");
            }
            else target = null;

            if (target != null)
            {
                FireProjectile(target);
                //target.TakeDamage(Atk);
            }

            if(unattackedLanes <= 0)
            {
                Debug.Log("Reset Lanes");
                attackedLeft = false;
                attackedRight = false;
                attackedTop = false;
                attackedBottom = false;

                unattackedLanes = numOfEmptyLanes;
            }
        }
    }

    private GameObject FindNearestEnemy(Vector3 direction)
    {
        RaycastHit hit;

        Debug.DrawRay(transform.position, direction,Color.red);
        if (Physics.BoxCast(transform.position, new Vector3(1, 1, 3), direction, out hit, Quaternion.identity, 12f,enemyLayer))
        {
            if (hit.transform.tag.Equals("Enemy"))
            {
                return hit.transform.gameObject;
            }
            else
            {
                Debug.Log(hit.transform.name);
            }
        }
        return null;
    }

    public void LaneEmpty(Lane lane)
    {
        switch (lane)
        {
            case Lane.Left:
                {
                    leftEmpty = true;
                }
                break;
            case Lane.Right:
                {
                    rightEmpty = true;
                }
                break;
            case Lane.Top:
                {
                    topEmpty = true;
                }
                break;
            case Lane.Bottom:
                {
                    bottomEmpty = true;
                }
                break;
            default:
                break;
        }

        ++numOfEmptyLanes;
        unattackedLanes += 1;
    }

    private void OnDestroy()
    {
        OnPlayerBaseDestroyed();
    }

}
