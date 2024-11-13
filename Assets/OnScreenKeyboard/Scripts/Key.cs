using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrabdeepDhaliwal.OnScreenKeyboard
{
    public class Key : MonoBehaviour
    {
        #region Fields
        [HideInInspector]
        public Button button;                  // Button for this key
        [HideInInspector]
        public TextMeshProUGUI primaryKeyText; // Text for the primary character (e.g., lowercase)
        [HideInInspector]
        public TextMeshProUGUI secondaryKeyText; // Text for the secondary character (e.g., uppercase or alternative)
        public string primaryValue;            // Primary character value (e.g., 'a', '1')
        [SerializeField]
        private Color primaryTextColor;        // Text color for the primary key text
        public string secondaryValue;          // Secondary character value (e.g., 'A', '!', etc.)
        [SerializeField]
        private Color secondaryTextColor;      // Text color for the secondary key text

        // Array of special key names that do not follow the normal primary/secondary text behavior
        private static readonly string[] specialKeys = new string[]
        {
            "Back", "Enter", "Delete", "Caps", "Space", ">", "<", "Shift"
        };
        #endregion

        #region Initialization
        public void Setup()
        { 
            // Assign the button if it isn't already set through the inspector
            if (button == null)
                button = GetComponent<Button>();

            // Get the TextMeshProUGUI components for primary and secondary text from the button's children
            TextMeshProUGUI[] texts = button.GetComponentsInChildren<TextMeshProUGUI>();

            // Assign the primary and secondary key text based on available TextMeshProUGUI components
            if (texts.Length >= 2)
            {
                primaryKeyText = texts[0];
                secondaryKeyText = texts[1];
            }
            else if (texts.Length == 1)
            {
                primaryKeyText = texts[0];
                // Create secondary text only if it's not a special key
                if (!IsSpecialKey(primaryValue))
                {
                    secondaryKeyText = CreateSecondaryText();
                }
            }
            else
            {
                primaryKeyText = CreatePrimaryText();
                // Create secondary text only if it's not a special key
                if (!IsSpecialKey(primaryValue))
                {
                    secondaryKeyText = CreateSecondaryText();
                }
            }

            // Set the initial text values and hide secondary text by default
            UpdateText();
        }

        private TextMeshProUGUI CreatePrimaryText()
        {
            GameObject primaryTextObj = new GameObject("PrimaryText", typeof(TextMeshProUGUI));
            primaryTextObj.transform.SetParent(button.transform);
            TextMeshProUGUI textComponent = primaryTextObj.GetComponent<TextMeshProUGUI>();
            textComponent.alignment = TextAlignmentOptions.Center; 
            return textComponent;
        }
        private TextMeshProUGUI CreateSecondaryText()
        {
            GameObject secondaryTextObj = new GameObject("SecondaryText", typeof(TextMeshProUGUI));
            secondaryTextObj.transform.SetParent(button.transform);
            TextMeshProUGUI textComponent = secondaryTextObj.GetComponent<TextMeshProUGUI>();
            textComponent.alignment = TextAlignmentOptions.Center; // Align secondary text top-left
                                                                   
            // Set the scale, position, width, and height
            secondaryTextObj.transform.localScale = Vector3.one; 
            secondaryTextObj.transform.localPosition = new Vector3(5, -5, 0); 

            // Set the width and height 
            RectTransform rectTransform = secondaryTextObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(25, 25); 

            return textComponent;
        }
        #endregion

        #region Text Update
        // Updates the primary and secondary key texts based on current key values and visibility rules
        public void UpdateText()
        {
            // Set the opacity of the text to full visibility
            primaryTextColor.a = 1.0f;
            secondaryTextColor.a = 1.0f;

            if (primaryKeyText != null)
            {
                // Set the primary key's text and styling
                primaryKeyText.text = primaryValue;
                primaryKeyText.color = primaryTextColor;
                primaryKeyText.fontSize = 36;  // Font size for the primary text
                primaryKeyText.alignment = TextAlignmentOptions.Center; // Center align
            }

            if (secondaryKeyText != null)
            {
                // Hide the secondary text for letter keys and special keys
                bool shouldHideSecondary = IsLetter(primaryValue) || IsSpecialKey(primaryValue);
                secondaryKeyText.gameObject.SetActive(!shouldHideSecondary); // Toggle visibility of secondary text

                if (!shouldHideSecondary)
                {
                    // Update the secondary key's text and styling if not hidden
                    secondaryKeyText.text = secondaryValue;
                    secondaryKeyText.color = secondaryTextColor;
                    secondaryKeyText.fontSize = 24;  // Font size for the secondary text
                    secondaryKeyText.alignment = TextAlignmentOptions.TopLeft; // Align top-left

                    // Position the secondary key text
                    RectTransform rectTransform = secondaryKeyText.GetComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(0, 1); // Anchor at top-left
                    rectTransform.anchorMax = new Vector2(0, 1); // Anchor at top-left
                    rectTransform.pivot = new Vector2(0, 1);     // Pivot at top-left
                    rectTransform.anchoredPosition = new Vector2(10, -5); // Offset position
                    rectTransform.sizeDelta = new Vector2(0, 0); // No extra size
                }
            }
        }
        #endregion

        #region Helper Methods
        // Checks if the given string is a single letter (a-z or A-Z)
        private bool IsLetter(string value)
        {
            return value.Length == 1 && char.IsLetter(value[0]);
        }

        // Checks if the given key value is a special key (such as Shift, Enter, Back, etc.)
        private bool IsSpecialKey(string value)
        {
            return System.Array.Exists(specialKeys, key => key.Equals(value, System.StringComparison.OrdinalIgnoreCase));
        }
        #endregion
    }
}
