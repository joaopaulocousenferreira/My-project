using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

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
    [Header("Configuração Principal")]
    public UIMode uiMode = UIMode.Hybrid;
    public BackendMode backendMode = BackendMode.LocalPlayerPrefs;

    [Header("Botão de Ação Híbrido (Exigência do Professor)")]
    [Tooltip("Este único botão executará o 'Jogar Anônimo' se deslogado, ou 'Continuar' se logado.")]
    public Button actionPlayButton;

    // --- VARIÁVEIS PRIVADAS (Ocultas do Inspector, mapeadas automaticamente) ---
    private Button showLoginPanelButton;
    private GameObject loginPanel;
    private Button showUserInfoButton;
    private GameObject loggedInInfoPanel;
    private Button showRankingButton; 
    private GameObject rankingPanel;
    private Button closeRankingButton;
    
    private TMP_InputField usernameInput;
    private TMP_InputField passwordInput;
    private TextMeshProUGUI errorText;
    private Button loginRegisterButton;
    private Button closeLoginPanelButton;

    private TextMeshProUGUI welcomeText;
    private Image loggedInUserEmblemIcon; 
    private TextMeshProUGUI loggedInUserLevelText; 
    private Slider loggedInUserXpBar;
    private TextMeshProUGUI loggedInUserXpText;
    private Button logoutButton;
    private Button closeInfoPanelButton;

    private const string ANONYMOUS_USER_ID = "local_anonymous_user";
    private Sprite[] emblemSprites;

    void Awake()
    {
        LoadEmblemAssets();
    }

    void Start()
    {
        // 1. Mapeamento Automático Profundo
        CarregarComponentes();

        // 2. Conexões Estáticas Básicas
        if (loginRegisterButton != null) loginRegisterButton.onClick.AddListener(OnLoginRegisterClick);
        if (closeLoginPanelButton != null) closeLoginPanelButton.onClick.AddListener(ToggleLoginPanel);
        if (logoutButton != null) logoutButton.onClick.AddListener(Logout);
        if (closeInfoPanelButton != null) closeInfoPanelButton.onClick.AddListener(ToggleLoggedInInfoPanel);
        if (showRankingButton != null) showRankingButton.onClick.AddListener(ToggleRankingPanel);
        if (closeRankingButton != null) closeRankingButton.onClick.AddListener(ToggleRankingPanel);

        // 3. Avaliação de Estado Absoluto
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

    // --- O NÚCLEO DE MAPEAMENTO (RESOLUÇÃO DO PROBLEMA) ---
    private void CarregarComponentes()
    {
        // Painéis e Botões Principais
        showLoginPanelButton = EncontrarUI<Button>("ShowLoginPanelButton");
        showUserInfoButton   = EncontrarUI<Button>("ShowLoginPanelButton"); 
        loginPanel           = EncontrarUI<GameObject>("LoginPanel", isGameObject: true) as GameObject;
        loggedInInfoPanel    = EncontrarUI<GameObject>("LoggedInInfoPanel", isGameObject: true) as GameObject;
        rankingPanel         = EncontrarUI<GameObject>("RankingPainel", isGameObject: true) as GameObject;
        showRankingButton    = EncontrarUI<Button>("ShowRankingButton");
        closeRankingButton   = EncontrarUI<Button>("CloseRankingButton");

        // Painel de Login
        usernameInput         = EncontrarUI<TMP_InputField>("UsernameInput");
        passwordInput         = EncontrarUI<TMP_InputField>("PasswordInput");
        errorText             = EncontrarUI<TextMeshProUGUI>("ErrorText");
        loginRegisterButton   = EncontrarUI<Button>("LoginRegisterButton");
        closeLoginPanelButton = EncontrarUI<Button>("exit");

        // Painel Info
        welcomeText            = EncontrarUI<TextMeshProUGUI>("WelcomeText");
        loggedInUserEmblemIcon = EncontrarUI<Image>("EmblemImage");
        loggedInUserLevelText  = EncontrarUI<TextMeshProUGUI>("LevelText");
        loggedInUserXpBar      = EncontrarUI<Slider>("XpSlider");
        loggedInUserXpText     = EncontrarUI<TextMeshProUGUI>("XpText");
        logoutButton           = EncontrarUI<Button>("LogoutButton");
        closeInfoPanelButton   = EncontrarUI<Button>("CloseInfoButton");
    }

    private T EncontrarUI<T>(string objName, bool isGameObject = false) where T : class
    {
        Transform[] todosObjetos = Resources.FindObjectsOfTypeAll<Transform>();
        
        foreach (Transform obj in todosObjetos)
        {
            if (obj.gameObject.scene.isLoaded && obj.name == objName)
            {
                if (isGameObject) return obj.gameObject as T;
                
                T componente = obj.GetComponent<T>();
                if (componente != null) return componente;
            }
        }
        
        Debug.LogError($"[AuthManager] ATENÇÃO: Objeto '{objName}' não encontrado na cena. O nome está idêntico?");
        return null;
    }

    // --- CONFIGURAÇÃO DE ESTADO ---
    private void SetupLoggedOutUI()
    {
        if (loggedInInfoPanel != null) loggedInInfoPanel.SetActive(false);
        if (showRankingButton != null) showRankingButton.gameObject.SetActive(false);
        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (showUserInfoButton != null) showUserInfoButton.gameObject.SetActive(false);

        // Configura o botão único para ser o "Jogar Anônimo"
        if (actionPlayButton != null)
        {
            actionPlayButton.gameObject.SetActive(uiMode != UIMode.LoginOnly);
            actionPlayButton.onClick.RemoveAllListeners();
            actionPlayButton.onClick.AddListener(StartAnonymousGame);
            
            // TextMeshProUGUI removido para dar autonomia ao Editor
        }

        switch (uiMode)
        {
            case UIMode.AnonymousOnly:
                if (showLoginPanelButton != null) showLoginPanelButton.gameObject.SetActive(false);
                if (loginPanel != null) loginPanel.SetActive(false);
                break;

            case UIMode.LoginOnly:
                if (showLoginPanelButton != null) showLoginPanelButton.gameObject.SetActive(false);
                if (loginPanel != null) loginPanel.SetActive(true);
                break;

            case UIMode.Hybrid:
                if (loginPanel != null) loginPanel.SetActive(false);
                if (showLoginPanelButton != null) 
                {
                    showLoginPanelButton.gameObject.SetActive(true);
                    showLoginPanelButton.onClick.RemoveAllListeners();
                    showLoginPanelButton.onClick.AddListener(ToggleLoginPanel);
                }
                break;
        }
    }

    private void SetupLoggedInUI(string username)
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (showLoginPanelButton != null) showLoginPanelButton.gameObject.SetActive(false);
        if (loggedInInfoPanel != null) loggedInInfoPanel.SetActive(false);
        
        if (showRankingButton != null) showRankingButton.gameObject.SetActive(true);
        if (rankingPanel != null) rankingPanel.SetActive(false);

        if (showUserInfoButton != null) 
        {
            showUserInfoButton.gameObject.SetActive(true);
            showUserInfoButton.onClick.RemoveAllListeners();
            showUserInfoButton.onClick.AddListener(ToggleLoggedInInfoPanel);
        }

        // Configura o botão único para ser o "Continuar"
        if (actionPlayButton != null)
        {
            actionPlayButton.gameObject.SetActive(true);
            actionPlayButton.onClick.RemoveAllListeners();
            actionPlayButton.onClick.AddListener(ContinueGame);
            
            // TextMeshProUGUI removido para dar autonomia ao Editor
        }
    }

    // --- FUNÇÕES DE ABRIR/FECHAR PAINÉIS ---
    public void ToggleLoginPanel()
    {
        if (loginPanel != null) 
        {
            loginPanel.SetActive(!loginPanel.activeSelf);
            if (rankingPanel != null) rankingPanel.SetActive(false);
        }
    }

    public void ToggleLoggedInInfoPanel()
    {
        if (loggedInInfoPanel != null)
        {
            bool isActive = !loggedInInfoPanel.activeSelf;
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
            if (loggedInInfoPanel != null) loggedInInfoPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(false);
            rankingPanel.SetActive(isActive);
        }
    }

    // --- LÓGICA DE DADOS ORIGINAIS ---
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

        if (loggedInUserEmblemIcon != null && emblemSprites != null && emblemSprites.Length > 0)
        {
            int spriteIndex = emblemData.currentLevel - 1;
            spriteIndex = Mathf.Min(spriteIndex, emblemSprites.Length - 1);
            spriteIndex = Mathf.Max(spriteIndex, 0);
            loggedInUserEmblemIcon.sprite = emblemSprites[spriteIndex];
        }
    }

    // --- LÓGICA DE LOGIN ---
    public void StartAnonymousGame()
    {
        if (actionPlayButton != null) actionPlayButton.interactable = false;
        if (showLoginPanelButton != null) showLoginPanelButton.interactable = false;
        LoginSuccess(ANONYMOUS_USER_ID);
    }

    public void OnLoginRegisterClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) 
        { 
            errorText.text = "Preencha todos os campos."; 
            return; 
        }
        
        loginRegisterButton.interactable = false;
        errorText.text = "A processar...";
        
        if (backendMode == BackendMode.LocalPlayerPrefs) { LoginRegisterLocal(username, password); }
        else if (backendMode == BackendMode.CloudAPI) { LoginRegisterCloud(username, password); }
    }
    
    private void LoginRegisterLocal(string username, string password)
    {
        string userKey = "user_" + username;
        if (PlayerPrefs.HasKey(userKey)) 
        {
            if (password == PlayerPrefs.GetString(userKey)) LoginSuccess(username);
            else LoginFail("Senha incorreta.");
        } 
        else 
        {
            PlayerPrefs.SetString(userKey, password);
            
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