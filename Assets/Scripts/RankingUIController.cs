using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RankingUIController : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("O objeto 'Content' dentro do seu Scroll View.")]
    public Transform rankingListContainer; 
    
    [Tooltip("O Prefab que representa uma linha do ranking (Posição, Nome, XP).")]
    public GameObject rankingItemPrefab;

    // Esta função é chamada automaticamente toda vez que o objeto é ativado (painel abre)
    private void OnEnable()
    {
        UpdateRankingDisplay();
    }

    public void UpdateRankingDisplay()
    {
        // 1. Limpa a lista atual (deleta itens antigos)
        foreach (Transform child in rankingListContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Pede os dados ao Tesoureiro
        List<SaveLoadManager.RankingEntry> rankingData = SaveLoadManager.GetRankingList();

        if (rankingData == null || rankingData.Count == 0)
        {
            return;
        }

        // 3. Cria os itens visuais (Top 10)
        int count = 0;
        foreach (var entry in rankingData)
        {
            count++;
            if (count > 10) break; // Mostra apenas os top 10

            // Cria a linha
            GameObject newItem = Instantiate(rankingItemPrefab, rankingListContainer);
            
            // Tenta encontrar os textos pelos nomes
            TextMeshProUGUI posText = newItem.transform.Find("PosText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = newItem.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI xpText = newItem.transform.Find("XpText")?.GetComponent<TextMeshProUGUI>();

            // Configura os textos e aplica a cor CINZA a todos
            if (posText != null) 
            {
                posText.text = "#" + count;
                posText.color = Color.gray; 
            }

            if (nameText != null) 
            {
                nameText.text = entry.username;
                nameText.color = Color.gray;
            }

            if (xpText != null) 
            {
                xpText.text = entry.totalXP + " XP";
                xpText.color = Color.gray;
            }

        }
    }
}