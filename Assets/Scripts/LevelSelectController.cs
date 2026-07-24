using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.UI; 
using System.Linq; 

[System.Serializable]
public class LevelButtonUI
{
    public Button buttonComponent;
    public GameObject unlockedContent;
    public GameObject lockIcon;
    public Transform starsContainer;
}

public class LevelSelectController : MonoBehaviour
{
    // --- Referências Preservadas ---
    [Header("Botões e Assets de Nível")]
    public LevelButtonUI[] levelButtons;
    
    [Header("Sprites das Estrelas")]
    public Sprite starGoldSprite; 
    public Sprite starGreySprite; 

    // --- Variáveis Ocultas (Vinculadas por Nome) ---
    private Image displayEmblemIcon;
    private Slider displayXpBar;
    private TextMeshProUGUI displayXpText;

    private GameObject emblemInfoPanel;
    private Image infoPanelEmblemIcon;
    private TextMeshProUGUI infoPanelEmblemNameText;
    private TextMeshProUGUI infoPanelEmblemDescriptionText;
    private Slider infoPanelXpBar;
    private TextMeshProUGUI infoPanelXpText;

    private GameObject unlockedEmblemsPanel;
    private Transform emblemGridContainer; 
    private GameObject emblemDisplayPrefab; 

    // --- Dados Dinâmicos ---
    private string[] emblemNames;             
    private string[] emblemLevelDescriptions; 
    private Sprite[] unlockedEmblemSprites;   
    private Sprite[] lockedEmblemSprites;     

    void Awake() 
    {
        BindUIElementsByName();
        LoadGameAssets(); 
    }

    void Start()
    {
        if (emblemInfoPanel != null) emblemInfoPanel.SetActive(false);
        if (unlockedEmblemsPanel != null) unlockedEmblemsPanel.SetActive(false);
        
        UpdateLevelButtons();
        UpdateEmblemDisplay();
    }

    /// <summary>
    /// Vincula os componentes baseado puramente em buscas de strings.
    /// ALERTA: Os GameObjects alvo DEVEM estar ativos na hierarquia durante o Awake.
    /// </summary>
    void BindUIElementsByName()
    {
        // 1. Display Integrado
        displayEmblemIcon = GameObject.Find("EmblemIcon_Display")?.GetComponent<Image>();
        displayXpBar = GameObject.Find("XpBar_Display")?.GetComponent<Slider>();
        
        // Na imagem consta como 'None'. Assumindo um nome padrão para evitar quebra lógica.
        GameObject displayXpTextObj = GameObject.Find("DisplayXpText"); 
        if(displayXpTextObj != null) displayXpText = displayXpTextObj.GetComponent<TextMeshProUGUI>();

        // 2. Pop-up 1 (Informações)
        emblemInfoPanel = GameObject.Find("EmblemInfoPanel");
        infoPanelEmblemIcon = GameObject.Find("InfoPanel_EmblemIcon")?.GetComponent<Image>();
        infoPanelEmblemNameText = GameObject.Find("InfoPanel_EmblemNameText")?.GetComponent<TextMeshProUGUI>();
        infoPanelEmblemDescriptionText = GameObject.Find("InfoPanel_DescriptionText")?.GetComponent<TextMeshProUGUI>();
        infoPanelXpBar = GameObject.Find("InfoPanel_XpBar")?.GetComponent<Slider>();
        infoPanelXpText = GameObject.Find("InfoPanel_XpText")?.GetComponent<TextMeshProUGUI>();

        // 3. Pop-up 2 (Grade de Desbloqueados)
        unlockedEmblemsPanel = GameObject.Find("UnlockedEmblemsPanel");
        emblemGridContainer = GameObject.Find("EmblemGrid")?.GetComponent<RectTransform>();
        
        // Prefabs não podem ser achados com GameObject.Find. Deve ser carregado de Resources.
        emblemDisplayPrefab = Resources.Load<GameObject>("UI/EmblemDisplayItem_Template");
        if (emblemDisplayPrefab == null)
        {
            Debug.LogError("Falha estrutural: O Prefab 'EmblemDisplayItem_Template' deve estar localizado em uma pasta 'Resources/UI/'.");
        }
    }

