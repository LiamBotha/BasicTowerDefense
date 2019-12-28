using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseUnit : MonoBehaviour, IHealth
{
    [SerializeField] protected int hp;
    protected int maxHp;

    [SerializeField] protected int currentMp;
    protected int maxMp;

    [SerializeField] protected int atk;
    [SerializeField] protected float speed;

    [SerializeField] protected string charName;
    [SerializeField] protected string description;

    [SerializeField] protected GameObject projectilePrefab;

    [NonSerialized] public float timePassed = 0;

    protected float attackCooldown = 1;

    protected bool tookDamage = false;

    private MeshRenderer meshR;
    private Material defaultMaterial;
    [SerializeField] Material selectedMaterial;
    [SerializeField] protected Animator anim;

    public Canvas hpCanvas;

    public Image healthBar;

    public int currentLane;

    public int Atk
    {
        get
        {
            return atk;
        }

        set
        {
            atk = value;
        }
    }

    public float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            speed = value;
        }
    }

    public int Hp { get => hp; }
    public int MaxHp { get => maxHp; }

    //Sound
    protected AudioSource audioSource;

    [Space]
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected AudioClip meleeSound;
    [SerializeField] protected AudioClip rangedSound;
    [SerializeField] protected AudioClip walkSound;

    public static event Action<BaseUnit> OnAnyUnitHover = delegate { };

    private void FixedUpdate()
    {
        timePassed += Time.deltaTime;

        hpCanvas.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(hpCanvas.transform.position, Camera.main.transform.position,100 * Time.deltaTime,0));
    }

    protected void OnMouseDown()
    {
        meshR.material = selectedMaterial;

        OnAnyUnitHover(this);
    }

    private void UnitSelected(BaseUnit unit)
    {
        if(unit != null && this != unit)
        {
            meshR.material = defaultMaterial;
        }
    }

    protected void FireProjectile(GameObject targetObj, TurretType type = TurretType.Normal,float pSpeed = 10)
    {
        var rot = targetObj.transform.rotation;
        Projectile proj = GameObject.Instantiate(projectilePrefab, transform.position,new Quaternion(rot.x,rot.y,rot.z,rot.w)).GetComponent<Projectile>();

        proj.targetedUnit = targetObj.GetComponent<BaseUnit>();
        proj.pAtk = atk;
        proj.pType = type;
        proj.pSpeed = pSpeed;
        proj.enemyLayer = targetObj.layer;

        Debug.DrawLine(transform.position, targetObj.transform.position);
    }

    public override string ToString()
    {
        string info = (charName + ";Description: "+ Environment.NewLine + Environment.NewLine + description + ";Hp: " + hp + "/" + maxHp + ";Atk: " + Atk + ";Speed: " + Speed);
        return info;
    }

    private void OnEnable()
    {
        OnAnyUnitHover += UnitSelected;
        GameManager.OneRemainingEnemy += isLastUnit;

        meshR = GetComponent<MeshRenderer>();
        if (meshR == null)
        {
            meshR = GetComponentInChildren<MeshRenderer>();
        }
        audioSource = GetComponent<AudioSource>();
        hpCanvas = GetComponentInChildren<Canvas>();
        defaultMaterial = meshR.material;
        maxHp = hp;

        hpCanvas.transform.eulerAngles = new Vector3(40, 0, 0);
    }

    private void OnDisable()
    {
        OnAnyUnitHover -= UnitSelected;
        GameManager.OneRemainingEnemy -= isLastUnit;
    }

    #region Interfaces

    public virtual void TakeDamage(int val)
    {
        tookDamage = true;

        if (hp > 0)
        {
            hp -= val;

            if (healthBar != null)
                healthBar.fillAmount = ((float)hp / maxHp);
        }
        else
        {
            Death();
        }
    }

    protected virtual void Death()
    {
        if (anim != null)
            anim.SetInteger("AnimState", 3);

        if(deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Destroy(gameObject,0.6f);
    }

    protected virtual void isLastUnit()
    {

    }

    public void GainHealth(int val)
    {
        hp += val;
    }

    #endregion Interfaces

}
