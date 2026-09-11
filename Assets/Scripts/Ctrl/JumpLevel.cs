using System.Collections;
using System.Collections.Generic;
using QFramework;
using QFramework.Example;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JumpLevel : MonoBehaviour, ICanSendEvent, ICanGetUtility
{
    public TMP_InputField inputField;
    public Button button;
    public Button Btnfinish;
    public Button BtnClose;
    public GameObject debugPanel;
    int i = 0;

    // 3指长按打开调试面板
    private float threeFingerPressTimer = 0f;
    private const float THREE_FINGER_LONG_PRESS_DURATION = 1.5f;
    private const int REQUIRED_FINGER_COUNT = 3;
    public IArchitecture GetArchitecture()
    {
        return GameMainArc.Interface;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        button.onClick.AddListener(() =>
        {
            LevelManager.Instance.StartGame(int.Parse(inputField.text));
            this.GetUtility<SaveDataUtility>().SaveLevel(int.Parse(inputField.text));
            UIKit.ClosePanel<UIGameNode>();
            UIKit.OpenPanel<UIGameNode>();
            //this.SendEvent<GameStartEvent>();
            //GameCtrl.Instance.InitGameCtrl();
        });

        Btnfinish.onClick.AddListener(() =>
        {
            StartCoroutine(LevelManager.Instance.TestFinish());
        });

        BtnClose.onClick.AddListener(() =>
        {
            debugPanel.SetActive(false);
        });
    }

    // Update is called once per frame
    void Update()
    {
// #if UNITY_EDITOR
//         if (Input.GetKeyDown(KeyCode.A))
//         {
//             for (int i = 0; i < 2; i++)
//                 this.SendEvent(new ReturnToMainEvent { PassLevel = true });
//         }

//         if (Input.GetKeyDown(KeyCode.S))
//         {
//             for (int i = 0; i < 10; i++)
//                 this.SendEvent(new ReturnToMainEvent { PassLevel = true });
//         }

//         if (Input.GetKey(KeyCode.L))
//         {
//             LevelManager.Instance.AddMoveNum();
//         }

//         if (Input.GetKeyDown(KeyCode.H))
//         {
//             debugPanel.SetActive(true);
//         }

//         if (Input.GetKeyDown(KeyCode.G))
//         {
//             i++;
//             LevelManager.Instance.StartGame(this.GetUtility<SaveDataUtility>().GetCurrentLevel() + i);
//         }

//         if (Input.GetKeyDown(KeyCode.O))
//         {
//             LevelManager.Instance.CurtainUpdate();
//         }
// #endif

//         // 3指长按打开调试面板（全平台生效）
//         CheckThreeFingerLongPress();
    }

    private void CheckThreeFingerLongPress()
    {
        int fingerCount = Input.touchCount;

        if (fingerCount >= REQUIRED_FINGER_COUNT)
        {
            // 检查所有触摸点是否都在按压状态
            bool allFingersStationary = true;
            for (int t = 0; t < fingerCount; t++)
            {
                if (Input.GetTouch(t).phase == TouchPhase.Moved ||
                    Input.GetTouch(t).phase == TouchPhase.Ended ||
                    Input.GetTouch(t).phase == TouchPhase.Canceled)
                {
                    allFingersStationary = false;
                    break;
                }
            }

            if (allFingersStationary)
            {
                threeFingerPressTimer += Time.deltaTime;
                if (threeFingerPressTimer >= THREE_FINGER_LONG_PRESS_DURATION)
                {
                    debugPanel.SetActive(true);
                    threeFingerPressTimer = 0f;
                }
            }
            else
            {
                threeFingerPressTimer = 0f;
            }
        }
        else
        {
            threeFingerPressTimer = 0f;
        }
    }
}
