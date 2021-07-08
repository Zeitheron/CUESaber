using HarmonyLib;
using static BeatmapSaveData;

namespace CUESaber
{
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("SendNoteWasCutEvent", MethodType.Normal)]
    internal static class SendNoteWasCutEvent
    {
        private static void Postfix(in NoteCutInfo noteCutInfo, BeatmapSaveData.NoteData ____noteData, NoteController __instance)
        {
            HarmonyHandler.OnNoteCut(noteCutInfo, ____noteData, __instance);
        }
    }

    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("OnDestroy", MethodType.Normal)]
    internal static class OnDestroy
    {
        private static void Postfix()
        {
            HarmonyHandler.OnControllerDestroy();
        }
    }
}