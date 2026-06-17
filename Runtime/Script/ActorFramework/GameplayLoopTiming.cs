namespace Ayla
{
#nullable enable

    public enum GameplayLoopTiming
    {
        None = 0,

        PreUpdate,
        DuringUpdate,
        PostUpdate,
        BehaviourUpdate,

        BeforePhysicsUpdate,
        // Animator Update
        // Physics Update
        AfterPhysicsUpdate,

        PreLateUpdate,
        DuringLateUpdate,
        PostLateUpdate
    }
}
