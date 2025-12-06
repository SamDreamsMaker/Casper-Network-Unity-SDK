#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace CasperSDK.Editor
{
    /// <summary>
    /// Editor utility to upgrade legacy UI Text and InputField to TextMeshPro.
    /// Menu: Casper SDK > Upgrade Scene to TextMeshPro
    /// </summary>
    public static class TMPUpgrader
    {
        [MenuItem("Casper SDK/Upgrade Scene to TextMeshPro")]
        public static void UpgradeScene()
        {
            int textUpgraded = 0;
            int inputUpgraded = 0;

            // Find all Text components
            var textComponents = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            foreach (var text in textComponents)
            {
                UpgradeText(text);
                textUpgraded++;
            }

            // Find all InputField components  
            var inputFields = Object.FindObjectsByType<InputField>(FindObjectsSortMode.None);
            foreach (var input in inputFields)
            {
                UpgradeInputField(input);
                inputUpgraded++;
            }

            Debug.Log($"[TMP Upgrader] Upgraded {textUpgraded} Text components and {inputUpgraded} InputField components to TextMeshPro");
            EditorUtility.DisplayDialog("TMP Upgrade Complete", 
                $"Upgraded:\n- {textUpgraded} Text → TMP_Text\n- {inputUpgraded} InputField → TMP_InputField", "OK");
        }

        private static void UpgradeText(Text text)
        {
            var go = text.gameObject;
            
            // Store text properties
            string content = text.text;
            int fontSize = text.fontSize;
            Color color = text.color;
            TextAnchor alignment = text.alignment;
            FontStyle fontStyle = text.fontStyle;
            
            // Remove old component
            Object.DestroyImmediate(text);
            
            // Add TMP component
            var tmpText = go.AddComponent<TextMeshProUGUI>();
            tmpText.text = content;
            tmpText.fontSize = fontSize;
            tmpText.color = color;
            
            // Convert alignment
            tmpText.alignment = ConvertAlignment(alignment);
            
            // Convert font style
            if ((fontStyle & FontStyle.Bold) != 0) tmpText.fontStyle |= FontStyles.Bold;
            if ((fontStyle & FontStyle.Italic) != 0) tmpText.fontStyle |= FontStyles.Italic;
            
            EditorUtility.SetDirty(go);
        }

        private static void UpgradeInputField(InputField input)
        {
            var go = input.gameObject;
            
            // Store properties
            string text = input.text;
            string placeholder = input.placeholder != null ? ((Text)input.placeholder).text : "";
            
            // Get references before destroying
            var targetGraphic = input.targetGraphic;
            
            // Remove old component
            Object.DestroyImmediate(input);
            
            // Add TMP component
            var tmpInput = go.AddComponent<TMP_InputField>();
            tmpInput.text = text;
            
            // Set up placeholder
            if (!string.IsNullOrEmpty(placeholder))
            {
                var placeholderObj = new GameObject("Placeholder");
                placeholderObj.transform.SetParent(tmpInput.transform);
                var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
                placeholderText.text = placeholder;
                placeholderText.fontStyle = FontStyles.Italic;
                placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                tmpInput.placeholder = placeholderText;
            }
            
            // Set up text component
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(tmpInput.transform);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "";
            tmpInput.textComponent = textComponent;
            
            EditorUtility.SetDirty(go);
        }

        private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
#endif
