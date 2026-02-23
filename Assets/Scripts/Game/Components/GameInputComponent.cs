namespace Cozy.Builder.Game.Components
{
    using Cozy.Builder.Game.Components.InputStates;
    using Cozy.Builder.Messaging;
    using Cozy.Builder.Messaging.Messages;
    using Cozy.Builder.Utility;
    using Cozy.Builder.Utility.Components;
    using UnityEngine;

    public class GameInputComponent : MonoStateMachine<GameInputComponent>,
        IMessageReceiver<EnterGameStateMessage>
    {
        [SerializeField]
        private LayerMask inputMask;

        public LayerMask InputMask => inputMask;

        [SerializeField]
        private GameObject gameCamera;

        public GameObject GameCamera => gameCamera;

        [SerializeField]
        private VolumeComponent bounds;

        public VolumeComponent Bounds => bounds;
        
        private void OnEnable()
        {
            Messenger.Subscribe(this);
        }

        private void OnDisable()
        {
            Messenger.Unsubscribe(this);
        }

        public void OnMessageReceived(EnterGameStateMessage message)
        {
            switch (message.StateName)
            {
                case GameStates.GameplayDefault:
                    EnterState<GameplayDefaultState>(this);
                    break;
                case GameStates.GameplayPlacement:
                    EnterState<GameplayPlacementState>(this);
                    break;
                default:
                    ExitState(this);
                    break;
            }
        }
    }
}