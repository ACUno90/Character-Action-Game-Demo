using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
using System.Data;

public class GameManger : MonoBehaviour
{

    [Header("Menues")]
    [SerializeField] GameObject Menu_Active;
    [SerializeField] GameObject Menu_Win;
    [SerializeField] GameObject Menu_Pause;
    [SerializeField] GameObject Menu_Lose;

    [Header("other")]
    public static GameManger Instance;
    public GameObject Player;
    public Player PlayerScript;
    public GameObject DMG_Screen;
    public Image Player_HP_Bar;
    public Image PlayerHealthColor;
    public Slider slider;
    public Gradient healthGradient;


    [Header("audio")]
    public AudioClip[] AUDclick;
    public float AUDclickV;
    public AudioSource playerADU;
    public bool Ispaused;
    float timeScale_OG;
    float safe;
    float healthPercentage;

    void Awake()
    {
        Instance = this;
        timeScale_OG = Time.timeScale;
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerScript = Player.GetComponent<Player>();
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (Menu_Active == null)
            {
                StartPause();
                Menu_Active = Menu_Pause;
                Menu_Active.SetActive(Ispaused);
            }
            else if (Menu_Active == Menu_Pause)
            {
                startUnPause();
            }
        }
        UpdateHealthBar();

    }


    public void StartPause()
    {
        Ispaused = !Ispaused;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Player.GetComponentInChildren<PlayerCamera>().enabled = false;
        PlayerScript.enabled = false;


    }


    public void startUnPause()
    {
        Ispaused = !Ispaused;
        Time.timeScale = timeScale_OG;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Menu_Active.SetActive(Ispaused);
        Menu_Active = null;
        PlayerScript.enabled = true;
        Player.GetComponentInChildren<PlayerCamera>().enabled = true;

    }

    public void UpdateHealthBar()
    {
        //PlayerHealthColor.color = new Color(255, 1, 0, 0);
        //if (PlayerScript != null)
        //{
        //    healthPercentage = 0;
        //    safe = 0;
        //    //  hp_text.text = Player.GetComponent<PlayerMovement>().health.ToString("F0");
        //    // Assuming PlayerScript has a 'health' and 'maxHealth' variable
        //    healthPercentage = Player.GetComponent<Player>().health / Player.GetComponent<Player>().maxHealth;
        //    safe = 70 / 100;
        //    safe = 1 - safe;
        //    safe = safe / 3;
        //    Player_HP_Bar.fillAmount = healthPercentage; // Updates the health bar fill amount
        //    healthPercentage = 1 - healthPercentage;
        //    healthPercentage = healthPercentage / 3;
        //    if (healthPercentage < safe)
        //    {
        //        PlayerHealthColor.color = new Color(255, 1, 0, healthPercentage);
        //    }
        //    else
        //    {
        //        PlayerHealthColor.color = new Color(255, 1, 0, safe);
        //    }


        //}
        setHealth();
        SetMaxHealth();
    }

    public void setHealth()
    {
        slider.value = PlayerScript.health;
        PlayerHealthColor.color = healthGradient.Evaluate(slider.normalizedValue);

    }

    public void SetMaxHealth()
    {
        slider.maxValue = PlayerScript.maxHealth;
        slider.value = PlayerScript.health;
        PlayerHealthColor.color = healthGradient.Evaluate(1f);

    }
    //public IEnumerator dmgflash()
    //{

    //    DMG_Screen.SetActive(true);
    //    yield return new WaitForEndOfFrame();
    //    DMG_Screen.SetActive(false);
    //}11
}
