using UnityEngine;

public class EmblemManager : MonoBehaviour
{
    // Padrão Singleton para garantir acesso global e único.
    public static EmblemManager instance;

    [Header("Estado Atual")]
    [Tooltip("Identifica o jogador atual cujos dados de emblema estão carregados.")]
    public string currentUserID; // <-- A variável-chave para o sistema de login

    [Header("Dados do Emblema")]
    [Tooltip("Contém os dados do emblema do jogador atualmente logado.")]
    public Emblem playerEmblem; // Referencia a classe 'Emblem' do seu outro arquivo

    // O método Awake é chamado antes de qualquer método Start.
    void Awake()
    {
        // Lógica do Singleton
        if (instance != null && instance != this)
        {
            // Se já existe uma instância e não sou eu, destruo-me.
            Destroy(this.gameObject);
            return;
        }
        // Se não existe uma instância, eu torno-me a instância.
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Inicializa o objeto 'playerEmblem' se ele for nulo.
        // Isto previne erros caso o 'EmblemManager' seja criado
        // sem um 'Emblem' pré-configurado no Inspector.
        if (playerEmblem == null)
        {
            playerEmblem = new Emblem();
        }
    }
}