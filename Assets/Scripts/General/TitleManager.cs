using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] string TutorialScene;
    [SerializeField] string gameScene;

    [Header("Menu References")]
    [SerializeField] GameObject queryTutMenu;
    [SerializeField] GameObject optionMenu;
    [SerializeField] GameObject exitMenu;

    [Header("Other References")]
    [SerializeField] InputActionAsset actionAsset;
    [SerializeField] GameObject MainButtonGroup;
    [SerializeField] GameObject queryDefaultButton;
    [SerializeField] GameObject optionDefaultButton;
    [SerializeField] GameObject exitDefaultButton;


    InputActionMap titleActionMap;
    InputAction navigationAction;
    InputAction closeAction;

    void Start()
    {
        queryTutMenu.SetActive(false);
        optionMenu.SetActive(false);
        exitMenu.SetActive(false);
        
        if (actionAsset==null) return;
        titleActionMap = actionAsset.FindActionMap("TitleMap");
        if (titleActionMap==null) return;
        navigationAction = titleActionMap.FindAction("Navigation");
        closeAction = titleActionMap.FindAction("Close");
        if (navigationAction!=null)
        {
            navigationAction.Enable();
            navigationAction.performed += OnNavigation;
        }
        if (closeAction!=null)
        {
            closeAction.Enable();
            closeAction.performed += Close;
        }

        CursorScript.SetCursor(CursorScript.Mode.Pointer);
    }

    void OnDisable()
    {
        if (navigationAction!=null)
        {
            navigationAction.Disable();
            navigationAction.performed -= OnNavigation;
        }
        if (closeAction!=null)
        {
            closeAction.Enable();
            closeAction.performed -= Close;
        }
    }


    public void StartButton()
    {
        MainButtonGroup.SetActive(false);
        queryTutMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ChangeScene(bool tut)
    {
        if (PersistentData.Instance!=null) PersistentData.Instance.StartTime = Time.time;
        SceneManager.LoadScene(tut ? TutorialScene : gameScene);
    }

    public void OptionButton()
    {
        MainButtonGroup.SetActive(false);
        optionMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Close()
    {
        //Toggle Menus
        if (queryTutMenu.activeSelf)
        {
            queryTutMenu.SetActive(false);
            MainButtonGroup.SetActive(true);
        }
        else if (optionMenu.activeSelf)
        {
            optionMenu.SetActive(false);
            MainButtonGroup.SetActive(true);
        }
        else
        {
            MainButtonGroup.SetActive(exitMenu.activeSelf);
            exitMenu.SetActive(!exitMenu.activeSelf);
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Close(InputAction.CallbackContext context) { Close(); }

    public void Exit() { Application.Quit(); }

    public void OnNavigation(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject!=null) return;
        if (queryTutMenu.activeSelf && queryDefaultButton!=null) EventSystem.current.SetSelectedGameObject(queryDefaultButton);
        else if (optionMenu.activeSelf && optionDefaultButton!=null)EventSystem.current.SetSelectedGameObject(optionDefaultButton);
        else if (exitMenu.activeSelf && exitDefaultButton!=null)EventSystem.current.SetSelectedGameObject(exitDefaultButton);
        else EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
    }
}
