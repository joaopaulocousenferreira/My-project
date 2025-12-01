using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveLoadManager
{
    public class RankingEntry
    {
        public string username;
        public int totalXP; // Agora isso vai representar o XP Total Real
        public int emblemLevel;
    }

    public static void SaveGame()
    {
        if (GameManager.instance == null || EmblemManager.instance == null)
        {
            Debug.LogError("Managers não encontrados.");
            return;
        }

        string userID = GameManager.instance.currentUserID;
        if (string.IsNullOrEmpty(userID)) return;

        Debug.Log("--- SALVANDO PROGRESSO PARA: " + userID + " ---");

        // Salva dados do Emblema
        PlayerPrefs.SetInt(userID + "_EmblemLevel", EmblemManager.instance.playerEmblem.currentLevel);
        PlayerPrefs.SetInt(userID + "_EmblemXP", EmblemManager.instance.playerEmblem.currentXP);
        PlayerPrefs.SetInt(userID + "_EmblemXPToNext", EmblemManager.instance.playerEmblem.xpToNextLevel);
        
        // --- NOVO: Salva o XP Total Acumulado ---
        PlayerPrefs.SetInt(userID + "_EmblemTotalXP", EmblemManager.instance.playerEmblem.totalAccumulatedXP);
        // ----------------------------------------

        // Salva dados dos Níveis
        for (int i = 0; i < GameManager.instance.levels.Length; i++)
        {
            int isUnlocked = GameManager.instance.levels[i].isUnlocked ? 1 : 0;
            PlayerPrefs.SetInt(userID + "_Level_" + i + "_Unlocked", isUnlocked);
            PlayerPrefs.SetInt(userID + "_Level_" + i + "_Stars", GameManager.instance.levels[i].starsEarned);
        }

        PlayerPrefs.SetInt("HasPlayedBefore", 1);
        PlayerPrefs.Save();
    }

    public static void LoadGame(string userID)
    {
        if (GameManager.instance == null || EmblemManager.instance == null) return;

        GameManager.instance.currentUserID = userID;
        EmblemManager.instance.currentUserID = userID;

        Debug.Log("--- CARREGANDO PROGRESSO DE: " + userID + " ---");

        // Carrega dados do Emblema
        EmblemManager.instance.playerEmblem.currentLevel = PlayerPrefs.GetInt(userID + "_EmblemLevel", 1);
        EmblemManager.instance.playerEmblem.currentXP = PlayerPrefs.GetInt(userID + "_EmblemXP", 0);
        EmblemManager.instance.playerEmblem.xpToNextLevel = PlayerPrefs.GetInt(userID + "_EmblemXPToNext", 100);
        
        // --- NOVO: Carrega o XP Total Acumulado ---
        EmblemManager.instance.playerEmblem.totalAccumulatedXP = PlayerPrefs.GetInt(userID + "_EmblemTotalXP", 0);
        // ------------------------------------------

        // Carrega dados dos Níveis
        for (int i = 0; i < GameManager.instance.levels.Length; i++)
        {
            bool defaultUnlockedState = (!GameManager.instance.useLevelLocking || i == 0);
            int defaultUnlockedInt = defaultUnlockedState ? 1 : 0;
            int isUnlockedInt = PlayerPrefs.GetInt(userID + "_Level_" + i + "_Unlocked", defaultUnlockedInt);
            
            GameManager.instance.levels[i].isUnlocked = (isUnlockedInt == 1);
            GameManager.instance.levels[i].starsEarned = PlayerPrefs.GetInt(userID + "_Level_" + i + "_Stars", 0);
        }
    }

    public static List<RankingEntry> GetRankingList()
    {
        List<RankingEntry> leaderboard = new List<RankingEntry>();
        string registry = PlayerPrefs.GetString("RegisteredUsersRegistry", "");
        
        if (!string.IsNullOrEmpty(registry))
        {
            string[] allUsers = registry.Split(',');

            foreach (string user in allUsers)
            {
                if (string.IsNullOrEmpty(user) || user == "local_anonymous_user") continue;

                // --- CORREÇÃO DO RANKING ---
                // Agora buscamos o '_EmblemTotalXP' em vez do '_EmblemXP' parcial
                int totalXP = PlayerPrefs.GetInt(user + "_EmblemTotalXP", 0);
                int lvl = PlayerPrefs.GetInt(user + "_EmblemLevel", 1);

                leaderboard.Add(new RankingEntry { username = user, totalXP = totalXP, emblemLevel = lvl });
            }
        }

        return leaderboard.OrderByDescending(x => x.totalXP).ToList();
    }

    public static void DeleteAllProgress()
    {
        Debug.LogWarning("--- PROGRESSO APAGADO ---");
        PlayerPrefs.DeleteAll();
    }
}