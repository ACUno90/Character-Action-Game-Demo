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
    [Header("Style Meter")]
    [SerializeField] Image DopeImage;
    [SerializeField] Image CrazyImage;
    [SerializeField] Image BallerImage;
    [SerializeField] Image AwesomeSauceImage;
    //  [SerializeField] Image SupremeImage;
    public int meterpoints;





    [Header("other")]
    public static GameManger Instance;
    public GameObject Player;
    public Player PlayerScript;
    public Image DMG_Screen;
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
     public int enmenycount;

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
    public void StartLose()
    {
        StartPause();
        Menu_Active = Menu_Lose;
        Menu_Lose.SetActive(true);
    }


    public void updateGameGoal(int goal)
    {
        enmenycount += goal;
        if (enmenycount <= 0)
        {

            StartPause();
            Menu_Active = Menu_Win;
            Menu_Win.SetActive(Ispaused);
        }
    }


    public void PlayClickSound()
    {
        playerADU.PlayOneShot(AUDclick[Random.Range(0, AUDclick.Length)], AUDclickV);
    }

    public void StopClickSound() {
        playerADU.Stop();
    }

    public void MeterPointAddage()
    {
        meterpoints++;
        //For testing now, will change numbers later
        if (meterpoints == 1)
        {
            DopeImage.enabled = true;
        }
        else if (meterpoints == 2)
        {
            CrazyImage.enabled = true;
        }
        else if (meterpoints == 3)
        {
            BallerImage.enabled = true;
        }
        else if (meterpoints == 4)
        {
            AwesomeSauceImage.enabled = true;
        }
    }
}
