using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq; // Necessário para ordenar os sprites

// --- Enums de Configuração ---
public enum UIMode
{
    AnonymousOnly,
    LoginOnly,
    Hybrid
}
public enum BackendMode
{
    LocalPlayerPrefs,
    CloudAPI
}

public class AuthenticationManager : MonoBehaviour
{
    [Header("Configuração Principal do Template")]
    public UIMode uiMode = UIMode.Hybrid;
    public BackendMode backendMode = BackendMode.LocalPlayerPrefs;

    [Header("Configuração da API (Nuvem)")]
    public string apiEndpoint = "https://sua-api.com/api/";
    public string apiKey = "sua-chave-api-secreta";

    [Header("Referências da UI (Deslogado)")]
    [Tooltip("Botão principal para 'Jogar como Convidado'")]
    public Button anonymousStartButton;
    [Tooltip("Botão que ABRE o painel de login. Pode ser o mesmo objeto do 'Show User Info Button'.")]
    public Button showLoginPanelButton;
    [Tooltip("O painel completo que contém os campos de login/registo")]
    public GameObject loginPanel;

    [Header("Referências da UI (Logado)")]
    [Tooltip("Botão que substitui o 'Jogar Anônimo' quando logado")]
    public Button continueButton;
    [Tooltip("Botão que ABRE o painel de info do utilizador. Pode ser o mesmo objeto do 'Show Login Panel Button'.")]
    public Button showUserInfoButton;
    [Tooltip("O painel que mostra as informações do utilizador já logado")]
    public GameObject loggedInInfoPanel;
    
    // --- UI DO RANKING ---
    [Header("UI de Ranking (Só aparece Logado)")]
    [Tooltip("O botão (Troféu) que abre o ranking.")]
    public Button showRankingButton; 
    [Tooltip("O painel que contém a lista de classificação.")]
    public GameObject rankingPanel;
    [Tooltip("O botão 'X' dentro do painel de ranking.")]
    public Button closeRankingButton;
    // --------------------

