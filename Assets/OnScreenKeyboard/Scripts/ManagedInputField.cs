using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PrabdeepDhaliwal.OnScreenKeyboard
{
    // Manages the interaction between a TMP_InputField and the on-screen keyboard, supporting both gamepad and mouse.
    public class ManagedInputField : MonoBehaviour, IPointerClickHandler
    {
        #region Fields
        public Color normalColor;           // Normal color for the input field when not selected.
        public Color selectedColor;         // Color when the input field is selected.

        private TMP_InputField inputField;  // Reference to the TMP_InputField.
        private InputActions action;  // Player input actions for gamepad navigation. (Replace this with the name of your created InputActions)

        private bool selected;              // Flag indicating whether the input field is selected.
        private bool isMouseClick;          // Flag to indicate if input source is a mouse click.
        #endregion

        #region Init & Update
        private void Awake()
        {
            action = new InputActions(); // Initialize input actions.
        }
        private void Start()
        {
            inputField = GetComponent<TMP_InputField>(); // Get the TMP_InputField attached to this GameObject.
            inputField.interactable = false; // Make the input field non-interactive initially.
            inputField.resetOnDeActivation = false;
            inputField.caretBlinkRate = 2f;
        }
        private void OnDisable()
        {
            // Hide the keyboard when the script is disabled.
            if (OnScreenKeyboard.Instance != null)
            {
                OnScreenKeyboard.Instance.gameObject.SetActive(false);
            }

            // safety measures
            action.UI.Select.performed -= _ => Selected();
            action.UI.Deselect.performed -= _ => Deselected();
            action.UI.Select.Disable(); 
            action.UI.Deselect.Disable();
        }

        private void Update()
        {
            // Check if the input field is selected either via gamepad or mouse click
            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject && !selected)
            {
                action.UI.Deselect.Disable();
                action.UI.Select.Enable();
                action.UI.Select.performed += _ => Selected(); // Select the input field on action.

                SetInputFieldColor(selectedColor);
            }
            else
            {
                action.UI.Select.Disable(); // Disable select action if not selected.

                SetInputFieldColor(normalColor);
            }

            // Enable deselection if input field is selected
            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject && selected)
            {
                action.UI.Deselect.Enable();
                action.UI.Deselect.performed += _ => Deselected(); // Deselect input field on action.
            }
        }
        #endregion

        #region Events
        // Called when the input field is selected (via gamepad or mouse)
        private void Selected(bool fromMouse = false)
        {
            selected = true;
            isMouseClick = fromMouse;
            inputField.interactable = true;
            action.UI.Select.performed -= _ => Selected(false);

            // Show keyboard only if not mouse input.
            if (!isMouseClick)
            {
                OnScreenKeyboard.Instance.gameObject.SetActive(true);
                OnScreenKeyboard.Instance.Setup(inputField);
            }
        }

        // Called when the input field is deselected (via gamepad or mouse)
        public void Deselected()
        {
            OnScreenKeyboard.Instance.gameObject.SetActive(false); 
            selected = false;
            inputField.interactable = false; 
            inputField.Select(); // Refocus on the input field.
            action.UI.Deselect.performed -= _ => Deselected(); // Unsubscribe from deselect action.
        }
        #endregion

        #region Mouse Interaction
        // Called when the input field is clicked with the mouse
        public void OnPointerClick(PointerEventData eventData)
        {
            // Select the input field if not already selected
            if (!selected)
            {
                Selected(false); // Pass `true` to indicate selection from mouse click.
                inputField.Select(); // Immediately allows typing with the mouse.
            }
        }
        #endregion

        #region Helper Methods
        private void SetInputFieldColor(Color color)
        {
            ColorBlock cb = inputField.colors;
            cb.disabledColor = color;
            inputField.colors = cb;
        }
        #endregion
    }
}
