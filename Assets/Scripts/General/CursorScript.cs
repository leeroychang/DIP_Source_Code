using UnityEngine;

public class CursorScript : Singleton<CursorScript>
{
    [SerializeField] Texture2D pointerIcon;
    [SerializeField] Vector2 pointerOffset;
    [SerializeField] Texture2D grabIcon;
    [SerializeField] Vector2 grabOffset;
    [SerializeField] Texture2D holdIcon;
    [SerializeField] Vector2 holdOffset;
    [SerializeField] Texture2D targetIcon;
    [SerializeField] Vector2 targetOffset;

    public static void SetCursor(Mode mode)
    {
        if (Instance==null) return;
        Instance.Set(mode);
    }

    void Set(Mode mode)
    {
        switch (mode)
        {
            case Mode.Pointer:
                Cursor.SetCursor(pointerIcon,pointerOffset,CursorMode.Auto);
                break;
            case Mode.Grab:
                Cursor.SetCursor(grabIcon,grabOffset,CursorMode.Auto);
                break;
            case Mode.Hold:
                Cursor.SetCursor(holdIcon,holdOffset,CursorMode.Auto);
                break;
            case Mode.Target:
                Cursor.SetCursor(targetIcon,targetOffset,CursorMode.Auto);
                break;
        }
    }




    public enum Mode
    {
        Pointer,
        Grab,
        Hold,
        Target
    }
}
