using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.UI; 
using System.Linq; 

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class LevelButtonUI
{
    public Button buttonComponent; 
    
    private GameObject unlockedContent;
    private GameObject lockIcon;
    private Transform starsContainer;

    public GameObject UnlockedContent => unlockedContent;
    public GameObject LockIcon => lockIcon;
    public Transform StarsContainer => starsContainer;

    public void BindChildrenByName()
    {
        if (buttonComponent == null) return;
        
        Transform[] allChildren = buttonComponent.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allChildren)
        {
            if (child.name == "UnlockedContent") unlockedContent = child.gameObject;
            else if (child.name == "LockIcon") lockIcon = child.gameObject;
            else if (child.name == "StarsContainer") starsContainer = child;
        }
    }
}

public class LevelSelectController : MonoBehaviour
{
    [Header("Botões e Assets de Nível")]
    public LevelButtonUI[] levelButtons;
    
    [Header("Sprites das Estrelas")]
    public Sprite starGoldSprite; 
    public Sprite starGreySprite; 

    // Ocultado do Inspector para evitar interação manual. Serializado para persistir na Build.
    [HideInInspector]
    [SerializeField] 
    private GameObject emblemDisplayPrefab; 

    // --- Variáveis Ocultas ---
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

    // --- Dados Dinâmicos ---
    private string[] emblemNames;             
    private string[] emblemLevelDescriptions; 
    private Sprite[] unlockedEmblemSprites;   
    private Sprite[] lockedEmblemSprites;     

#if UNITY_EDITOR
    /// <summary>
    /// Automatiza a busca do Prefab sem intervenção manual (Drag and Drop).
    /// Executado automaticamente pelo Editor sempre que o script é atualizado ou a cena é modificada.
    /// </summary>
    void OnValidate()
    {
        if (emblemDisplayPrefab == null)
        {
            string prefabPath = "Assets/Prefabs/EmblemDisplayItem_Template.prefab";
            emblemDisplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (emblemDisplayPrefab == null)
            {
                Debug.LogWarning($"[Automated Binding] Falha ao localizar o prefab no caminho estrito: {prefabPath}");
            }
        }
    }
#endif

    void Awake() 
    {
        if (emblemDisplayPrefab == null)
        {
            Debug.LogError("Falha fatal estrutural: O Prefab 'EmblemDisplayItem_Template' não foi alocado na compilação.");
        }

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

    void BindUIElementsByName()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].BindChildrenByName();
        }

        Transform rootCanvas = this.transform.root;
        Transform[] allUIElements = rootCanvas.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allUIElements)
        {
            switch (t.name)
            {
                case "EmblemIcon_Display": displayEmblemIcon = t.GetComponent<Image>(); break;
                case "XpBar_Display": displayXpBar = t.GetComponent<Slider>(); break;
                case "DisplayXpText": displayXpText = t.GetComponent<TextMeshProUGUI>(); break;
                case "EmblemInfoPanel": emblemInfoPanel = t.gameObject; break;
                case "InfoPanel_EmblemIcon": infoPanelEmblemIcon = t.GetComponent<Image>(); break;
                case "InfoPanel_EmblemNameText": infoPanelEmblemNameText = t.GetComponent<TextMeshProUGUI>(); break;
                case "InfoPanel_DescriptionText": infoPanelEmblemDescriptionText = t.GetComponent<TextMeshProUGUI>(); break;
                case "InfoPanel_XpBar": infoPanelXpBar = t.GetComponent<Slider>(); break;
                case "InfoPanel_XpText": infoPanelXpText = t.GetComponent<TextMeshProUGUI>(); break;
                case "UnlockedEmblemsPanel": unlockedEmblemsPanel = t.gameObject; break;
                case "EmblemGrid": emblemGridContainer = t.GetComponent<RectTransform>(); break;
            }
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
                if (currentButtonUI.UnlockedContent != null) currentButtonUI.UnlockedContent.SetActive(true);
                if (currentButtonUI.LockIcon != null) currentButtonUI.LockIcon.SetActive(false);

                if (currentButtonUI.StarsContainer != null && starGoldSprite != null && starGreySprite != null)
                {
                    for (int j = 0; j < currentButtonUI.StarsContainer.childCount; j++)
                    {
                        Image starImage = currentButtonUI.StarsContainer.GetChild(j).GetComponent<Image>();
                        starImage.sprite = (j < levelData.starsEarned) ? starGoldSprite : starGreySprite;
                    }
                }
            }
            else
            {
                currentButtonUI.buttonComponent.interactable = false;
                if (currentButtonUI.UnlockedContent != null) currentButtonUI.UnlockedContent.SetActive(false);
                if (currentButtonUI.LockIcon != null) currentButtonUI.LockIcon.SetActive(true);
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