using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.UI; 
using System.Linq; 

//------------------------------------------------------------------//
// Classe Auxiliar para organizar as referências visuais dos botões //
//------------------------------------------------------------------//
[System.Serializable]
public class LevelButtonUI
{
    [Tooltip("O componente Button principal.")]
    public Button buttonComponent;
    [Tooltip("O GameObject que agrupa o número e as estrelas.")]
    public GameObject unlockedContent;
    [Tooltip("O GameObject da imagem do cadeado.")]
    public GameObject lockIcon;
    [Tooltip("A referência direta ao container das estrelas.")]
    public Transform starsContainer;
}

//------------------------------------------------------------------//
// Classe Principal                                                 //
//------------------------------------------------------------------//
public class LevelSelectController : MonoBehaviour
{
    // --- 1. Display Integrado (Canto da Tela) ---
    [Header("UI do Emblema (Display Integrado)")]
    public Image displayEmblemIcon;
    public Slider displayXpBar;
    public TextMeshProUGUI displayXpText;

    // --- 2. Painel de Informações (Pop-up 1) ---
    [Header("Painel de Informações (Pop-up 1)")]
    public GameObject emblemInfoPanel;
    public Image infoPanelEmblemIcon;
    public TextMeshProUGUI infoPanelEmblemNameText;
    public TextMeshProUGUI infoPanelEmblemDescriptionText;
    public Slider infoPanelXpBar;
    public TextMeshProUGUI infoPanelXpText;

    // --- 3. Painel de Grade (Pop-up 2) ---
    [Header("Painel Emblemas Desbloqueados (Pop-up 2)")]
    public GameObject unlockedEmblemsPanel;
    public Transform emblemGridContainer; 
    public GameObject emblemDisplayPrefab; 

    // --- 4. Configurações de Nível e Estrelas ---
    [Header("Botões e Assets de Nível")]
    public LevelButtonUI[] levelButtons;
    
    [Header("Sprites das Estrelas (Manual)")]
    [Tooltip("Arraste aqui o sprite da estrela DOURADA.")]
    public Sprite starGoldSprite; 
    [Tooltip("Arraste aqui o sprite da estrela CINZA.")]
    public Sprite starGreySprite; 
    
    // --- 5. Dados Carregados Dinamicamente (Privados) ---
    private string[] emblemNames;             
    private string[] emblemLevelDescriptions; 
    private Sprite[] unlockedEmblemSprites;   
    private Sprite[] lockedEmblemSprites;     

    // ------------------------------------------------------------------

    void Awake() 
    { 
        LoadGameAssets(); 
    }

    void Start()
    {
        // Garante que os painéis pop-up comecem fechados
        if (emblemInfoPanel != null) emblemInfoPanel.SetActive(false);
        if (unlockedEmblemsPanel != null) unlockedEmblemsPanel.SetActive(false);
        
        // Atualiza toda a interface
        UpdateLevelButtons();
        UpdateEmblemDisplay();
    }

    /// <summary>
    /// Carrega Sprites e Textos da pasta Resources.
    /// </summary>
    void LoadGameAssets()
    {
        // A. Carregar Sprites Desbloqueados
        unlockedEmblemSprites = Resources.LoadAll<Sprite>("Game/Emblemas/emblemas_spritesheet");
        if (unlockedEmblemSprites != null && unlockedEmblemSprites.Length > 0)
            unlockedEmblemSprites = unlockedEmblemSprites.OrderBy(s => s.name).ToArray();
        else 
            Debug.LogError("Sprite Sheet de emblemas DESBLOQUEADOS não encontrado.");

        // B. Carregar Sprites Bloqueados
        // (Ajuste o caminho se você usou arquivos individuais ou outro nome de pasta)
        lockedEmblemSprites = Resources.LoadAll<Sprite>("Game/Emblemas/Locked/emblemas_locked_spritesheet"); 
        if (lockedEmblemSprites != null && lockedEmblemSprites.Length > 0)
            lockedEmblemSprites = lockedEmblemSprites.OrderBy(s => s.name).ToArray();
        else 
            Debug.LogError("Sprite Sheet de emblemas BLOQUEADOS não encontrado.");

        // C. Carregar Nomes (.txt)
        TextAsset namesFile = Resources.Load<TextAsset>("Game/Text/emblem_names");
        if (namesFile != null)
        {
            emblemNames = namesFile.text.Split(';');
        }
        else
        {
            emblemNames = new string[] { "Nível 1", "Nível 2", "Nível 3", "Nível 4", "Nível 5", "Nível 6" };
        }

        // D. Carregar Descrições (.txt)
        TextAsset descFile = Resources.Load<TextAsset>("Game/Text/emblem_descriptions");
        if (descFile != null)
        {
            emblemLevelDescriptions = descFile.text.Split(';');
        }
        else
        {
            emblemLevelDescriptions = new string[] { "Continue jogando!" };
        }
    }

