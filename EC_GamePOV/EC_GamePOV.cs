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
        private static Transform eyeCenter;
        private static GameObject head;
        
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
            (hideHead = Config.Bind(new ConfigDefinition("General", "Hide head"), true)).SettingChanged += delegate
            {
                if (!povEnabled || head == null)
                    return;

                head.SetActive(!hideHead.Value);
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

            if (povCharacter == null)
                yield break;
            
            povCharacter.neckLookCtrl.neckLookScript.aBones[0].neckBone.Rotate(viewRotation);

            cc.TargetPos = Vector3.Lerp(eyes[0].eyeTransform.position, eyes[1].eyeTransform.position, 0.5f);
            cc.CameraAngle = eyeCenter.eulerAngles;
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
                eyeCenter = povCharacter.transform.Find("BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_n_spine01/cf_j_spine01/cf_n_spine02/cf_j_spine02/cf_n_spine03/cf_j_spine03/cf_n_neck/cf_j_neck/cf_n_head/cf_j_head/cf_s_head/p_cf_head_bone/cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
                head = povCharacter.objHeadBone;

                if(hideHead.Value)
                    head.SetActive(false);
                
                var data = cc.GetCameraData();
                
                backupData = data;
                backupFov = cc.CameraFov;
            
                cc.CameraDir = Vector3.zero;
                viewRotation = Vector3.zero;

                cc.CameraFov = fov.Value;
            }

            Tools.UpdateButton();
        }
        
        public static void StopPOV()
        {
            povEnabled = false;
            povCharacter = null;
                
            if (cc != null)
            {
                cc.SetCameraData(backupData);
                cc.CameraFov = backupFov;
            }

            if(head != null)
                head.SetActive(true);

            head = null;
            eyes = null;
        }
    }
}