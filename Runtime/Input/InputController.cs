using KBCore.Refs;
using System;
using System.Runtime.Serialization;
using Telegraphist.Lifecycle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telegraphist.Input
{
    public enum ActionMapType
    {
        Game,
        LevelEditor,
        Skippable,
    }

    public enum ControlSchemeType
    {
        Unknown,
        [EnumMember(Value = "Keyboard & Mouse")]
        KeyboardMouse,
        [EnumMember(Value = "Gamepad")]
        Gamepad
    }
    
    [RequireComponent(typeof(PlayerInput))]
    public class InputController : SceneSingleton<InputController>
    {
        [SerializeField, Self]
        private PlayerInput playerInput;
        
        public void ToggleActionMap(ActionMapType type, bool state)
        {
            if (playerInput == null)
            {
                Debug.LogError($"err: playerInput not found", this);
                return;
            }

            var actionMap = playerInput.actions.FindActionMap(type.ToString());
            if (state)
            {
                actionMap.Enable();
            }
            else
            {
                actionMap.Disable();
            }
        }
        
        public ControlSchemeType CurrentControlScheme
        {
            get
            {
                return Enum.TryParse<ControlSchemeType>(playerInput.currentControlScheme, out var scheme)
                    ? scheme
                    : ControlSchemeType.Unknown;
            }
        }
    }
}