    /// <summary>
    /// Atualiza o estado visual dos botões de nível (Bloqueio + Estrelas).
    /// </summary>
    void UpdateLevelButtons() 
    {
        if (GameManager.instance == null) return;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonUI currentButtonUI = levelButtons[i];
            LevelData levelData = GameManager.instance.levels[i];

            if (levelData.isUnlocked)
            {
                // --- NÍVEL DESBLOQUEADO ---
                currentButtonUI.buttonComponent.interactable = true;
                currentButtonUI.unlockedContent.SetActive(true);
                currentButtonUI.lockIcon.SetActive(false);

                // --- AQUI ESTÁ A LÓGICA DAS ESTRELAS ---
                // Verifica se o container e os sprites existem
                if (currentButtonUI.starsContainer != null && starGoldSprite != null && starGreySprite != null)
                {
                    for (int j = 0; j < currentButtonUI.starsContainer.childCount; j++)
                    {
                        // Pega o componente de imagem da estrela (filho j)
                        Image starImage = currentButtonUI.starsContainer.GetChild(j).GetComponent<Image>();
                        
                        // Se o índice da estrela for menor que o total ganho, pinta de ouro. Senão, cinza.
                        if (j < levelData.starsEarned)
                        {
                            starImage.sprite = starGoldSprite;
                        }
                        else
                        {
                            starImage.sprite = starGreySprite;
                        }
                    }
                }
            }
            else
            {
                // --- NÍVEL BLOQUEADO ---
                currentButtonUI.buttonComponent.interactable = false;
                currentButtonUI.unlockedContent.SetActive(false);
                currentButtonUI.lockIcon.SetActive(true);
            }
        }
     }
    
    // --- Funções de Display e Painéis ---

    void UpdateEmblemDisplay() 
    {
        if (EmblemManager.instance == null) { return; }
        Emblem emblemData = EmblemManager.instance.playerEmblem;

        if(displayXpBar != null)
        {
            displayXpBar.maxValue = emblemData.xpToNextLevel;
            displayXpBar.value = emblemData.currentXP;
        }
        if(displayXpText != null) displayXpText.text = emblemData.currentXP + " / " + emblemData.xpToNextLevel + " XP";
        
        UpdateEmblemIcon(displayEmblemIcon, emblemData);
     }

    void UpdateEmblemInfoPanelContent() 
    {
        if (EmblemManager.instance == null) { return; }
        Emblem emblemData = EmblemManager.instance.playerEmblem;

        // 1. Nome
        if(infoPanelEmblemNameText != null && emblemNames != null && emblemNames.Length > 0)
        {
            int nameIndex = Mathf.Clamp(emblemData.currentLevel - 1, 0, emblemNames.Length - 1);
            infoPanelEmblemNameText.text = emblemNames[nameIndex];
        }

        // 2. XP
        if(infoPanelXpBar != null)
        {
            infoPanelXpBar.maxValue = emblemData.xpToNextLevel;
            infoPanelXpBar.value = emblemData.currentXP;
        }
        if(infoPanelXpText != null) infoPanelXpText.text = emblemData.currentXP + " / " + emblemData.xpToNextLevel + " XP";
        
        // 3. Ícone
        UpdateEmblemIcon(infoPanelEmblemIcon, emblemData);

        // 4. Descrição
        if (infoPanelEmblemDescriptionText != null && emblemLevelDescriptions != null && emblemLevelDescriptions.Length > 0)
        {
            int descriptionIndex = Mathf.Clamp(emblemData.currentLevel - 1, 0, emblemLevelDescriptions.Length - 1);
            infoPanelEmblemDescriptionText.text = emblemLevelDescriptions[descriptionIndex];
        }
        else if (infoPanelEmblemDescriptionText != null)
        {
             infoPanelEmblemDescriptionText.text = "Continue jogando para evoluir!";
        }
     }

    void UpdateEmblemIcon(Image iconImage, Emblem emblemData)
    {
         if (iconImage != null && unlockedEmblemSprites != null && unlockedEmblemSprites.Length > 0)
        {
            int spriteIndex = emblemData.currentLevel - 1;
            spriteIndex = Mathf.Min(spriteIndex, unlockedEmblemSprites.Length - 1);
            spriteIndex = Mathf.Max(spriteIndex, 0);
            iconImage.sprite = unlockedEmblemSprites[spriteIndex];
        }
    }

    // --- Controle de Abertura/Fechamento ---

    public void ToggleEmblemInfoPanel()
    {
        if (emblemInfoPanel != null)
        {
            bool wasActive = emblemInfoPanel.activeSelf;
            if (unlockedEmblemsPanel != null) unlockedEmblemsPanel.SetActive(false); 
            emblemInfoPanel.SetActive(!wasActive);
            if (emblemInfoPanel.activeSelf) UpdateEmblemInfoPanelContent();
        }
    }

    public void ShowUnlockedEmblemsPanel()
    {
        if (emblemInfoPanel != null) emblemInfoPanel.SetActive(false);
        if (unlockedEmblemsPanel != null)
        {
            unlockedEmblemsPanel.SetActive(true);
            PopulateEmblemGrid();
        }
    }

    public void HideUnlockedEmblemsPanel()
    {
        if (unlockedEmblemsPanel != null) unlockedEmblemsPanel.SetActive(false);
        if (emblemInfoPanel != null)
        {
             emblemInfoPanel.SetActive(true);
             UpdateEmblemInfoPanelContent();
        }
    }

    void PopulateEmblemGrid()
    {
        if (EmblemManager.instance == null || emblemGridContainer == null || emblemDisplayPrefab == null || unlockedEmblemSprites == null || lockedEmblemSprites == null)
        {
             return;
        }

        int playerLevel = EmblemManager.instance.playerEmblem.currentLevel;

        foreach (Transform child in emblemGridContainer) { Destroy(child.gameObject); }

        int numberOfItems = Mathf.Min(emblemNames.Length, unlockedEmblemSprites.Length, lockedEmblemSprites.Length);

        for (int i = 0; i < numberOfItems; i++)
        {
            GameObject itemGO = Instantiate(emblemDisplayPrefab, emblemGridContainer);
            Image icon = itemGO.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI nameText = itemGO.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            
            int currentItemLevel = i + 1;

            if (playerLevel >= currentItemLevel)
            {
                icon.sprite = unlockedEmblemSprites[i];
                nameText.text = emblemNames[i]; 
            }
            else
            {
                icon.sprite = lockedEmblemSprites[i];
                nameText.text = emblemNames[i]; 
            }
        }
    }

    // --- Navegação ---
    public void SelectLevel(int levelNumber) 
    { 
        if (GameManager.instance != null) { GameManager.instance.currentLevelToLoad = levelNumber; } 
        SceneManager.LoadScene("FaseDeJogo"); 
    }
    public void GoToMainMenu() 
    { 
        SceneManager.LoadScene("TelaInicial"); 
    }
}