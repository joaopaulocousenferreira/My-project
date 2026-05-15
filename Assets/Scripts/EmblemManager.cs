using UnityEngine;

public class EmblemManager : MonoBehaviour
{
    // Padrão Singleton para garantir acesso global e único.
    public static EmblemManager instance;

    [Header("Estado Atual")]
    [Tooltip("Identifica o jogador atual cujos dados de emblema estão carregados.")]
    public string currentUserID; 

    [Header("Dados do Emblema")]
    [Tooltip("Contém os dados do emblema do jogador atualmente logado.")]
    public Emblem playerEmblem; 

    void Awake()
    {
        // 1. Lógica do Singleton Anti-Clonagem
        if (instance != null && instance != this)
        {
            // Se já existe uma instância na memória, a cópia nova se mata imediatamente.
            Destroy(this.gameObject);
            return;
        }
        
        // 2. Assunção do Trono
        instance = this;

        // 3. A DECAPITAÇÃO DO PREFAB (O Segredo da Sobrevivência)
        // Arranca este objeto de qualquer "Pai" ou "Canvas" que o esteja segurando.
        // Ele vai forçadamente para a raiz da hierarquia, garantindo que o comando abaixo funcione.
        transform.SetParent(null);

        // 4. Imortalidade Absoluta
        DontDestroyOnLoad(this.gameObject);

        // 5. Prevenção de NullReferenceException
        // Inicializa o objeto 'playerEmblem' se ele for nulo.
        if (playerEmblem == null)
        {
            playerEmblem = new Emblem();
        }
    }
}