using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnemiesStateMachine : MonoBehaviour
{
    private EnemiesHealth enemyHealth;
    private Animator animator;
    private Rigidbody rb;


    public Transform target;


    public float detectionRange = 10f;
    public float attackRange = 0.1f;
    public float moveSpeed = 3f;

    public GameObject attackHitbox;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemiesHealth>();
        rb = GetComponent<Rigidbody>();

        target = GameObject.FindGameObjectWithTag("Player").transform;

        enemyHealth.currentState = EnemyState.Idle;
    }

    void Update()
    {
        HandleStateTransitions();
    }

    void FixedUpdate()
    {
        if (enemyHealth.currentState == EnemyState.Running)
        {
            HandleMovement();
        }
    }

    #region State Machine Related Functions
    void HandleStateTransitions()
    {
        if (enemyHealth.currentState == EnemyState.Dying)
        {
            return;
        }

        if (target == null)
        {
            SetState(EnemyState.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(this.transform.position, target.position);
        if (distanceToTarget <= attackRange)
        {
            SetState(EnemyState.Attacking);
        }
        else if (distanceToTarget <= detectionRange)
        {
            SetState(EnemyState.Running);
        }
        else
        {
            SetState(EnemyState.Idle);
        }
    }

    public void SetState(EnemyState newState)
    {
        if (enemyHealth.currentState == newState)
        {
            return;
        }

        ExitState(enemyHealth.currentState);
        enemyHealth.currentState = newState;
        EnterState(enemyHealth.currentState);
    }

    void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                animator.SetBool("Idle", true);
                rb.linearVelocity = Vector3.zero;
                break;
            case EnemyState.Running:
                animator.SetBool("Run", true);
                break;
            case EnemyState.Attacking:
                EnableHitbox();
                animator.SetBool("Attack", true);
                rb.linearVelocity = Vector3.zero;
                break;
            case EnemyState.Dying:
                animator.SetBool("Dying", true);
                break;
        }
    }

    void ExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                animator.SetBool("Idle", false);
                rb.linearVelocity = Vector3.zero;
                break;
            case EnemyState.Running:
                animator.SetBool("Run", false);
                break;
            case EnemyState.Attacking:
                DisableHitbox();
                animator.SetBool("Attack", false);
                break;
            case EnemyState.Dying:
                animator.SetBool("Dying", false);
                break;
        }
    }
    #endregion


    void HandleMovement()
    {
        if (enemyHealth.currentState != EnemyState.Running) return;
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
        rb.linearVelocity = direction * moveSpeed;
        SetState(EnemyState.Running);
    }

    public void EnableHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.GetComponent<BoxCollider>().enabled = false;
        }
    }
}

public enum EnemyState
{
    Idle,
    Running,
    Attacking,
    Dying
}