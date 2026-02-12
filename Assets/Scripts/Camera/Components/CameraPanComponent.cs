namespace Cozy.Builder.Camera
{
    using Cozy.Builder.Messaging;
    using Cozy.Builder.Messaging.Messages;
    using UnityEngine;

    public class CameraPanComponent : MonoBehaviour,
        IMessageReceiver<InputMessage>
    {
        [SerializeField]
        private LayerMask inputMask;

        private Vector2? inputPosition;
        private Vector2 lastInputPosition;

        private bool shouldPan = false;

        private void OnEnable()
        {
            Messenger.Subscribe(this);
        }

        private void OnDisable()
        {
            Messenger.Unsubscribe(this);
        }

        private void LateUpdate()
        {
            
        }

        public void OnMessageReceived(InputMessage message)
        {
            shouldPan = message.IsMoving;
            inputPosition = message.MovePosition;
        }
    }
}