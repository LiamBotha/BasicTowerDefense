using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrankensteinEnemy : SmartEnemy
{
    //Behaviour Variables
    private bool isMelee;

    private int agressiveness = 1; // 1,2, or 3 // TODO - Add changes to behaviour based on agressiveness

    bool canTurnInvis; //
    bool canTeleport; //
    bool canUseCover; //
    bool canRetreat; //
    bool canSuicide; //
    bool hasAOE; //

    bool isInvisible;
    bool isTeleporting;

    //Method Variables
    private float teleportCountdown = 8;

    private string[] baseNames = new string[] {"Mech","Bot","Unit", "System", "Bob"};

    public Transform[] teleportLanes;
    BaseUnit coverUnit;
    GameManager manager;

    [SerializeField] GameObject ScoutModel;
    [SerializeField] GameObject VanguardModel;
    [SerializeField] GameObject CommanderModel;
    [SerializeField] GameObject TechyModel;
    [SerializeField] GameObject WizardModel;

    [SerializeField] ParticleSystem explosionParticle;
    [SerializeField] ParticleSystem attackParticle;

    [SerializeField] AudioClip explosionSound;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        teleportLanes = GameObject.Find("Spawners").GetComponentsInChildren<Transform>();

        SetCharacteristics();
        SetName();

        ChangeState(Move);
    }
    protected override void Update()
    {
        timePassed += Time.deltaTime;

        base.Update();
    }

    private void SetCharacteristics()// Instantiates monster with its behaviours
    {
        isMelee = UnityEngine.Random.Range(0, 2) != 0;

        if(isMelee)
        {
            maxHp = 120;
            maxMp = 10;
            atk = 5;
            enemyRange = 2;

            int rand = UnityEngine.Random.Range(0, 5);

            SelectAbility(rand);

            int nextRand = UnityEngine.Random.Range(0, 5);

            while ((rand == 0 && nextRand == 4) || (rand == 2 && nextRand == 4) || nextRand == rand)
            {
                nextRand = UnityEngine.Random.Range(0, 5);
            } // Invisible  & Suicide , Cover & Suicide

            SelectAbility(nextRand);
        }
        else
        {
            maxHp = 80;
            maxMp = 30;
            atk = 3;
            enemyRange = 5;

            int rand = UnityEngine.Random.Range(0, 6);

            SelectAbility(rand);

            int nextRand = UnityEngine.Random.Range(0, 6);

            while((rand == 0 && nextRand == 4) || (rand == 2 && nextRand == 4) || nextRand == rand)
            {
                nextRand = UnityEngine.Random.Range(0, 6);
            } // Invisible  & Suicide ,  Cover & Suicide

            SelectAbility(nextRand);
        }


        speed = 5 * agressiveness;
        hp = maxHp;
        currentMp = maxMp;
    }

    private void SetName()
    {
        string val = "";
        
        if(canSuicide)
        {
            val += "Expendable ";
        }

        if(canTeleport)
        {
            val += "Prototype ";
        }

        int rand = UnityEngine.Random.Range(0, baseNames.Length);

        val += baseNames[rand];

        if(canUseCover)
        {
            val += " H-30";
        }

        if(canRetreat)
        {
            val += " Tactical";
        }

        if(hasAOE)
        {
            val += " ver-2.0";
        }

        if(canTurnInvis)
        {
            val += " MK4";
        }

        charName = val;
    }

    private void SelectAbility(int val)
    {
        switch (val)
        {
            case 0:
                {
                    canTurnInvis = true;
                    agressiveness += 1;
                    maxHp -= 20;
                    maxMp += 20;

                    ScoutModel.SetActive(true);
                    break;
                }
            case 1:
                {
                    canTeleport = true;
                    maxMp += 10;
                    VanguardModel.SetActive(true);
                    break;
                }
            case 2:
                {
                    canUseCover = true;
                    CommanderModel.SetActive(true);
                    break;
                }
            case 3:
                {
                    canRetreat = true;
                    CommanderModel.SetActive(true);
                    break;
                }
            case 4:
                {
                    canSuicide = true;
                    agressiveness += 2;
                    maxHp /= 2;
                    TechyModel.SetActive(true);
                    break;
                }
            case 5:
                {
                    hasAOE = true;
                    atk = 2;
                    maxMp += 50;
                    WizardModel.SetActive(true);
                    break;
                }
        }
    }

    private IEnumerator TurnInvisible()
    {
        if (!isInvisible && currentMp > 5) // Rebalance cost of ability
        {
            isInvisible = true;
            gameObject.tag = "Untagged";
            gameObject.layer = LayerMask.NameToLayer("Default"); // cant be detected if not on attack layer
            --currentMp;

            yield return new WaitForSeconds(3f);

            isInvisible = false;
            gameObject.tag = "Enemy";
            gameObject.layer = LayerMask.NameToLayer("Enemies");
        }
    } // Scout

    private void CastTeleport()
    {
        isTeleporting = true;
        --currentMp;

        if (teleportCountdown <= 0)
        {
            isTeleporting = false;
            teleportCountdown = 2;
            Teleport();
        }
        else
        {
            teleportCountdown -= Time.deltaTime;
            if (tookDamage)
            {
                tookDamage = false;
                isTeleporting = false;
                return;
            }
        }
    } // Keeps restarting TP till it succeeds when hit.

    private void Teleport()
    {
        anim.SetInteger("animStates", 4);

        int val = manager.GetLeastTurrets();

        currentLane = val;

        transform.position = teleportLanes[val + 1].position;
        transform.rotation = teleportLanes[val + 1].rotation;
        ChangeState(Move);

    } // Vanguard - Add casting teleport to special method

    private void Search()
    {
        RaycastHit hit;

        if (Physics.BoxCast(transform.position, new Vector3(1, 5, 5), transform.right, out hit, Quaternion.identity, 3f))
        {
            if (hit.transform.tag.Equals("Enemy"))
            {
                coverUnit = hit.transform.GetComponent<BaseUnit>();
            }
        }

        timePassed = 0;
    } // Commander

    private void CommanderRetreat() 
    {
        anim.SetInteger("AnimState", -1);

        if (Vector3.Distance(transform.position, Vector3.zero) >= 9) // Find correct turret range
        {
            ChangeState(Move);
        }

        rb.MovePosition(transform.position + (-transform.right * ((Speed / 5) * Time.deltaTime))); // Speed is faster as divided by 5 instead of 10 
    } // Commander

    private void Explode()
    {
        //if (explosionParticle != null)
        //{
        //    explosionParticle.Play();
        //}

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

        Destroy(gameObject, 0.8f);
    } // Techy

    protected void AOEAttack()
    {
        currentMp -= 5;
        target.TakeDamage(atk);

        if (explosionParticle != null)
        {
            explosionParticle.transform.position = target.transform.position;
            explosionParticle.Play();
        }

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

        audioSource.PlayOneShot(explosionSound, 0.05f);
    } // Wizard

    //States
    protected override void Move()
    {
        Debug.DrawRay(transform.position, transform.right);
        anim.SetInteger("AnimState", 1);

        if (walkSound != null && audioSource.clip != walkSound)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.volume = 0.1f;
            audioSource.Play();
        }

        if (canUseCover == false || (canUseCover && coverUnit != null && timePassed < 10))
        {
            rb.MovePosition(transform.position + (transform.right * ((Speed / 10) * Time.deltaTime)));
        }
        else if(canUseCover)
        {
            if (timePassed < 15)
            {
                TakeDamage(hp);
            }

            anim.SetInteger("AnimState", 4);
            Search();
        }

        BaseUnit foundTarget = GetRaycastEnemy();
        if (foundTarget != null)
        {
            target = foundTarget;
            ChangeState(Attack);
        }
    }

    protected override void Attack()
    {
        anim.SetInteger("AnimState", 2);

        if (target != null)
        {
            if (attackCooldown <= 0)
            {
                attackCooldown = 1;
                if (canSuicide)
                {
                    Explode();
                }
                if (isMelee)
                {
                    IHealth targetHp = target.GetComponent<IHealth>();
                    targetHp.TakeDamage(Atk);
                    damageDealt += atk;

                    audioSource.PlayOneShot(meleeSound, 0.05f);
                }
                else if (hasAOE)
                {
                    if (currentMp > 5)
                    {
                        AOEAttack();
                    }
                    else
                    {
                        currentMp++;
                    }
                }
                else
                {
                    if (currentMp > 5)
                    {

                        if (attackParticle != null)
                            attackParticle.Play();
                        IHealth targetHp = target.GetComponent<IHealth>();
                        targetHp.TakeDamage(Atk);
                        damageDealt += atk;

                        audioSource.PlayOneShot(rangedSound, 0.05f);
                    }
                    else
                    {
                        currentMp++;
                    }
                }
            }
            else
            {
                attackCooldown -= Time.deltaTime;
            }
        }
        else
        {
            ChangeState(Move);
        }
    }

    protected override void Special()
    {
        CastTeleport();
    }

    // Interface
    public override void TakeDamage(int val)
    {
        base.TakeDamage(val);

        if(canTurnInvis)
        {
            StartCoroutine(TurnInvisible());
        }

        if (canRetreat)
        {
            ChangeState(CommanderRetreat);
        }
        else if (canTeleport & currentMp >= 5)
        {
            Special();
        }
    }

    protected override void Death()
    {
        anim.SetInteger("AnimState", 3);

        base.Death();
    }

    protected override void isLastUnit()
    {
        if (this != null)
            Destroy(gameObject, 1f);
    }

    public string Descript()
    {
        string val = charName + Environment.NewLine;
        val += "Is Melee: " + isMelee + Environment.NewLine;
        val += "Has Invis: " + canTurnInvis + Environment.NewLine;
        val += "Has Teleport: " + canTeleport + Environment.NewLine;
        val += "Has Cover: " + canUseCover + Environment.NewLine;
        val += "Has Retreat: " + canRetreat + Environment.NewLine;
        val += "Has Suicide: " + canSuicide + Environment.NewLine;
        val += "HP: " + maxHp + Environment.NewLine;
        val += "MP: " + maxMp + Environment.NewLine;

        return val;
    }
}
