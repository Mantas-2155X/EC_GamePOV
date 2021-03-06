using System.Collections;

using HarmonyLib;

using HEdit;
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HPlayScene), "GameEnd")]
        [HarmonyPatch(typeof(HPlayScene), "SceneEnd")]
        [HarmonyPatch(typeof(HPlayScene), "OnDestroy")]
        [HarmonyPatch(typeof(HPlayScene), "SceneEndProc")]
        public static void HPlayScene_StopPOV()
        {
            if(EC_GamePOV.povEnabled)
                EC_GamePOV.StopPOV();
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(HPlayScene), "InitPart")]
        public static void HPlayScene_InitPart_Prefix(string _UID, ref ChaControl __state)
        {
            if (!EC_GamePOV.povEnabled || EC_GamePOV.povCharacter == null) 
                return;

            __state = Singleton<HEditData>.Instance.nodes[_UID].kind == 0 ? EC_GamePOV.povCharacter : null;
            
            EC_GamePOV.StopPOV();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(HPlayScene), "InitPart")]
        public static void HPlayScene_InitPart_Postfix(ref object __result, string _UID, ChaControl __state)
        {
            __result = new[] { __result, StartPOV() }.GetEnumerator();

            IEnumerator StartPOV()
            {
                if (__state == null || !__state.visibleAll || Singleton<HEditData>.Instance.nodes[_UID].kind != 0) 
                    yield break;
            
                EC_GamePOV.uiCharacter = __state;
                EC_GamePOV.TogglePOV();
            }
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