using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTurnBasedAI : MonoBehaviour
{
    // Variables para el estado del juego
    public int enemyHealth;
    public int playerHealth;
    public int enemyResources;
    public int playerResources;

    // Método para que la IA decida qué hacer en su turno
    public void PerformAIActions()
    {
        if (enemyHealth < 20 && enemyResources >= 10)
        {
            // Regla: Si la salud del enemigo es baja y tiene suficientes recursos, curarse
            Heal();
        }
        else if (playerHealth < 10)
        {
            // Regla: Si la salud del jugador es baja, atacar
            AttackPlayer();
        }
        else if (enemyResources > 20)
        {
            // Regla: Si tiene muchos recursos, construir
            BuildStructure();
        }
        else
        {
            // Regla: Si no se cumplen otras condiciones, recolectar recursos
            CollectResources();
        }
    }

    void Heal()
    {
        // Lógica para curarse
        Debug.Log("El enemigo se cura.");
        enemyHealth += 10;
        enemyResources -= 10;
    }

    void AttackPlayer()
    {
        // Lógica para atacar al jugador
        Debug.Log("El enemigo ataca al jugador.");
        playerHealth -= 5;
    }

    void BuildStructure()
    {
        // Lógica para construir estructuras
        Debug.Log("El enemigo construye una estructura.");
        enemyResources -= 20;
    }

    void CollectResources()
    {
        // Lógica para recolectar recursos
        Debug.Log("El enemigo recolecta recursos.");
        enemyResources += 5;
    }
}
