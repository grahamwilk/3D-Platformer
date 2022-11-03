using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public Image deathImage;
    public Image[] healthPoints;
    float health;
    float maxHealth = 10;
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (health > maxHealth) health = maxHealth;
        HealthBarFiller();
        if (health <= 0)
        {
            Die();
        }
    }
    void HealthBarFiller()
    {
        for (int i =0; i < healthPoints.Length; i++)
        {
            healthPoints[i].enabled = !DisplayHealthPoint(health, i);
        }
    }
    bool DisplayHealthPoint(float aHealth, int pointNumber)
    {
        return pointNumber >= aHealth;
    }
    public void Damage(int damagePoints)
    {
        if (health > 0)
        {
            health -= damagePoints;
        }
    }
    public void Heal(int healingPoints)
    {
        if (health < maxHealth)
        {
            health += healingPoints;
        }
    }
    public void Die()
    {
        deathImage.enabled = true;
    }
}
