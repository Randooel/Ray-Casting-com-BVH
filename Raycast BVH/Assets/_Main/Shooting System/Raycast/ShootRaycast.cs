using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootRaycast : MonoBehaviour
{
    #region Odin Inspector Buttons
    private bool _odinToggle;

    #region Using BVH Button
    [PropertyOrder(-10)]
    [HideIf("_odinToggle")]
    [Button(ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
    private void UsingBVH()
    {
        this._odinToggle = !this._odinToggle;
        _useBVH = true;
    }
    #endregion

    #region Not Using BVH Button
    [ShowIf("_odinToggle")]
    [PropertyOrder(-10)]
    [Button(ButtonSizes.Gigantic), GUIColor(1, 0, 0)]
    private void NotUsingBVH()
    {
        this._odinToggle = !this._odinToggle;
        _useBVH = false;
    }
    #endregion

    #endregion

    private Animator _animator;

    [SerializeField] private TextMeshProUGUI _feedbackText;
    [SerializeField] private TextMeshProUGUI _damageText;

    #region New Input System Actions
    [SerializeField] private InputActionAsset _inputAsset;
    private InputAction _shootAction;
    private InputAction _changeBVHUse;
    #endregion

    #region Variables
    [SerializeField] private bool _useBVH;

    private List<EnemiesBVH> _allEnemies = new List<EnemiesBVH>();

    [Space(15)]
    [SerializeField] private Transform _rayOrigin;
    [SerializeField, Range(1, 10)] private float _rayOffset;
    [SerializeField, Range(1, 100)] private float _rayDistance;
    [SerializeField] float _shootDamage = 20f;
    #endregion

    #region Body parts Multipliers
    [SerializeField] float _headMultiplier = 3f;
    [SerializeField] float bodyMultiplier = 1.0f;
    [SerializeField] float _limbsMultiplier = 0.5f;
    #endregion

    #region Ticks
    [HideInInspector] public long lastTicksBVH;
    [HideInInspector] public long lastTicksNoBVH;
    [HideInInspector] public long ticksDifference;

    [HideInInspector] public string lastHitLocation = "Nenhum";
    [HideInInspector] public float lastDamageDealt = 0f;
    [HideInInspector] public float lastMultiplier = 0f;
    #endregion

    #region Gun Statistics
    [SerializeField] private int _headshotsDealt = 0;
    [SerializeField] private int _killedEnemies = 0;
    [SerializeField] private int _aliveEnemies = 0;
    #endregion


    #region Setup
    private void OnEnable()
    {
        _shootAction = _inputAsset.FindAction("Attack");
        _shootAction.performed += Shoot;

        _changeBVHUse = _inputAsset.FindAction("Tab");
        _changeBVHUse.performed += ToggleBVHUse;
    }
    private void OnDisable()
    {
        _shootAction.performed -= Shoot;
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();

        _useBVH = true;
        RefreshEnemyList();
    }

    public void RefreshEnemyList()
    {
        _allEnemies = FindObjectsByType<EnemiesBVH>(FindObjectsSortMode.None).ToList();
        _aliveEnemies = _allEnemies.Count;
    }

    void Update()
    {
        _aliveEnemies = _allEnemies.Count(z => z != null);
    }
    #endregion


    #region Shoot Logic
    private void Shoot(InputAction.CallbackContext context)
    {
        /*
        Vector3 origin = _rayOrigin.position;
        Vector3 distance = new Vector3(0, 0, _rayDistance + _rayOffset);
        
        // Casting Ray
        //Physics.Raycast(origin, distance);
        */

        _animator.SetTrigger("shoot");

        _allEnemies.RemoveAll(z => z == null);
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Stopwatch swBVH = Stopwatch.StartNew();
        RaycastHit hitBVH = UseBVH(ray, out EnemiesBVH enemiesBVH, out bool foundBVH);
        swBVH.Stop();
        lastTicksBVH = swBVH.ElapsedTicks;

        Stopwatch swNoBVH = Stopwatch.StartNew();
        RaycastHit hitNoBVH = DontUseBVH(ray, out EnemiesBVH enemiesWithoutBVH, out bool foundNoBVH);
        swNoBVH.Stop();
        lastTicksNoBVH = swNoBVH.ElapsedTicks;
        ticksDifference = lastTicksNoBVH - lastTicksBVH;
        bool hitSuccess = _useBVH ? foundBVH : foundNoBVH;
        RaycastHit finalHit = _useBVH ? hitBVH : hitNoBVH;
        EnemiesBVH finalEnemie = _useBVH ? enemiesBVH : enemiesWithoutBVH;

        if (hitSuccess && finalEnemie != null)
        {
            ApplyDamage(finalHit, finalEnemie, ray);
        }
        else
        {
            lastHitLocation = "Missed";
            lastDamageDealt = 0;
            lastMultiplier = 0;
        }
    }

    void ApplyDamage(RaycastHit hit, EnemiesBVH enemie, Ray ray)
    {
        float finalDamage = CalculateDamage(hit.collider, out string hitType);

        EnemiesHealth health = enemie.GetComponent<EnemiesHealth>();
        if (health == null) health = enemie.GetComponentInParent<EnemiesHealth>();

        if (health != null)
        {
            health.TakeDamage(finalDamage);
            if (health.currentHealth <= 0) OnKill();
        }
        UnityEngine.Debug.DrawLine(ray.origin, hit.point, _useBVH ? Color.green : Color.red, 2f);
    }

    float CalculateDamage(Collider hitCol, out string hitType)
    {
        string name = hitCol.name.ToLower();
        float multiplier = bodyMultiplier;
        hitType = "Body";

        if (name.Contains("head"))
        {
            multiplier = _headMultiplier;
            hitType = "Headshot!";
            _headshotsDealt++;
        }
        else if (name.Contains("arm"))
        {
            multiplier = _limbsMultiplier;
            hitType = "Arm shot!";
        }
        else if(name.Contains("leg"))
        {
            multiplier = _limbsMultiplier;
            hitType = "Leg shot!";
        }

        lastHitLocation = hitType;
        lastMultiplier = multiplier;
        lastDamageDealt = _shootDamage * multiplier;

        // UI feedback
        _feedbackText.text = hitType;
        _damageText.text = "Damage: " + lastDamageDealt;

        return lastDamageDealt;
    }

    private void OnKill() 
    { 
        _killedEnemies++; 
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(_rayOrigin.position, 0.15f);
    }
    */

    #endregion


    #region BVH Logic

    private void ToggleBVHUse(InputAction.CallbackContext context)
    {
        _useBVH = !_useBVH;
    }

    RaycastHit UseBVH(Ray ray, out EnemiesBVH hitEnemie, out bool found)
    {
        float minDst = _rayDistance;
        RaycastHit best = new RaycastHit();
        hitEnemie = null;
        found = false;

        foreach (var e in _allEnemies)
        {
            if (e == null) continue;

            List<Collider> allParts = new List<Collider>();
            allParts.AddRange(e.upperBodyParts);
            allParts.AddRange(e.lowerBodyParts);

            foreach (var part in allParts)
            {
                if (part == null) continue;
                if (part.Raycast(ray, out RaycastHit tempHit, _rayDistance))
                {
                    if (tempHit.distance < minDst)
                    {
                        minDst = tempHit.distance;
                        best = tempHit;
                        hitEnemie = e;
                        found = true;
                    }
                }
            }
        }
        return best;
    }

    RaycastHit DontUseBVH(Ray ray, out EnemiesBVH hitEnemie, out bool found)
    {
        float minDst = _rayDistance;
        RaycastHit best = new RaycastHit();
        hitEnemie = null;
        found = false;

        foreach (var e in _allEnemies)
        {
            if (e == null) continue;

            List<Collider> allParts = new List<Collider>();
            allParts.AddRange(e.upperBodyParts);
            allParts.AddRange(e.lowerBodyParts);

            foreach (var part in allParts)
            {
                if (part == null) continue;
                if (part.Raycast(ray, out RaycastHit tempHit, _rayDistance))
                {
                    if (tempHit.distance < minDst)
                    {
                        minDst = tempHit.distance;
                        best = tempHit;
                        hitEnemie = e;
                        found = true;
                    }
                }
            }
        }
        return best;
    }
    #endregion

    #region Draw Rayscast Gizmos
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Gizmos.color = _useBVH ? Color.green : Color.red;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 endPoint = ray.origin + (ray.direction * _rayDistance);

        Gizmos.DrawLine(ray.origin, endPoint);
        Gizmos.DrawSphere(ray.origin, 0.05f);
        Gizmos.DrawWireSphere(endPoint, 0.2f);

        if (_rayOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_rayOrigin.position, 0.1f);
        }
    }
    #endregion
}