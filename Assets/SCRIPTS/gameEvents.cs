using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnGhostKilled;
    public static event Action OnPlayerWin;
    public static event Action OnPlayerLose;

    public static void GhostKilled()
    {
        Debug.Log($"[GameEvents] GhostKilled — listener attivi: {OnGhostKilled?.GetInvocationList().Length ?? 0}");
        OnGhostKilled?.Invoke();
    }

    public static void PlayerWin()   => OnPlayerWin?.Invoke();
    public static void PlayerLose()  => OnPlayerLose?.Invoke();
}