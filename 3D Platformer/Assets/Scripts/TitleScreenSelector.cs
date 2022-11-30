using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class TitleScreenSelector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void OpenScene()
    {
        SceneManager.LoadScene("LevelSelection");
    }
}
