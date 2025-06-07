using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScenesManager : MonoBehaviour
{
    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene(1); 
        
    }
    public void MenuPrincipal()
    {
        SceneManager.LoadScene(0); 
        
    }
}
