using BepInEx;

using HarmonyLib;

namespace EC_GamePOV
{
    [BepInProcess("EmotionCreators")]
    [BepInPlugin(nameof(EC_GamePOV), nameof(EC_GamePOV), VERSION)]
    public class EC_GamePOV : BaseUnityPlugin
    {
        public const string VERSION = "1.0.1";

        public static ChaControl povCharacter;
        public static ChaControl uiCharacter;

        public static bool povEnabled;

        private void Awake()
        {
            povEnabled = false;

            Harmony.CreateAndPatchAll(typeof(Hooks), "EC_GamePOV");
        }

        public static void TogglePOV()
        {
            if (uiCharacter == null)
            {
                povEnabled = false;
                povCharacter = null;
                
                return;
            }

            if (povCharacter == uiCharacter)
            {
                povEnabled = false;
                povCharacter = null;
            }
            else
            {
                if(povCharacter == null)
                    povEnabled = true;
                
                povCharacter = uiCharacter;
            }

            Tools.UpdateButton();
        }
    }
}