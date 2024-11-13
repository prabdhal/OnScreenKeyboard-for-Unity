using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrabdeepDhaliwal.OnScreenKeyboard
{
    public class OnScreenKeyboard : MonoBehaviour
    {
        #region Singleton
        public static OnScreenKeyboard Instance;
        private void Awake()
        {
            // Singleton pattern ensures only one instance of OnScreenKeyboard exists
            if (Instance == null)
                Instance = this;
            else
                Destroy(this); // Destroy the duplicate instance if it already exists
        }
        #endregion

        #region Fields
        [Header("Basic Keys")]
        public Key[] keys; // Array of Key objects representing alphabet/symbol keys

        [Space]
        [Header("Special Keys")]
        public Button shiftButton;       // Button to toggle Shift mode
        public Button capsLockButton;    // Button to toggle Caps Lock mode
        public Button enterButton;       // Button to submit text
        public Button backspaceButton;   // Button to delete the previous character
        public Button deleteButton;      // Button to delete the character at caret
        public Button spaceButton;       // Button to insert a space
        public Button leftArrowButton;   // Button to move caret left
        public Button rightArrowButton;  // Button to move caret right
        public Button upArrowButton;     // Button to move caret up

        public List<string> ignoreKeyNames = new List<string> // Keys to ignore for special functionality
        {
            "Shift",
            "Caps",
            "Enter",
            "Delete",
            "Back",
            "Space",
            "LeftArrow",
            "RightArrow"
        };

        private bool isCapsLockOn;  // Whether Caps Lock is enabled
        private bool isShiftPressed; // Whether Shift key is pressed

        private TMP_InputField inputField;  // Reference to TMP_InputField component for text input
        private GameObject lastSelectedButton; // Store last selected button (used for refocus)
        #endregion

        #region Initialization
        private void Start()
        {
            // Set up key buttons with their respective listeners
            foreach (var key in keys)
            {
                key.Setup();
                key.button.onClick.AddListener(() => OnKeyPress(key));
            }

            AddButtonListeners();
            ToggleKeyboardVisibility(); // Initially toggle keyboard visibility
        }

        private void OnEnable() => ResetKeyState();
        private void OnDisable()
        {
            inputField = null;  // Clear the reference to the input field
            lastSelectedButton = null; // Clear the last selected button
        }

        public void Setup(TMP_InputField inputField)
        {
            this.inputField = inputField;
            ResetKeyState();
        }
        #endregion

        #region Key Press Handling
        private void OnKeyPress(Key key)
        {
            string keyText = GetKeyText(key);
            InsertCharacter(keyText);

            if (isShiftPressed) ToggleShift();
        }
        private string GetKeyText(Key key)
        {
            // This method determines the correct key text based on Shift and Caps Lock states.
            if (isShiftPressed)
            {
                // If Shift is pressed, use secondary value for special characters or swap letter case
                if (string.IsNullOrEmpty(key.secondaryValue))
                {
                    return key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0])
                        ? (isCapsLockOn ? key.primaryValue.ToLower() : key.primaryValue.ToUpper())
                        : key.primaryValue;  // If it's not a letter, use the primary value (unchanged for Shift)
                }

                return key.secondaryValue;  // Use secondary for non-letters when Shift is on
            }
            else
            {
                // If Shift is not pressed, use the correct text based on Caps Lock
                return isCapsLockOn
                    ? key.primaryValue.ToUpper()  // Caps Lock ON means uppercase letters
                    : key.primaryValue.ToLower(); // Caps Lock OFF means lowercase letters
            }
        }

        private void ToggleCapsLock()
        {
            isShiftPressed = false;
            isCapsLockOn = !isCapsLockOn;
            UpdateKeyLabels();
        }
        private void ToggleShift()
        {
            isShiftPressed = !isShiftPressed;
            UpdateKeyLabels();
        }

        private void UpdateKeyLabels()
        {
            // Iterate through each key and update the labels based on Caps Lock and Shift states
            foreach (var key in keys)
            {
                if (key.primaryKeyText == null || key.secondaryKeyText == null || ignoreKeyNames.Contains(key.primaryValue))
                    continue;

                if (isShiftPressed)
                    UpdateKeyTextForShift(key);
                else
                    UpdateKeyTextForNormal(key);
            }
        }

        private void UpdateKeyTextForShift(Key key)
        {
            if (string.IsNullOrEmpty(key.secondaryValue))
            {
                key.primaryKeyText.text = key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0])
                    ? (isCapsLockOn ? key.primaryValue.ToLower() : key.primaryValue.ToUpper())
                    : key.primaryValue;
            }
            else
            {
                SwapKeyTexts(key);
            }
        }
        private void UpdateKeyTextForNormal(Key key)
        {
            if (isCapsLockOn)
            {
                if (key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0]))
                    key.primaryKeyText.text = key.primaryValue.ToUpper();
                else
                    key.primaryKeyText.text = key.primaryValue;
            }
            else
            {
                key.primaryKeyText.text = key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0])
                    ? key.primaryValue.ToLower()
                    : key.primaryValue;
            }
            key.secondaryKeyText.text = key.secondaryValue;
        }
        private void SwapKeyTexts(Key key)
        {
            var temp = key.primaryKeyText.text;
            key.primaryKeyText.text = key.secondaryValue;
            key.secondaryKeyText.text = temp;
        }
        #endregion

        #region Character Insertion and Deletion
        private void InsertCharacter(string character)
        {
            if (string.IsNullOrEmpty(character)) return;

            int start = Mathf.Min(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition);
            int end = Mathf.Max(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition);

            if (start != end)
            {
                // If there's selected text, replace it with the new character
                inputField.text = inputField.text.Remove(start, end - start).Insert(start, character);
                inputField.caretPosition = start + character.Length;
                Debug.Log("caret position: " + inputField.caretPosition);
            }
            else
            {
                // If no text is selected, insert the character at the caret position
                inputField.text = inputField.text.Insert(inputField.caretPosition, character);
                inputField.caretPosition += character.Length;
                Debug.Log("caret position: " + inputField.caretPosition);
            }
        }
        private void InsertSpaceCharacter() => InsertCharacter(" ");

        private void DeleteCharacter()
        {
            int start = Mathf.Min(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition);
            int end = Mathf.Max(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition);

            if (start != end)
            {
                // If there's selected text, delete it
                inputField.text = inputField.text.Remove(start, end - start);
                inputField.caretPosition = start;
            }
            else if (inputField.caretPosition > 0)
            {
                // If no text is selected, delete the character to the left of the caret
                int caretPos = inputField.caretPosition;
                inputField.text = inputField.text.Remove(caretPos - 1, 1);
                inputField.caretPosition = caretPos - 1;
            }
        }
        private void DeleteCharacterAtCaret()
        {
            int start = Mathf.Min(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition);
            int end = Mathf.Max(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition);

            if (start != end)
            {
                inputField.text = inputField.text.Remove(start, end - start);
                inputField.caretPosition = start;
            }
            else if (inputField.caretPosition < inputField.text.Length)
            {
                inputField.text = inputField.text.Remove(inputField.caretPosition, 1);
            }
        }
        #endregion

        #region Caret Movement
        private void MoveCaretLeft() => MoveCaret(-1);
        private void MoveCaretRight() => MoveCaret(1);
        private void MoveCaret(int direction)
        {
            int position = inputField.selectionStringAnchorPosition != inputField.selectionStringFocusPosition
                ? Mathf.Min(inputField.selectionStringAnchorPosition, inputField.selectionStringFocusPosition)
                : Mathf.Clamp(inputField.caretPosition + direction, 0, inputField.text.Length);

            inputField.caretPosition = position;
        }
        #endregion

        #region Keyboard Visibility
        private void SubmitText()
        {
            // Submit the text when the "Enter" key or the designated submit button is pressed
            // You can extend this to handle custom submit logic if needed (e.g., send data, close keyboard)
            if (inputField.TryGetComponent(out ManagedInputField input))
            {
                input.Deselected();
            }
        }
        public void ToggleKeyboardVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
        private void ResetKeyState()
        {
            isCapsLockOn = false;
            isShiftPressed = false;
            UpdateKeyLabels();
        }

        private void AddButtonListeners()
        {
            capsLockButton?.onClick.AddListener(ToggleCapsLock);
            shiftButton?.onClick.AddListener(() => ToggleShift());
            enterButton?.onClick.AddListener(SubmitText);
            backspaceButton?.onClick.AddListener(DeleteCharacter);
            deleteButton?.onClick.AddListener(DeleteCharacterAtCaret);
            spaceButton?.onClick.AddListener(InsertSpaceCharacter);
            leftArrowButton?.onClick.AddListener(MoveCaretLeft);
            rightArrowButton?.onClick.AddListener(MoveCaretRight);
        }
        private void RemoveButtonListeners()
        {
            capsLockButton?.onClick.RemoveAllListeners();
            shiftButton?.onClick.RemoveAllListeners();
            enterButton?.onClick.RemoveAllListeners();
            backspaceButton?.onClick.RemoveAllListeners();
            deleteButton?.onClick.RemoveAllListeners();
            spaceButton?.onClick.RemoveAllListeners();
            leftArrowButton?.onClick.RemoveAllListeners();
            rightArrowButton?.onClick.RemoveAllListeners();

            foreach (var key in keys)
                key.button.onClick.RemoveAllListeners();
        }
        #endregion
    }
}