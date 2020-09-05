using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace EC_GamePOV
{
    public static class Tools
    {
        private static TextMeshProUGUI btnText;

        public static void CreateButton()
        {
            EC_GamePOV.cc = Singleton<CameraControl_Ver2>.Instance;
            
            var UI = GameObject.Find("UI");
            var orig = UI.transform.Find("System/Canvas/System/ClothMenu/btnMenu2");

            var copy = Object.Instantiate(orig, orig.parent);
            copy.name = "btnPOV";

            var btn = copy.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(EC_GamePOV.TogglePOV);

            btnText = copy.GetComponentInChildren<TextMeshProUGUI>();
            
            UpdateButton();
        }

        public static void UpdateButton()
        {
            if (btnText == null)
                return;
            
            var text = "Enable POV";

            if (EC_GamePOV.povEnabled)
                text = EC_GamePOV.povCharacter == EC_GamePOV.uiCharacter ? "Disable POV" : "Switch POV";

            btnText.text = text;
        }
    }
}