using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
    public WeaponItem weaponStats;
    public bool nerfAgainstBoss;

    [Header("Visual Parameters")]
    public ParticleSystem shoot;
    public GameObjectPool tracers;
    public bool reloadInProgress = false;
    public int currentAmmo { get; protected set; }

    [Header("Audio Parameters")] 
    public AudioClip[] shootSounds;
    public AudioClip reloadSound;

    [Header("Aim Parameters")]
    public bool usePhysicalSystem;
    public Transform aimMuzzlePoint;
    public Vector3 aimEndPoint { get; protected set; }
    public float factualAimDistance { get; protected set; }

    [Header("References")]
    [SerializeField] protected Animator animator;
    protected WeaponRecoilController wrc;
    protected WeaponMovementController wmc;
    [SerializeField] protected AudioSource audioSource;
    protected UnityEngine.Camera cam;
    protected PlayerStats owner;

    protected float timer = 0f;
    private bool isLocked;
    protected bool nextCrit;

    protected abstract void PrimaryFire();
    protected abstract void SecondaryFire();
    protected virtual void Reload() {StartCoroutine(Reloading()); }

    protected IEnumerator Reloading()
    {
        reloadInProgress = true;
        wmc.reloading = true;
        yield return new WaitForSeconds(weaponStats.reloadTime);
        RefreshAmmo();
        reloadInProgress = false;
        wmc.reloading = false;
    }

    protected virtual void Start()
    {
        if (wmc == null) wmc = GetComponent<WeaponMovementController>();
        if (wrc == null) wrc = GetComponent<WeaponRecoilController>();

        wrc.InitStats(weaponStats);
        wmc.InitStats(weaponStats);

        if(audioSource == null) audioSource = GetComponent<AudioSource>();
        cam = UnityEngine.Camera.main;
        currentAmmo = weaponStats.ammoMax;
        owner = GetComponentInParent<PlayerStats>();
    }

    protected void PlayShootSound()
    {
        audioSource.PlayOneShot(shootSounds[Random.Range(0,shootSounds.Length)]);
        audioSource.pitch = Random.Range(0.9f,1.1f);
    }

    protected virtual void Update()
    {
        timer += Time.deltaTime;

        if (timer > 1 / weaponStats.shootSpeed && currentAmmo > 0 && !reloadInProgress)
        {
            if (weaponStats.isAutomatic)
            {
                if (Input.GetMouseButton(0))
                {
                    if (Random.value < weaponStats.muzzleChance) shoot.Play();

                    PrimaryFire();

                    wrc.Recoil();

                    timer = 0;

                    currentAmmo -= weaponStats.ammoPerShot;

                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Random.value < weaponStats.muzzleChance) shoot.Play();

                    PrimaryFire();

                    wrc.Recoil();

                    timer = 0;

                    currentAmmo -= weaponStats.ammoPerShot;
                }
            }
        }

        if(Input.GetMouseButton(1))
        {
            SecondaryFire();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < weaponStats.ammoMax && !reloadInProgress)
        {
            Reload();
        }

        if(usePhysicalSystem) GetAimPoint();
    }

    protected void GetAimPoint()
    {
        Vector3 direction = transform.forward;
        Vector3 muzzlePoint = aimMuzzlePoint.position;
        if(Physics.Raycast(muzzlePoint, direction, out RaycastHit hit, weaponStats.range))
        {
            factualAimDistance = hit.distance;
            aimEndPoint = hit.point;
        }
        else
        {
            factualAimDistance = weaponStats.range;
            aimEndPoint = muzzlePoint + transform.forward * weaponStats.range;
        }
    }

    public virtual void RefreshAmmo()
    {
        currentAmmo = weaponStats.ammoMax;
    }

    protected virtual void SendDamage()
    {
        float spr = weaponStats.weaponSpread;
        Ray ray = cam.ViewportPointToRay(Vector2.one/2 + new Vector2(Random.Range(-spr,spr)/Screen.width, Random.Range(-spr,spr)/Screen.height));
        bool isHit = false;
        LayerMask lm = LayerMask.GetMask("PlayerBarrier");
        if(Physics.Raycast(ray, out RaycastHit hit, weaponStats.range, ~lm))
        {
            if(hit.collider.TryGetComponent<IDamagePoint>(out IDamagePoint ed)) 
            {
                float bossMul = 1f;
                if(ed is EnemyData) bossMul = ((ed as EnemyData).isBoss && nerfAgainstBoss) ? 0.5f : 1f;
                ed.SendDamage(owner, hit.point, GetDamage() * (nextCrit ? 2f : 1f) * bossMul, nextCrit);
            }
            isHit = true;
        }
        GameObject tracerO = tracers.GetPooledObject();
        LineRenderer tracer = tracerO.GetComponent<LineRenderer>();
        tracer.SetPosition(0, aimMuzzlePoint.position);
        if(isHit) tracer.SetPosition(1, hit.point);
        else tracer.SetPosition(1, aimMuzzlePoint.position + ray.direction * weaponStats.range);
        tracerO.SetActive(true);
    }

    protected float GetDamage()
    {
        nextCrit = Random.value < (owner.critChance + weaponStats.critChanceBonus);
        return (weaponStats.damage + Random.Range(0, weaponStats.damageVariance))*DifficultyManager.playerDamageMultiplier;
    }
}