    void LoadGameAssets()
    {
        unlockedEmblemSprites = Resources.LoadAll<Sprite>("Game/Emblemas/emblemas_spritesheet");
        if (unlockedEmblemSprites != null && unlockedEmblemSprites.Length > 0)
            unlockedEmblemSprites = unlockedEmblemSprites.OrderBy(s => s.name).ToArray();

        lockedEmblemSprites = Resources.LoadAll<Sprite>("Game/Emblemas/Locked/emblemas_locked_spritesheet"); 
        if (lockedEmblemSprites != null && lockedEmblemSprites.Length > 0)
            lockedEmblemSprites = lockedEmblemSprites.OrderBy(s => s.name).ToArray();

        TextAsset namesFile = Resources.Load<TextAsset>("Game/Text/emblem_names");
        if (namesFile != null)
            emblemNames = namesFile.text.Split(';');
        else
            emblemNames = new string[] { "Nível 1", "Nível 2", "Nível 3", "Nível 4", "Nível 5", "Nível 6" };

        TextAsset descFile = Resources.Load<TextAsset>("Game/Text/emblem_descriptions");
        if (descFile != null)
            emblemLevelDescriptions = descFile.text.Split(';');
        else
            emblemLevelDescriptions = new string[] { "Continue jogando!" };
    }

    void UpdateLevelButtons() 
    {
        if (GameManager.instance == null) return;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonUI currentButtonUI = levelButtons[i];
            LevelData levelData = GameManager.instance.levels[i];

            if (levelData.isUnlocked)
            {
                currentButtonUI.buttonComponent.interactable = true;
                currentButtonUI.unlockedContent.SetActive(true);
                currentButtonUI.lockIcon.SetActive(false);

                if (currentButtonUI.starsContainer != null && starGoldSprite != null && starGreySprite != null)
                {
                    for (int j = 0; j < currentButtonUI.starsContainer.childCount; j++)
                    {
                        Image starImage = currentButtonUI.starsContainer.GetChild(j).GetComponent<Image>();
                        starImage.sprite = (j < levelData.starsEarned) ? starGoldSprite : starGreySprite;
                    }
                }
            }
            else
            {
                currentButtonUI.buttonComponent.interactable = false;
                currentButtonUI.unlockedContent.SetActive(false);
                currentButtonUI.lockIcon.SetActive(true);
            }
        }
    }

    void UpdateEmblemDisplay() 
    {
        if (EmblemManager.instance == null) return;
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
        if (EmblemManager.instance == null) return;
        Emblem emblemData = EmblemManager.instance.playerEmblem;

        if(infoPanelEmblemNameText != null && emblemNames != null && emblemNames.Length > 0)
        {
            int nameIndex = Mathf.Clamp(emblemData.currentLevel - 1, 0, emblemNames.Length - 1);
            infoPanelEmblemNameText.text = emblemNames[nameIndex];
        }

        if(infoPanelXpBar != null)
        {
            infoPanelXpBar.maxValue = emblemData.xpToNextLevel;
            infoPanelXpBar.value = emblemData.currentXP;
        }
        if(infoPanelXpText != null) infoPanelXpText.text = emblemData.currentXP + " / " + emblemData.xpToNextLevel + " XP";
        
        UpdateEmblemIcon(infoPanelEmblemIcon, emblemData);

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
            int spriteIndex = Mathf.Clamp(emblemData.currentLevel - 1, 0, unlockedEmblemSprites.Length - 1);
            iconImage.sprite = unlockedEmblemSprites[spriteIndex];
        }
    }

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

    public void GoToMainMenu() 
    { 
        SceneManager.LoadScene("TelaInicial"); 
    }
}