using UnityEngine;
using UnityEngine.SceneManagement;

public class MudarCena : MonoBehaviour
{
    [ContextMenu("CarregarCena")]
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }

    [ContextMenu("Executar Método")]
    private void ExecutarMetodo()
    {
            SceneManager.LoadScene("SelecaoDeFase");
    }
}