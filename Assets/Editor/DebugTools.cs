// Como este script está na pasta 'Editor', precisamos usar a biblioteca UnityEditor.
using UnityEditor;
using UnityEngine;

public class DebugTools
{
    // A "mágica" [MenuItem(...)] cria um novo botão no menu superior da Unity.
    [MenuItem("Ferramentas/Apagar Progresso (PlayerPrefs)")]
    public static void ClearPlayerPrefs()
    {
        // O mesmo comando que usamos no botão "Novo Jogo".
        PlayerPrefs.DeleteAll();
        Debug.Log("Progresso do jogador (PlayerPrefs) foi apagado com sucesso!");
    }
}