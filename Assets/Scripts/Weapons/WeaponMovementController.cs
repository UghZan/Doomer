using UnityEngine;

public class WeaponMovementController : MonoBehaviour
{
    private WeaponItem _weaponStats;

    [Header("Sway Settings")]
    public float maxSway;
    public float swayFactor;
    public float swayRotateFactor;
    public float swayDamp;
    public float swayRotateDamp;

    [Header("Bob Settings")]
    public float bobAmount;
    public float bobSpeed;

    [Header("Breath Settings")]
    public float breathAmount;
    public float breathSpeed;

    [Header("Wall Push Settings")]
    public float wallCheckDistance;
    public float pushChangeSpeed;
    public float wallPushRotAngle;

    [Header("Aimpunch Settings")]
    public float returnSpeed = 5f;
    public float changeSpeed = 10f;

    Vector3 position;
    public bool isLocked;
    public bool reloading;

    //position
    private Vector3 swayAdditive;
    private Vector3 walkAdditive;
    private Vector3 breathAdditive;
    private Vector3 wallpushOffset;
    private Vector3 reloadOffset;

    //rotation
    private Quaternion rotSwayAdditive;

    //recoil
    private Vector3 rotationRecoil;
    private Vector3 positionRecoil;
    private Vector3 rotDelta;

    // Use this for initialization
    void Start()
    {
        position = transform.localPosition;
    }
    public void InitStats(WeaponItem stats)
    {
        _weaponStats = stats;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isLocked)
        {
            rotationRecoil = Vector3.Lerp(rotationRecoil, Vector3.zero, Time.deltaTime * returnSpeed);
            positionRecoil = Vector3.Lerp(positionRecoil, Vector3.zero, Time.deltaTime * returnSpeed);
            reloadOffset = Vector3.Lerp(reloadOffset, (reloading ? Vector3.down : Vector3.zero), Time.deltaTime * changeSpeed);

            rotDelta = Vector3.Slerp(rotDelta, rotationRecoil, Time.deltaTime * changeSpeed);
            transform.localRotation = Quaternion.Euler(rotDelta) * rotSwayAdditive * Quaternion.Euler(new Vector3(wallpushOffset.z*wallPushRotAngle,0,0));
            transform.localPosition = position + walkAdditive + breathAdditive + swayAdditive + positionRecoil + wallpushOffset + reloadOffset;

            Sway();

            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                Walk();
            else
                Breath();

            Wallpush();
        }
    }

    public void Sway()
    {
        float fX = Input.GetAxisRaw("Mouse X") * swayFactor;
        float fY = Input.GetAxisRaw("Mouse Y") * swayFactor;

        float rX = PlayerInputController.GetMovementVector().x;

        fX = Mathf.Clamp(fX, -maxSway, maxSway);
        fY = Mathf.Clamp(fY, -maxSway, maxSway);

        Vector3 final = new Vector3(fX, fY, 0);
        Vector3 rotate = Vector3.forward * (-rX * swayRotateFactor);
        swayAdditive = Vector3.Lerp(swayAdditive, final, swayDamp);
        rotSwayAdditive = Quaternion.Lerp(rotSwayAdditive, Quaternion.Euler(rotate), swayRotateDamp);
    }

    public void Walk()
    {
        float xBob = 2 * Mathf.Sin(Time.time * bobSpeed + Mathf.PI / 2) * bobAmount;
        float yBob = Mathf.Sin(Time.time * 2 * bobSpeed) * bobAmount;

        Vector3 final = new Vector3(xBob, yBob, 0);
        walkAdditive = Vector3.Lerp(walkAdditive, final, 0.1f * Time.deltaTime);
    }
    public void Wallpush()
    {
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if(!hit.transform.CompareTag("Player")) wallpushOffset = Vector3.Lerp(wallpushOffset, Vector3.back * (wallCheckDistance - hit.distance), Time.deltaTime * pushChangeSpeed);
        }
        else
        {
            wallpushOffset = Vector3.Lerp(wallpushOffset, Vector3.zero, Time.deltaTime*pushChangeSpeed);
        }
    }

    public void Breath()
    {
        walkAdditive = Vector3.Lerp(walkAdditive, Vector3.zero, Time.deltaTime * bobSpeed);
        float yBob = Mathf.Sin(Time.time * 2 * breathSpeed) * breathAmount;

        Vector3 final = new Vector3(0, yBob, 0);
        breathAdditive = Vector3.Lerp(breathAdditive, final, 0.1f * Time.deltaTime);
    }

    public void Punch(float coef)
    {
        float fX = Random.Range(-_weaponStats.horizontalRotPunch, _weaponStats.horizontalRotPunch);
        float fY = Random.Range(-_weaponStats.verticalRotPunch * _weaponStats.negativeVerticalReduction, _weaponStats.verticalRotPunch);

        Vector3 rotate = Vector3.up * fX + Vector3.left * fY;
        Vector3 pushBack = Vector3.back * _weaponStats.pushBackForce;

        rotationRecoil += rotate * coef;
        positionRecoil = pushBack;
    }
}
