using System.Linq;
using HarmonyLib;
using PieceManager;
using UnityEngine;
using UnityEngine.Audio;

namespace OdinsHollow
{
    /// <summary>
    /// Fixes Odin's Hollow sounds (gateway/teleport, torches, spawner effects and any other AudioSources in the
    /// bundle) playing at full volume regardless of the game's volume sliders.
    ///
    /// AudioSources inside an asset bundle are authored in Unity against a mixer group that only exists in the Unity
    /// project, so in game their output mixer group is null or dangling. A source with no valid group skips
    /// Valheim's mixer entirely, which means the master and SFX volume sliders never touch it. Once AudioMan is up we
    /// point every AudioSource in the odinshollow bundle at Valheim's own "SFX" group. The bundle's prefabs are
    /// shared, so everything spawned from them later inherits it.
    ///
    /// AudioMan is a DontDestroyOnLoad singleton that wakes once in the start scene, so this only works because
    /// Harmony.PatchAll runs in the plugin's Awake.
    /// </summary>
    public static class OdinsHollowAudioMixerFix
    {
        [HarmonyPatch(typeof(AudioMan), "Awake")]
        private static class AudioManAwakePatch
        {
            [HarmonyPostfix]
            private static void Postfix(AudioMan __instance)
            {
                if (__instance == null || __instance.m_masterMixer == null)
                {
                    return;
                }

                AudioMixerGroup[] groups = __instance.m_masterMixer.FindMatchingGroups("SFX");
                AudioMixerGroup? sfx = groups.FirstOrDefault(g => g.name == "SFX") ?? groups.FirstOrDefault();
                if (sfx == null)
                {
                    return;
                }

                AssetBundle bundle = PiecePrefabManager.RegisterAssetBundle("odinshollow");
                if (bundle == null)
                {
                    return;
                }

                foreach (GameObject prefab in bundle.LoadAllAssets<GameObject>())
                {
                    foreach (AudioSource source in prefab.GetComponentsInChildren<AudioSource>(true))
                    {
                        source.outputAudioMixerGroup = sfx;
                    }
                }
            }
        }
    }
}
