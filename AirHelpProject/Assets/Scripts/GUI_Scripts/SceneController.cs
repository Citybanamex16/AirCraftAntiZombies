using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Carga la escena de juego (GameLevel)
    public void PlayGame()
    {
        // Restaurar la velocidad del juego si venimos de estar pausados
        Time.timeScale = 1f; 
        SceneManager.LoadScene(1); // O por índice: SceneManager.LoadScene(1);
    }
}
