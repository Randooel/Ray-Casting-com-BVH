using UnityEngine;
using UnityEngine.InputSystem;

public class MouseModeController : MonoBehaviour
{
    #region Input Action related
    [SerializeField] private InputActionAsset _inputAsset;
    private InputAction _toggleMouseLock;

    void OnEnable()
    {
        _toggleMouseLock = _inputAsset.FindAction("Exit");
        _toggleMouseLock.performed += ToggleMouseLockState;    
    }
    void OnDisable()
    {
        _toggleMouseLock.performed -= ToggleMouseLockState;
    }
    #endregion

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ToggleMouseLockState(InputAction.CallbackContext context)
    {
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else Cursor.lockState = CursorLockMode.Locked;
    }
}
