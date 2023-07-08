using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthCharacter : MonoBehaviour
{
    [Header("Для здоровья")]
    [SerializeField] private float m_maxHealthPoint;
    [SerializeField] private Slider m_healthPointBar;
    [SerializeField] private float m_damage;
    private float m_currentHealth = 100f;

    private Animator _animator;

    void Start()
    {

        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        m_healthPointBar.value = m_currentHealth;
        if (m_currentHealth <= 0)
        {
            // Here you will need to write reload the level
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Enemy")
        {
            _animator.SetTrigger("Hurt");
            m_currentHealth -= m_damage;
        }

    }


}
