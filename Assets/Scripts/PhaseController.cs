using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Necessário para interagir com o Input Field

public class PhaseController : MonoBehaviour
{
    [Header("Configuração dos Níveis")]
    [Tooltip("Uma lista/array com os GameObjects que contêm os elementos de cada nível.")]
    public GameObject[] levelObjects;

    [Header("Estado da Fase")]
    public int errorCount = 0;
    public int correctDropCount = 0;
    public int totalItemsToDrop; // Este valor será definido pela lógica de cada nível

    // O método Start é chamado uma vez quando a cena é carregada.
    void Start()
    {
        // Garante que o GameManager existe antes de tentar usá-lo.
        if (GameManager.instance != null)
        {
            // Pergunta ao GameManager qual nível deve ser carregado.
            int levelToLoad = GameManager.instance.currentLevelToLoad;
            LoadLevelData(levelToLoad);
        }
        else
        {
            // Mensagem de segurança caso a cena seja iniciada diretamente no editor.
            Debug.LogError("GameManager não encontrado! Carregando nível 1 como padrão.");
            LoadLevelData(1);
        }
    }

    /// <summary>
    /// Prepara a cena com os objetos e regras do nível correto.
    /// </summary>
    void LoadLevelData(int levelNumber)
    {
        Debug.Log("Montando o nível: " + levelNumber);
        
        // Primeiro, desativa todos os containers de nível para garantir uma cena limpa.
        foreach (var level in levelObjects)
        {
            if(level != null) level.SetActive(false);
        }
        
        // Ativa apenas o container do nível correto.
        // (Nível 1 = índice 0, Nível 2 = índice 1, etc.)
        if(levelNumber > 0 && levelNumber <= levelObjects.Length)
        {
            levelObjects[levelNumber - 1].SetActive(true);
        }

        // Define as regras específicas de cada nível.
        // IMPORTANTE: Adicione as condições para cada nível do seu jogo.
        if (levelNumber == 1) { totalItemsToDrop = 3; }
        else if (levelNumber == 2) { totalItemsToDrop = 4; }
        else { totalItemsToDrop = 3; } // Valor padrão caso o nível não esteja configurado
    }

    /// <summary>
    /// Função para o painel de teste, permite forçar o fim da fase com um número de erros.
    /// </summary>
    public void EndPhaseManually(TMP_InputField errorInputField)
    {
        int.TryParse(errorInputField.text, out int manualErrors);
        this.errorCount = manualErrors;
        EndPhase();
    }

    // --- Funções para a jogabilidade real (arrastar e soltar) ---

    public void OnWrongDrop()
    {
        errorCount++;
    }

    public void OnCorrectDrop()
    {
        correctDropCount++;
        if (correctDropCount >= totalItemsToDrop)
        {
            EndPhase();
        }
    }

    /// <summary>
    /// Finaliza a fase, calcula todos os resultados, atualiza o GameManager e EmblemManager,
    /// salva o jogo e carrega a próxima cena.
    /// </summary>
    private void EndPhase()
    {
        // 1. Pega o índice do nível atual (Nível 1 = índice 0).
        int currentLevelIndex = GameManager.instance.currentLevelToLoad - 1;

        // 2. Calcula os resultados da partida.
        int score = 1000 - (errorCount * 200);
        if (score < 0) score = 0;
        int xpGained = 150 - (errorCount * 25);
        if (xpGained < 0) xpGained = 0;
        int starsEarned = CalculateStars(errorCount);

        // 3. ATUALIZA O EMBLEMA NO EMBLEMManager.
        // Isso deve acontecer ANTES de salvar o jogo.
        if (EmblemManager.instance != null)
        {
            EmblemManager.instance.playerEmblem.AddXP(xpGained);
        }

        // 4. Atualiza os dados de progressão no "diário de bordo" do GameManager.
        // Só salva as estrelas se o jogador conseguiu uma pontuação melhor que a anterior.
        if (starsEarned > GameManager.instance.levels[currentLevelIndex].starsEarned)
        {
            GameManager.instance.levels[currentLevelIndex].starsEarned = starsEarned;
        }

        // Desbloqueia o próximo nível apenas se o jogador conseguiu 2 ou mais estrelas.
        if (starsEarned >= 2)
        {
            int nextLevelIndex = currentLevelIndex + 1;
            // Garante que não tentamos desbloquear um nível que não existe.
            if (nextLevelIndex < GameManager.instance.levels.Length)
            {
                GameManager.instance.levels[nextLevelIndex].isUnlocked = true;
            }
        }

        // 5. Prepara o "pacote de dados" para a tela de conclusão.
        GameManager.instance.lastPhaseScore = score;
        GameManager.instance.lastPhaseErrors = errorCount;
        GameManager.instance.lastPhaseXpGained = xpGained;
        
        // 6. SALVA O PROGRESSO NO DISCO. (Agora com o emblema e níveis atualizados).
        SaveLoadManager.SaveGame();

        // 7. Carrega a cena de conclusão.
        SceneManager.LoadScene("ConclusaoFase");
    }

    /// <summary>
    /// Calcula as estrelas com base nos erros.
    /// </summary>
    private int CalculateStars(int errorCount)
    {
        if (errorCount == 0) return 3;       // Perfeito (0 erros)
        if (errorCount <= 2) return 2; // Bom (1 ou 2 erros)
        if (errorCount <= 4) return 1; // Razoável (3 ou 4 erros)
        return 0;                      // Falha (5 ou mais erros)
    }
}