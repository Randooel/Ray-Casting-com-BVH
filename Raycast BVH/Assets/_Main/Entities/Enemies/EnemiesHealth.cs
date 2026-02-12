using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnemiesHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    public float currentHealth;

    private Animator animator;

    public EnemyState currentState;
    private EnemiesStateMachine EnemiesStateMachine;

    public GameManager gameManager;

    public int points = 10;

    void Start()
    {
        currentHealth = maxHealth;
        EnemiesStateMachine = GetComponent<EnemiesStateMachine>();

        animator = GetComponent<Animator>();
        gameManager = FindAnyObjectByType<GameManager>();
    }


    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EnemiesStateMachine.SetState(EnemyState.Dying);
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;

        gameManager.Score += points;

        Destroy(gameObject, 3f);
    }
}