    [Header("Referências do Painel de Login")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public Button loginRegisterButton;
    public Button closeLoginPanelButton;

    [Header("Referências do Painel de Info (Logado)")]
    public TextMeshProUGUI welcomeText;
    public Image loggedInUserEmblemIcon; // Ícone do emblema no painel
    public TextMeshProUGUI loggedInUserLevelText; // Texto do nível
    public Slider loggedInUserXpBar;
    public TextMeshProUGUI loggedInUserXpText;
    public Button logoutButton;
    public Button closeInfoPanelButton;

    private const string ANONYMOUS_USER_ID = "local_anonymous_user";
    
    // Cache dos sprites dos emblemas
    private Sprite[] emblemSprites;

    void Awake()
    {
        LoadEmblemAssets();
    }

    void Start()
    {
        // --- Conexões Estáticas ---
        if (anonymousStartButton != null) anonymousStartButton.onClick.AddListener(StartAnonymousGame);
        if (loginRegisterButton != null) loginRegisterButton.onClick.AddListener(OnLoginRegisterClick);
        if (closeLoginPanelButton != null) closeLoginPanelButton.onClick.AddListener(ToggleLoginPanel);
        
        if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
        if (logoutButton != null) logoutButton.onClick.AddListener(Logout);
        if (closeInfoPanelButton != null) closeInfoPanelButton.onClick.AddListener(ToggleLoggedInInfoPanel);

        // Conexão do Ranking
        if (showRankingButton != null) showRankingButton.onClick.AddListener(ToggleRankingPanel);
        if (closeRankingButton != null) closeRankingButton.onClick.AddListener(ToggleRankingPanel);

        // --- Lógica de Decisão de Estado ---
        // Verifica se existe utilizador E se ele NÃO é o anônimo.
        if (GameManager.instance != null && 
            !string.IsNullOrEmpty(GameManager.instance.currentUserID) && 
            GameManager.instance.currentUserID != ANONYMOUS_USER_ID)
        {
            SetupLoggedInUI(GameManager.instance.currentUserID);
        }
        else
        {
            SetupLoggedOutUI();
        }
    }

    void LoadEmblemAssets()
    {
        emblemSprites = Resources.LoadAll<Sprite>("Game/Emblemas/emblemas_spritesheet");
        if (emblemSprites != null && emblemSprites.Length > 0)
        {
            emblemSprites = emblemSprites.OrderBy(s => s.name).ToArray();
        }
        else
        {
            Debug.LogError("AuthenticationManager: Sprites de emblema não encontrados em Resources.");
        }
    }

    // --- CONFIGURAÇÃO DE ESTADO (VISIBILIDADE) ---

    private void SetupLoggedOutUI()
    {
        // Esconde elementos de "Logado"
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (loggedInInfoPanel != null) loggedInInfoPanel.SetActive(false);
        
        // Esconde Ranking se deslogado
        if (showRankingButton != null) showRankingButton.gameObject.SetActive(false);
        if (rankingPanel != null) rankingPanel.SetActive(false);
        
        // Lógica do botão duplo (Info/Login)
        if (showUserInfoButton != null && showUserInfoButton != showLoginPanelButton) 
            showUserInfoButton.gameObject.SetActive(false);

        switch (uiMode)
        {
            case UIMode.AnonymousOnly:
                if (anonymousStartButton != null) anonymousStartButton.gameObject.SetActive(true);
                if (showLoginPanelButton != null) showLoginPanelButton.gameObject.SetActive(false);
                if (loginPanel != null) loginPanel.SetActive(false);
                break;

            case UIMode.LoginOnly:
                if (anonymousStartButton != null) anonymousStartButton.gameObject.SetActive(false);
                if (showLoginPanelButton != null) showLoginPanelButton.gameObject.SetActive(false);
                if (loginPanel != null) loginPanel.SetActive(true);
                break;

            case UIMode.Hybrid:
                if (anonymousStartButton != null) anonymousStartButton.gameObject.SetActive(true);
                if (loginPanel != null) loginPanel.SetActive(false);
                
                if (showLoginPanelButton != null) 
                {
                    showLoginPanelButton.gameObject.SetActive(true);
                    // Remove conexões antigas e adiciona a de abrir login
                    showLoginPanelButton.onClick.RemoveAllListeners();
                    showLoginPanelButton.onClick.AddListener(ToggleLoginPanel);
                }
                break;
        }
    }

    private void SetupLoggedInUI(string username)
    {
        // Esconde elementos de "Deslogado"
        if (anonymousStartButton != null) anonymousStartButton.gameObject.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);

        if (showLoginPanelButton != null && showLoginPanelButton != showUserInfoButton) 
            showLoginPanelButton.gameObject.SetActive(false);

        // Mostra elementos de "Logado"
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        if (loggedInInfoPanel != null) loggedInInfoPanel.SetActive(false); // Começa fechado

        // Mostra botão de Ranking
        if (showRankingButton != null) showRankingButton.gameObject.SetActive(true);
        if (rankingPanel != null) rankingPanel.SetActive(false);

        if (showUserInfoButton != null) 
        {
            showUserInfoButton.gameObject.SetActive(true);
            // Remove conexões antigas e adiciona a de abrir info do usuário
            showUserInfoButton.onClick.RemoveAllListeners();
            showUserInfoButton.onClick.AddListener(ToggleLoggedInInfoPanel);
        }
    }
    
    // --- FUNÇÕES DE ABRIR/FECHAR PAINÉIS ---

    public void StartAnonymousGame()
    {
        if (anonymousStartButton != null) anonymousStartButton.interactable = false;
        if (showLoginPanelButton != null) showLoginPanelButton.interactable = false;
        LoginSuccess(ANONYMOUS_USER_ID);
    }

    public void ToggleLoginPanel()
    {
        if (loginPanel != null) 
        {
            loginPanel.SetActive(!loginPanel.activeSelf);
            // Fecha outros painéis se estiverem abertos para evitar sobreposição
            if (rankingPanel != null) rankingPanel.SetActive(false);
        }
    }

