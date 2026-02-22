namespace Cozy.Builder.Game
{
    using Cozy.Builder.Messaging;
    using Cozy.Builder.Messaging.Messages;
    using UnityEngine;

    public static class Game
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Initialization logic here
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void PostInitialize()
        {
            Messenger.Publish(new EnterGameStateMessage { StateName = GameStates.Gameplay });
        }
    }
}