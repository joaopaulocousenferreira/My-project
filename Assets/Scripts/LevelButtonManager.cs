using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton_Manager : MonoBehaviour
{
    [Tooltip("Nome exato da cena que este botão deve carregar.")]
    public string fase = "FaseDeJogo";
    
    [Tooltip("Número da fase que será enviado ao GameManager.")]
    public int levelNumber = 1;

    public void SelectLevel() 
    { 
        if (GameManager.instance != null) 
        { 
            GameManager.instance.currentLevelToLoad = levelNumber; 
        } 
        
        SceneManager.LoadScene(fase); 
    }
}