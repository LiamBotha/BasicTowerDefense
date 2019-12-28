using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Turret : BaseUnit
{
    public Lane turretLane;

    [SerializeField] LayerMask targetLayer; 

    private float turretCooldown;

    private int turretLevel = 1;

    private TurretType type = TurretType.Normal;

    [SerializeField] Material defaultMat;
    [SerializeField] Material multiAttackMat;
    [SerializeField] Material grenadierMat;

    MeshRenderer mRender;

    public TurretType Type
    {
        get
        {
            return type;
        }
    }

    public static event Action<Turret> OnAnyTurretDestroyed = delegate { };

    private void Start()
    {
        mRender = GetComponentInChildren<MeshRenderer>();
        turretCooldown = 10 / Speed;
    }

    private void Update()
    {
        TurretAttack();
    }

    private void TurretAttack()
    {
        if(Type != TurretType.Multi_Attack)
        {
            if (turretCooldown <= 0)
            {
                turretCooldown = 10 / Speed;
                GameObject targetObj = FindEnemyWithSphere();
                if (targetObj == null)
                    targetObj = FindNearestEnemy();
                if (targetObj != null)
                {
                    IHealth target = targetObj.GetComponent<IHealth>();
                    if (target != null)
                    {
                        FireProjectile(targetObj,Type,speed);
                    }
                }
            }
            else
            {
                turretCooldown -= Time.deltaTime;
            }
        }
        else
        {            
            if (turretCooldown <= 0)
            {
                turretCooldown = 10 / Speed;

                GameObject[] targetObjs = FindNearestEnemies();
                foreach (var targetObj in targetObjs)
                {
                    IHealth target = targetObj.GetComponent<IHealth>();
                    if (target != null)
                    {
                        FireProjectile(targetObj, Type);
                    }
                }
            }
            else
            {
                turretCooldown -= Time.deltaTime;
            }
        }
    }    

    private GameObject FindEnemyWithSphere()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.7f, targetLayer);

        if(hits.Length != 0)
        {
            return hits[0].gameObject;
        }

        return null;
    }

    private GameObject[] FindEnemiesWithSphere()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.7f, targetLayer);

        if (hits.Length != 0)
        {
            GameObject[] targets = new GameObject[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                targets[i] = hits[i].gameObject;
            }

            return targets;
        }

        return null;
    }

    private GameObject FindNearestEnemy()
    {
        RaycastHit hit;
        var position = Vector3.zero;

        if(turretLane == Lane.Left)
        {
            position = new Vector3(-3,0.53f,0);
        }
        else if (turretLane == Lane.Right)
        {
            position = new Vector3(3, 0.53f, 0);
        }
        else if (turretLane == Lane.Bottom)
        {
            position = new Vector3(0, 0.53f, -3);
        }
        else if (turretLane == Lane.Top)
        {
            position = new Vector3(0, 0.53f, 3);
        }

        if (Physics.BoxCast(position,new Vector3(0.5f,4,2.5f), -transform.right,out hit,transform.rotation,10f,targetLayer))
        {
            if(hit.transform.tag.Equals("Enemy"))
            {
                return hit.transform.gameObject;
            }
        }

        return null;
    }

    private GameObject[] FindNearestEnemies()
    {
        List<GameObject> targets = new List<GameObject>();
        var position = Vector3.zero;

        if (turretLane == Lane.Left)
        {
            position = new Vector3(-3, 0.53f, 0);
        }
        else if (turretLane == Lane.Right)
        {
            position = new Vector3(3, 0.53f, 0);
        }
        else if (turretLane == Lane.Bottom)
        {
            position = new Vector3(0, 0.53f, -3);
        }
        else if (turretLane == Lane.Top)
        {
            position = new Vector3(0, 0.53f, 3);
        }

        RaycastHit[] hits = Physics.BoxCastAll(position, new Vector3(0.5f, 4, 2.5f), -transform.right,transform.rotation, 10f,targetLayer);

        foreach (var hit in hits)
        {
            if (hit.transform.tag.Equals("Enemy"))
            {
                targets.Add(hit.transform.gameObject);
            }
            else
            {
                Debug.Log(hit.transform.gameObject);
            }
        }

        return targets.ToArray();
    }

    public void ChangeTurretType(TurretType newType)
    {
        type = newType;
        
        charName = newType.ToString().Replace('_',' ') + " Turret";

        switch (newType)
        {
            case TurretType.Normal:
                {
                    mRender.material = defaultMat;
                }
                break;
            case TurretType.Multi_Attack:
                {
                    description = "A powerful speaker that hits multiple targets";
                    mRender.material = multiAttackMat;
                }
                break;
            case TurretType.Grenadier:
                {
                    description = "A pumping speakers that unleashes bombing beats";
                    mRender.material = grenadierMat;
                }
                break;
        }

        if(Type == TurretType.Multi_Attack)
        {
            
        }
    }

    public void SelectTurret()
    {
        base.OnMouseDown();
    }

    private void OnDestroy()
    {
        OnAnyTurretDestroyed(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, 1.7f);
    }
}
