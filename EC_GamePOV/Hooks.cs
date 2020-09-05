using HarmonyLib;

using HPlay;

namespace EC_GamePOV
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(HPlayHPartMemberUI), "Start")]
        public static void HPlayHPartMemberUI_Start_CreateButton()
        {
            Tools.CreateButton();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(HPlayHPartMemberUI), "SetChara")]
        public static void HPlayHPartMemberUI_SetChara_Patch(ChaControl _chara)
        {
            EC_GamePOV.uiCharacter = _chara;
            
            Tools.UpdateButton();
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(HPlayScene), "SceneEndProc")]
        public static void HPlayScene_SceneEndProc_Patch()
        {
            if(EC_GamePOV.povEnabled)
                EC_GamePOV.TogglePOV();

            EC_GamePOV.povCharacter = null;
            EC_GamePOV.uiCharacter = null;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
        private static bool CameraControl_Ver2_LateUpdate_Patch(CameraControl_Ver2 __instance)
        {
            if (!EC_GamePOV.povEnabled) 
                return true;
            
            Traverse.Create(__instance).Method("CameraUpdate").GetValue();
            
            return false;
        }
    }
}