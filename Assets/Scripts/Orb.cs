using UnityEngine;

public class Orb : MonoBehaviour
{
    public void ExecutarInteracao()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegistrarColetaDeOrb();
        }

        Destroy(gameObject);
    }
}