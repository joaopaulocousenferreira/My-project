using UnityEngine;

[System.Serializable]
public class Emblem
{
    public string emblemName = "Emblema do Explorador";
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    // Guarda a soma de todo o XP que o jogador já ganhou na vida.
    public int totalAccumulatedXP = 0; 

    public void AddXP(int xpGained)
    {
        currentXP += xpGained;
        
        totalAccumulatedXP += xpGained; 

        // O loop de level up continua igual (consome apenas o currentXP)
        while (currentXP >= xpToNextLevel)
        {
            currentLevel++;
            currentXP -= xpToNextLevel;
            xpToNextLevel = Mathf.FloorToInt(xpToNextLevel * 1.2f); 
        }
    }
}