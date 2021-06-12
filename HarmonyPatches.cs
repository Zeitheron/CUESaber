using HarmonyLib;
using static BeatmapSaveData;

namespace CUESaber
{
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("SendNoteWasCutEvent", MethodType.Normal)]
    internal static class SendNoteWasCutEvent
    {
        private static void Postfix(in NoteCutInfo noteCutInfo, NoteData ____noteData, NoteController __instance)
        {
            CUEHandler.OnNoteCut(noteCutInfo, ____noteData, __instance);
        }
    }

    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("OnDestroy", MethodType.Normal)]
    internal static class OnDestroy
    {
        private static void Postfix()
        {
            CUEHandler.OnControllerDestroy();
        }
    }
}