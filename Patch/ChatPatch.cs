using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChatSwitch.Plugin;
using static Groups.ChatCommands;

namespace ChatSwitch;

[HarmonyPatch]
internal class ChatPatch
{
    [HarmonyPatch(typeof(Chat), nameof(Chat.InputText)), HarmonyPrefix]
    public static bool ChatAddSMode(Chat __instance)
    {
        if (__instance.m_input.text != "/s") return true;
        
        inSMode = !inSMode;
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, inSMode == true ? "In main chat" : "Out main chat");
        return false;
    }    [HarmonyPatch(typeof(Chat), nameof(Chat.InputText)), HarmonyPrefix]
    public static bool ChatAddWMode(Chat __instance)
    {
        if (__instance.m_input.text != "/w") return true;
        
        inWMode = !inWMode;
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, inWMode == true ? "In wisper chat" : "Out wisper chat");
        return false;
    }
    [HarmonyPatch(typeof(Chat), nameof(Chat.InputText)), HarmonyPrefix]
    public static void CheckForMode(Chat __instance)
    {
        if (inSMode && !__instance.m_input.text.StartsWith("/s"))
        {
            __instance.m_input.text = "/s " + __instance.m_input.text ;
            return;
        }
        if (inWMode)
        {
            __instance.m_input.text = "/w " + __instance.m_input.text ;
            return;
        }
        
    }
}