    public void ToggleLoggedInInfoPanel()
    {
        if (loggedInInfoPanel != null)
        {
            bool isActive = !loggedInInfoPanel.activeSelf;
            
            // Fecha outros painéis
            if (rankingPanel != null) rankingPanel.SetActive(false);

            loggedInInfoPanel.SetActive(isActive);
            if (isActive) { UpdateLoggedInInfoPanel(); }
        }
    }

    public void ToggleRankingPanel()
    {
        if (rankingPanel != null)
        {
            bool isActive = !rankingPanel.activeSelf;

            // Fecha outros painéis
            if (loggedInInfoPanel != null) loggedInInfoPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(false);

            rankingPanel.SetActive(isActive);
        }
    }

    // --- FUNÇÕES DE ATUALIZAÇÃO DE DADOS ---

    private void UpdateLoggedInInfoPanel()
    {
        if (EmblemManager.instance == null) return;
        
        string username = GameManager.instance.currentUserID;
        Emblem emblemData = EmblemManager.instance.playerEmblem;

        if (welcomeText != null) welcomeText.text = "Utilizador: " + username;
        if (loggedInUserLevelText != null) loggedInUserLevelText.text = "Nível: " + emblemData.currentLevel;

        if (loggedInUserXpBar != null)
        {
            loggedInUserXpBar.maxValue = emblemData.xpToNextLevel;
            loggedInUserXpBar.value = emblemData.currentXP;
        }
        if (loggedInUserXpText != null)
        {
            loggedInUserXpText.text = "XP: " + emblemData.currentXP + " / " + emblemData.xpToNextLevel;
        }

        // Atualiza o ícone com base no nível
        if (loggedInUserEmblemIcon != null && emblemSprites != null && emblemSprites.Length > 0)
        {
            int spriteIndex = emblemData.currentLevel - 1;
            spriteIndex = Mathf.Min(spriteIndex, emblemSprites.Length - 1);
            spriteIndex = Mathf.Max(spriteIndex, 0);
            loggedInUserEmblemIcon.sprite = emblemSprites[spriteIndex];
        }
    }

    // --- LÓGICA DE LOGIN ---

    public void OnLoginRegisterClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) { errorText.text = "Preencha todos os campos."; return; }
        loginRegisterButton.interactable = false;
        errorText.text = "A processar...";
        if (backendMode == BackendMode.LocalPlayerPrefs) { LoginRegisterLocal(username, password); }
        else if (backendMode == BackendMode.CloudAPI) { LoginRegisterCloud(username, password); }
    }
    
    private void LoginRegisterLocal(string username, string password)
    {
        string userKey = "user_" + username;
        if (PlayerPrefs.HasKey(userKey)) {
            if (password == PlayerPrefs.GetString(userKey)) LoginSuccess(username);
            else LoginFail("Senha incorreta.");
        } else {
            PlayerPrefs.SetString(userKey, password);
            
            // Regista na lista mestra para o Ranking
            string registry = PlayerPrefs.GetString("RegisteredUsersRegistry", "");
            if (string.IsNullOrEmpty(registry)) registry = username;
            else if(!registry.Contains(username)) registry += "," + username;
            PlayerPrefs.SetString("RegisteredUsersRegistry", registry);

            PlayerPrefs.Save();
            LoginSuccess(username);
        }
    }

    private void LoginRegisterCloud(string username, string password)
    {
        errorText.text = "Função de nuvem ainda não implementada.";
        loginRegisterButton.interactable = true;
    }

    private void LoginSuccess(string username)
    {
        if(errorText != null) errorText.text = "Bem-vindo, " + username + "!";
        
        GameManager.instance.currentUserID = username;
        EmblemManager.instance.currentUserID = username;
        SaveLoadManager.LoadGame(username);
        SceneManager.LoadScene("SelecaoDeFase");
    }

    private void LoginFail(string message)
    {
        errorText.text = message;
        loginRegisterButton.interactable = true;
    }

    private void Logout()
    {
        GameManager.instance.currentUserID = null;
        EmblemManager.instance.currentUserID = null;
        EmblemManager.instance.playerEmblem = new Emblem(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ContinueGame()
    {
        SceneManager.LoadScene("SelecaoDeFase");
    }
}