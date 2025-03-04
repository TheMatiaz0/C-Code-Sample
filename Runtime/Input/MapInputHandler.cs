using Telegraphist.Helpers;
using Telegraphist.UI.Menus.Map;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telegraphist.Input
{
    public class MapInputHandler : MonoBehaviour
    {
        [SerializeField]
        private MenuMapController menuMapController;

        [SerializeField]
        private InputActionReference navigateReference;
        [SerializeField]
        private InputActionReference submitReference;

        private float previousX;

        private void Awake()
        {
            navigateReference.ToInputAction().performed += OnNavigate;
            submitReference.ToInputAction().performed += OnSubmit;
        }

        private void OnDestroy()
        {
            navigateReference.ToInputAction().performed -= OnNavigate;
            submitReference.ToInputAction().performed -= OnSubmit;
        }

        private void OnSubmit(InputAction.CallbackContext obj)
        {
            menuMapController.PlayLevel();
        }
        
        private void OnNavigate(InputAction.CallbackContext obj)
        {
            var vect = obj.ReadValue<Vector2>();
            if (previousX != 0)
            {
                previousX = vect.x;
                return;
            }
            
            if (vect.x >= 1)
            {
                menuMapController.SelectNextLevel();
            }
            else if (vect.x <= -1)
            {
                menuMapController.SelectPreviousLevel();
            }
            
            previousX = vect.x;
        }
    }
}
