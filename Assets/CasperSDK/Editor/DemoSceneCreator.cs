using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace CasperSDK.Editor
{
    /// <summary>
    /// Editor script to create a complete demo scene with proper UI layout
    /// </summary>
    public class DemoSceneCreator : EditorWindow
    {
        [MenuItem("Window/Casper SDK/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Set camera background
            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f); // Dark blue-gray
                camera.clearFlags = CameraClearFlags.SolidColor;
                
                // Add URP camera data if using Universal Render Pipeline
                var urpCameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                if (urpCameraDataType != null && camera.GetComponent(urpCameraDataType) == null)
                {
                    camera.gameObject.AddComponent(urpCameraDataType);
                }
            }

            // Create Canvas
            var canvasGO = CreateCanvas();
            
            // Create main container with vertical layout
            var mainContainer = CreateMainContainer(canvasGO.transform);
            
            // Create Header
            CreateHeader(mainContainer.transform);
            
            // Create Content Area
            var contentArea = CreateContentArea(mainContainer.transform);
            
            // Create Wallet Panel
            CreateWalletPanel(contentArea.transform);
            
            // Create Action Buttons
            CreateActionButtons(contentArea.transform);
            
            // Create Transfer Panel
            CreateTransferPanel(contentArea.transform);
            
            // Create Status Panel
            CreateStatusPanel(contentArea.transform);
            
            // Create Demo Controller
            CreateDemoController(canvasGO);
            
            // Save scene
            var scenePath = "Assets/CasperSDK/Samples/CasperWalletDemo.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"[CasperSDK] Demo scene created at: {scenePath}");
            EditorUtility.DisplayDialog("Demo Scene Created", 
                $"Scene saved to:\n{scenePath}\n\nPress Play to test!", "OK");
        }

        private static GameObject CreateCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Add EventSystem if needed
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Try to use new Input System UI module if available, otherwise fallback to legacy
                var inputSystemType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputSystemType != null)
                {
                    eventSystem.AddComponent(inputSystemType);
                }
                else
                {
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            
            return canvasGO;
        }

        private static GameObject CreateMainContainer(Transform parent)
        {
            var container = CreatePanel("MainContainer", parent, new Color(0, 0, 0, 0));
            
            // Full screen with padding
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(40, 40);
            rect.offsetMax = new Vector2(-40, -40);
            
            // Vertical layout
            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return container;
        }

        private static void CreateHeader(Transform parent)
        {
            var header = CreatePanel("Header", parent, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            var headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 100;
            headerLayout.flexibleWidth = 1;
            
            // Add rounded corners effect (border)
            var headerRect = header.GetComponent<RectTransform>();
            
            // Title
            var titleGO = CreateTMPText("Title", header.transform, "CASPER WALLET", 36, Color.white);
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(1, 0.5f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(0, 50);
            
            var titleText = titleGO.GetComponent<TMP_Text>();
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
        }

        private static GameObject CreateContentArea(Transform parent)
        {
            var content = CreatePanel("ContentArea", parent, new Color(0, 0, 0, 0));
            
            var layout = content.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            
            var layoutElement = content.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1;
            layoutElement.flexibleWidth = 1;
            
            return content;
        }

        private static void CreateWalletPanel(Transform parent)
        {
            var panel = CreatePanel("WalletPanel", parent, new Color(0.12f, 0.12f, 0.18f, 0.95f));
            var panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.flexibleWidth = 1;
            panelLayout.minWidth = 400;
            
            var vertLayout = panel.AddComponent<VerticalLayoutGroup>();
            vertLayout.spacing = 15;
            vertLayout.padding = new RectOffset(25, 25, 25, 25);
            vertLayout.childAlignment = TextAnchor.UpperCenter;
            vertLayout.childControlWidth = true;
            vertLayout.childControlHeight = false;
            vertLayout.childForceExpandWidth = true;
            
            // Panel Title
            var title = CreateTMPText("PanelTitle", panel.transform, "ACCOUNT", 24, new Color(0.7f, 0.7f, 0.8f));
            title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            title.AddComponent<LayoutElement>().preferredHeight = 40;
            
            // Balance Display
            var balanceContainer = CreatePanel("BalanceContainer", panel.transform, new Color(0.08f, 0.08f, 0.12f, 1f));
            balanceContainer.AddComponent<LayoutElement>().preferredHeight = 120;
            
            var balanceLayout = balanceContainer.AddComponent<VerticalLayoutGroup>();
            balanceLayout.spacing = 5;
            balanceLayout.padding = new RectOffset(20, 20, 20, 20);
            balanceLayout.childAlignment = TextAnchor.MiddleCenter;
            balanceLayout.childControlWidth = true;
            balanceLayout.childControlHeight = true;
            
            var balanceLabel = CreateTMPText("BalanceLabel", balanceContainer.transform, "Balance", 16, new Color(0.5f, 0.5f, 0.6f));
            balanceLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            balanceLabel.AddComponent<LayoutElement>().preferredHeight = 25;
            
            var balanceValue = CreateTMPText("BalanceValue", balanceContainer.transform, "0.00 CSPR", 42, new Color(0.4f, 0.9f, 0.6f));
            balanceValue.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            balanceValue.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            balanceValue.AddComponent<LayoutElement>().preferredHeight = 55;
            
            // Address Display
            var addressContainer = CreatePanel("AddressContainer", panel.transform, new Color(0.08f, 0.08f, 0.12f, 1f));
            addressContainer.AddComponent<LayoutElement>().preferredHeight = 80;
            
            var addressLayout = addressContainer.AddComponent<VerticalLayoutGroup>();
            addressLayout.spacing = 5;
            addressLayout.padding = new RectOffset(15, 15, 15, 15);
            addressLayout.childAlignment = TextAnchor.MiddleCenter;
            addressLayout.childControlWidth = true;
            addressLayout.childControlHeight = true;
            
            var addressLabel = CreateTMPText("AddressLabel", addressContainer.transform, "Public Key (enter to check balance)", 14, new Color(0.5f, 0.5f, 0.6f));
            addressLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            addressLabel.AddComponent<LayoutElement>().preferredHeight = 20;
            
            // Input field for address - allows checking balance of any address
            CreateInputField("AddressInput", addressContainer.transform, "01abc...or paste address", 35);
        }

        private static void CreateActionButtons(Transform parent)
        {
            var panel = CreatePanel("ActionsPanel", parent, new Color(0.12f, 0.12f, 0.18f, 0.95f));
            var panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.preferredWidth = 200;
            
            var vertLayout = panel.AddComponent<VerticalLayoutGroup>();
            vertLayout.spacing = 12;
            vertLayout.padding = new RectOffset(20, 20, 25, 25);
            vertLayout.childAlignment = TextAnchor.UpperCenter;
            vertLayout.childControlWidth = true;
            vertLayout.childControlHeight = false;
            vertLayout.childForceExpandWidth = true;
            
            // Title
            var title = CreateTMPText("ActionsTitle", panel.transform, "ACTIONS", 20, new Color(0.7f, 0.7f, 0.8f));
            title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            title.AddComponent<LayoutElement>().preferredHeight = 35;
            
            // Buttons
            CreateButton("GenerateBtn", panel.transform, "Generate Account", new Color(0.3f, 0.5f, 0.9f), 45);
            CreateButton("ImportBtn", panel.transform, "Import Keys", new Color(0.6f, 0.3f, 0.7f), 45);
            CreateButton("RefreshBtn", panel.transform, "Refresh Balance", new Color(0.2f, 0.6f, 0.8f), 45);
            CreateButton("CopyBtn", panel.transform, "Copy Address", new Color(0.4f, 0.4f, 0.5f), 45);
            CreateButton("ExportBtn", panel.transform, "Export Keys", new Color(0.5f, 0.4f, 0.7f), 45);
            CreateButton("FaucetBtn", panel.transform, "Open Faucet", new Color(0.9f, 0.6f, 0.2f), 45);
        }

        private static void CreateTransferPanel(Transform parent)
        {
            var panel = CreatePanel("TransferPanel", parent, new Color(0.12f, 0.12f, 0.18f, 0.95f));
            var panelLayout = panel.AddComponent<LayoutElement>();
            panelLayout.flexibleWidth = 1;
            panelLayout.minWidth = 350;
            
            var vertLayout = panel.AddComponent<VerticalLayoutGroup>();
            vertLayout.spacing = 15;
            vertLayout.padding = new RectOffset(25, 25, 25, 25);
            vertLayout.childAlignment = TextAnchor.UpperCenter;
            vertLayout.childControlWidth = true;
            vertLayout.childControlHeight = false;
            vertLayout.childForceExpandWidth = true;
            
            // Title
            var title = CreateTMPText("TransferTitle", panel.transform, "SEND CSPR", 24, new Color(0.7f, 0.7f, 0.8f));
            title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            title.AddComponent<LayoutElement>().preferredHeight = 40;
            
            // Recipient Input
            CreateInputField("RecipientInput", panel.transform, "Recipient Public Key", 50);
            
            // Amount Input
            CreateInputField("AmountInput", panel.transform, "Amount (CSPR)", 50);
            
            // Send Button
            CreateButton("SendBtn", panel.transform, "SEND TRANSACTION", new Color(0.2f, 0.7f, 0.4f), 55);
            
            // Status
            var status = CreateTMPText("TransferStatus", panel.transform, "", 14, new Color(0.6f, 0.6f, 0.7f));
            status.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            status.AddComponent<LayoutElement>().preferredHeight = 30;
        }

        private static void CreateStatusPanel(Transform parent)
        {
            // This will be at the bottom as a log area
        }

        private static void CreateDemoController(GameObject canvas)
        {
            var controller = new GameObject("DemoController");
            
            // Use reflection to add the controller component (Editor can't directly reference Runtime scripts)
            var controllerType = System.Type.GetType("CasperSDK.Samples.CasperWalletDemoController, Assembly-CSharp");
            if (controllerType != null)
            {
                controller.AddComponent(controllerType);
                Debug.Log("[CasperSDK] CasperWalletDemoController added successfully");
            }
            else
            {
                Debug.LogWarning("[CasperSDK] Add CasperWalletDemoController component manually to DemoController");
            }
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            var rect = panel.AddComponent<RectTransform>();
            
            var image = panel.AddComponent<Image>();
            image.color = color;
            
            return panel;
        }

        private static GameObject CreateTMPText(string name, Transform parent, string text, int fontSize, Color color)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            var rect = textGO.AddComponent<RectTransform>();
            
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            
            return textGO;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Color bgColor, float height)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            
            var rect = btnGO.AddComponent<RectTransform>();
            
            var image = btnGO.AddComponent<Image>();
            image.color = bgColor;
            
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = image;
            
            // Hover colors
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f, 1f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f, 1f);
            btn.colors = colors;
            
            var layout = btnGO.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            
            // Button text
            var textGO = CreateTMPText("Text", btnGO.transform, text, 16, Color.white);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textGO.GetComponent<TMP_Text>().fontStyle = FontStyles.Bold;
            
            return btnGO;
        }

        private static GameObject CreateInputField(string name, Transform parent, string placeholder, float height)
        {
            var inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);
            
            var rect = inputGO.AddComponent<RectTransform>();
            
            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.06f, 0.06f, 0.1f, 1f);
            
            var input = inputGO.AddComponent<TMP_InputField>();
            
            var layout = inputGO.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            
            // Text Area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGO.transform, false);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(15, 5);
            textAreaRect.offsetMax = new Vector2(-15, -5);
            textArea.AddComponent<RectMask2D>();
            
            // Placeholder
            var placeholderGO = CreateTMPText("Placeholder", textArea.transform, placeholder, 16, new Color(0.4f, 0.4f, 0.5f));
            var placeholderRect = placeholderGO.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            placeholderGO.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;
            
            // Input Text
            var inputText = CreateTMPText("Text", textArea.transform, "", 16, Color.white);
            var inputTextRect = inputText.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;
            inputText.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;
            
            input.textViewport = textAreaRect;
            input.textComponent = inputText.GetComponent<TMP_Text>();
            input.placeholder = placeholderGO.GetComponent<TMP_Text>();
            
            return inputGO;
        }
    }
}
