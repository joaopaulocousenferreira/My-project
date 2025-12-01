using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Necessário para TextMeshProUGUI
using UnityEngine.UI; // Necessário para Button

public class ResultsScreenController : MonoBehaviour
{
    [Header("Referências da UI de Resultados")]
    [Tooltip("O objeto de texto que mostrará o XP ganho na fase.")]
    public TextMeshProUGUI xpGainedText; // Variável atualizada para o XP
    
    [Tooltip("A lista/array com os 3 GameObjects das imagens de estrela.")]
    public GameObject[] stars;
    
    [Tooltip("O botão para voltar à tela de seleção de fases.")]
    public Button backButton;

    // O método Start é chamado automaticamente uma vez, assim que a cena é carregada.
    void Start()
    {
        // Conecta a função ao clique do botão "Voltar".
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToLevelSelect);
        }

        // Verificação de segurança para garantir que o GameManager existe.
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager não encontrado! Exibindo dados de exemplo.");
            // Mostra dados de exemplo se o GameManager falhar (útil para testar o visual da cena diretamente).
            DisplayResults(100, 2); // Passa um XP e um número de erros de exemplo.
            return;
        }

        // 1. Pega os resultados do "pacote de dados" que o PhaseController deixou no GameManager.
        int errors = GameManager.instance.lastPhaseErrors;
        int xpGained = GameManager.instance.lastPhaseXpGained;

        // 2. Chama a função para exibir os resultados na tela.
        DisplayResults(xpGained, errors);
    }

    /// <summary>
    /// Atualiza todos os elementos visuais da tela com os dados da fase.
    /// </summary>
    void DisplayResults(int xpGained, int errorCount)
    {
        // Atualiza o texto para mostrar o XP ganho, com um formato amigável.
        if (xpGainedText != null)
        {
            xpGainedText.text = "+ " + xpGained.ToString() + " XP";
        }

        // A lógica para calcular e exibir as estrelas continua a mesma.
        int starsEarned = CalculateStars(errorCount);
        DisplayStars(starsEarned);
    }

    /// <summary>
    /// Calcula o número de estrelas com base na contagem de erros.
    /// </summary>
    private int CalculateStars(int errorCount)
    {
        if (errorCount == 0) return 3;       // Perfeito
        if (errorCount <= 2) return 2; // Bom
        if (errorCount <= 4) return 1; // Razoável
        return 0;                      // Falha
    }

    /// <summary>
    /// Ativa o número correto de imagens de estrela na UI.
    /// </summary>
    private void DisplayStars(int count)
    {
        if (stars == null) return; // Verificação de segurança

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                // Ativa a estrela se seu índice (0, 1 ou 2) for menor que a quantidade ganha.
                stars[i].SetActive(i < count);
            }
        }
    }

    /// <summary>
    /// Carrega a cena de Seleção de Fases. É chamada pelo backButton.
    /// </summary>
    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("SelecaoDeFase");
    }
}