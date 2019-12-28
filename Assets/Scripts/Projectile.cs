using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int pAtk;
    public float pSpeed;
    public BaseUnit targetedUnit;
    public TurretType pType;
    public LayerMask enemyLayer;

    private void Update() // add destruction over a certain distance for hang back mechanic and Teleport
    {
        if (targetedUnit != null)
            transform.position = Vector3.MoveTowards(transform.position, targetedUnit.transform.position, (float)pSpeed / 100);
        else
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
            if (other.name == targetedUnit.name)
            {
                IHealth enemyHp = other.GetComponent<IHealth>();
                switch (pType)
                {
                    case TurretType.Normal:
                        {
                            enemyHp.TakeDamage(pAtk);
                        }
                        break;
                    case TurretType.Multi_Attack:
                        {
                            enemyHp.TakeDamage(pAtk - 2);
                        }
                        break;
                    case TurretType.Grenadier:
                        {
                            Collider[] others = Physics.OverlapSphere(transform.position, 1.1f, enemyLayer);

                            foreach (var hit in others)
                            {
                                IHealth hitHp = hit.GetComponent<IHealth>();
                                if (hitHp != null)
                                {
                                    hitHp.TakeDamage(pAtk - 3);
                                }
                            }

                            enemyHp.TakeDamage(pAtk);
                        }
                        break;
                }

                Destroy(gameObject);
            }
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
