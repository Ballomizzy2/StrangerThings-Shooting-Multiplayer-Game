using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockMouse : MonoBehaviour
{
    private bool _isMouseLocked;
    private void Start()
    {
        ToggleMouseLock(true);
    }

    private void ToggleMouseLock(bool lockState)
    {
        if (lockState)
            Cursor.lockState = CursorLockMode.Locked;        
        else
            Cursor.lockState = CursorLockMode.None;
        Cursor.visible = !lockState;
        _isMouseLocked = lockState;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMouseLock(!_isMouseLocked);
    }
}
