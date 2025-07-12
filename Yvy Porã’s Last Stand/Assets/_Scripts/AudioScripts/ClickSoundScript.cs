using UnityEngine;

public class ClickSoundScript : MonoBehaviour
{
    
    public void Click()
    {
        AudioManager.instance.Play("ClickSound");
    }

}
