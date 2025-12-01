using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para gerenciar as cenas do jogo

public class MainMenuController : MonoBehaviour
{
    /// <summary>
    /// Esta função é chamada pelo botão "Iniciar" ou "Continuar".
    /// Ela carrega a cena de Seleção de Fases, mantendo o progresso existente.
    /// </summary>
    public void StartGame()
    {
        // O nome da cena aqui deve ser EXATAMENTE o mesmo nome do arquivo da cena
        // e como ele está nas Build Settings.
        SceneManager.LoadScene("SelecaoDeFase");
    }

    /// <summary>
    /// Esta função é chamada pelo botão "Novo Jogo".
    /// Ela apaga todo o progresso salvo e reinicia a cena inicial.
    /// </summary>
    public void NewGame()
    {
        Debug.Log("Iniciando um Novo Jogo... Todo o progresso salvo foi apagado!");
        
        // Apaga todos os dados salvos no disco (níveis, estrelas, emblema, etc.).
        PlayerPrefs.DeleteAll();

        // Recarrega a cena atual. Isso é importante para que os managers
        // (GameManager, EmblemManager) possam reiniciar com os dados "limpos",
        // em vez de usar os dados que estavam na memória da sessão de jogo anterior.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}