using System.Collections;

using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;

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

        private static EyeObject[] eyes;
        
        public static CameraControl_Ver2 cc;
        private static BaseCameraControl_Ver2.CameraData backupData;
        
        private static Vector3 viewRotation;
        
        private static float backupFov;

        private static ConfigEntry<bool> hideHead { get; set; }
        private static ConfigEntry<float> fov { get; set; }
        private static ConfigEntry<float> sensitivity { get; set; }
        
        private void Awake()
        {
            povEnabled = false;

            sensitivity = Config.Bind(new ConfigDefinition("General", "Mouse sensitivity"), 2f);
            (fov = Config.Bind(new ConfigDefinition("General", "FOV"), 75f, new ConfigDescription("POV field of view", new AcceptableValueRange<float>(1f, 180f)))).SettingChanged += delegate
            {
                if (!povEnabled || cc == null)
                    return;

                cc.CameraFov = fov.Value;
            };

            Harmony.CreateAndPatchAll(typeof(Hooks), "EC_GamePOV");
        }

        private void Update()
        {
            if (!povEnabled)
                return;
            
            if (Input.GetKey(KeyCode.Mouse0))
            {
                var x = Input.GetAxis("Mouse X") * sensitivity.Value;
                var y = -Input.GetAxis("Mouse Y") * sensitivity.Value;
                
                viewRotation += new Vector3(y, x, 0f);
            }
            
            StartCoroutine(ApplyPOV());
        }

        private static IEnumerator ApplyPOV()
        {
            yield return new WaitForEndOfFrame();

            povCharacter.neckLookCtrl.neckLookScript.aBones[0].neckBone.Rotate(viewRotation);
            
            cc.TargetPos = Vector3.Lerp(eyes[0].eyeTransform.position, eyes[1].eyeTransform.position, 0.5f);
            cc.CameraAngle = eyes[0].eyeTransform.eulerAngles;
        }

        public static void TogglePOV()
        {
            if (uiCharacter == null || povCharacter == uiCharacter)
            {
                StopPOV();
            }
            else
            {
                if(povCharacter != null)
                    StopPOV();
                
                povEnabled = true;
                povCharacter = uiCharacter;
                
                eyes = povCharacter.eyeLookCtrl.eyeLookScript.eyeObjs;

                var data = cc.GetCameraData();
                
                backupData = data;
                backupFov = cc.CameraFov;
            
                cc.CameraDir = Vector3.zero;
                viewRotation = Vector3.zero;

                cc.CameraFov = fov.Value;
            }

            Tools.UpdateButton();
        }
        
        private static void StopPOV()
        {
            povEnabled = false;
            povCharacter = null;

            if (cc == null) 
                return;
            
            cc.SetCameraData(backupData);
            cc.CameraFov = backupFov;
        }
    }
}