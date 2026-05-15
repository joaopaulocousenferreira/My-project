using UnityEngine;

[System.Serializable]
public class LevelData
{
    public bool isUnlocked;
    public int starsEarned;
}

public class GameManager : MonoBehaviour
{
    // Padrão Singleton para garantir acesso global e único.
    public static GameManager instance;

    [Header("Regras do Jogo (Configuráveis no Inspector)")]
    [Tooltip("Se marcado, o jogo exigirá o login. Se desmarcado, mostrará um botão 'Iniciar' direto para o modo anónimo.")]
    public bool requiresLogin = false; // Por padrão, o login é opcional (desligado).

    [Tooltip("Se marcado, os níveis precisam ser desbloqueados em sequência (apenas para novos jogos).")]
    public bool useLevelLocking = true;
    
    [Header("Estado Atual do Jogo")]
    [Tooltip("Identifica o jogador atual. É definido pelo AuthenticationManager.")]
    public string currentUserID;

    // Guarda o número do nível que o jogador selecionou na tela de seleção.
    public int currentLevelToLoad;

    // Armazena os resultados da última fase concluída para a tela de conclusão.
    [Header("Resultados da Última Fase")]
    public int lastPhaseScore;
    public int lastPhaseErrors;
    public int lastPhaseXpGained;

    // "Diário de bordo" com os dados de progressão de todos os níveis.
    [Header("Dados de Progressão dos Níveis")]
    public LevelData[] levels;

    private bool initialized = false;

    // O método Awake é chamado antes de qualquer método Start.
    void Awake()
    {
        // 1. Lógica do Singleton Anti-Clonagem
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        // 2. A DECAPITAÇÃO DO PREFAB
        // Liberta o GameManager do Canvas/Pasta pai e o joga na raiz da cena
        transform.SetParent(null);

        // 3. Imortalidade Absoluta
        DontDestroyOnLoad(this.gameObject);

        // --- LÓGICA DE DECISÃO INTELIGENTE ---
        // Verifica se o jogador já jogou antes (procurando a "marca" deixada pelo SaveLoadManager).
        if (PlayerPrefs.HasKey("HasPlayedBefore"))
        {
            // Se já jogou, NÃO inicializamos. Apenas garantimos que o array 'levels'
            // tenha o tamanho correto para o LoadGame() que será chamado após o login.
            Debug.Log("GameManager: Progresso existente detetado. À espera de login para carregar.");
            if (levels == null || levels.Length == 0)
            {
                int totalLevels = 6; // Deve corresponder ao número total de níveis
                levels = new LevelData[totalLevels];
                for (int i = 0; i < levels.Length; i++) { levels[i] = new LevelData(); }
            }
        }
        else
        {
            // Se for a primeira vez, INICIALIZAMOS o jogo com as regras do Inspector.
            Debug.Log("GameManager: Nenhum progresso salvo. A inicializar novo jogo...");
            InitializeLevelData();
        }
        // ------------------------------------
    }

    /// <summary>
    /// Configura os dados padrão para todos os níveis na primeira vez que o jogo é executado.
    /// Respeita a regra 'useLevelLocking' definida no Inspector.
    /// </summary>
    void InitializeLevelData()
    {
        if (initialized) return;

        if (levels == null || levels.Length == 0) 
        { 
            int totalLevels = 6;
            levels = new LevelData[totalLevels]; 
        }

        for (int i = 0; i < levels.Length; i++)
        {
            if(levels[i] == null) levels[i] = new LevelData();

            if (useLevelLocking)
            {
                // Modo com bloqueio: só o nível 1 começa desbloqueado.
                levels[i].isUnlocked = (i == 0);
            }
            else
            {
                // Modo sem bloqueio: todos os níveis começam desbloqueados.
                levels[i].isUnlocked = true;
            }

            levels[i].starsEarned = 0;
        }
        
        initialized = true; 
    }
}