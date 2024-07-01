using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    public GameObject menuJogar;
    public GameObject menuConfig;
    public GameObject menuCredits;
    public Button[] voltarBtn;
    public Button jogarBtn;
    public Button configBtn;
    public Button creditsBtn;
    public Button jogarOffBtn;
    
    // Start is called before the first frame update
    void Start()
    {
        menu.SetActive(true);
        menuJogar.SetActive(false);
        menuConfig.SetActive(false);
        menuCredits.SetActive(false);
        jogarBtn.onClick.AddListener(() => OnButtonClick(1));
        configBtn.onClick.AddListener(() => OnButtonClick(2));
        creditsBtn.onClick.AddListener(() => OnButtonClick(3));
        jogarOffBtn.onClick.AddListener(() => OnButtonClick(4));
        for(int i = 0; i < voltarBtn.Length; i++)
        {
            voltarBtn[i].onClick.AddListener(() => OnButtonClick(0));
        }
    }

    public void OnButtonClick(int acao)
    {
        if (acao == 0)
        {
            menu.SetActive(true);
            menuJogar.SetActive(false);
            menuConfig.SetActive(false);
            menuCredits.SetActive(false);
        }
        else if (acao == 1)
        {
            menu.SetActive(false);
            menuJogar.SetActive(true);
            menuConfig.SetActive(false);
            menuCredits.SetActive(false);
        }else if(acao == 2)
        {
            menu.SetActive(false);
            menuJogar.SetActive(false);
            menuConfig.SetActive(true);
            menuCredits.SetActive(false);
        }else if(acao == 3)
        {
            menu.SetActive(false);
            menuJogar.SetActive(false);
            menuConfig.SetActive(false);
            menuCredits.SetActive(true);
        }else if(acao == 4)
        {
            SceneManager.LoadScene(1);
        }
    }
}
