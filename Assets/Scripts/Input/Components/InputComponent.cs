namespace Cozy.Builder.Input.Components
{
    using Cozy.Builder.Messaging;
    using Cozy.Builder.Messaging.Messages;
    using UnityEngine;

    public class InputComponent : MonoBehaviour
    {
        private static InputSystem_Actions _inputActions;

        [SerializeField]
        private float moveThreshold = 0.1f;

        private InputMessage message = new();

        private void Awake()
        {
            _inputActions ??= new InputSystem_Actions();
            Clear();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            var touched = _inputActions.Player.Touch.IsPressed();
            var position = _inputActions.Player.Position.ReadValue<Vector2>();

            if (touched)
            {
                message.IsTouching = true;

                if (message.DownPosition == null)
                {
                    message.DownPosition = position;
                    Messenger.Publish(message);

                    return;
                }

                var delta = position - message.DownPosition.Value;

                if (TryMove(position, delta, out Vector2 moveDelta))
                {
                    message.MoveDelta = moveDelta;
                    Messenger.Publish(message);
                }
            }
            else
            {
                var wasTouching = message.IsTouching;

                Clear();

                if (wasTouching)
                {
                    message.IsTouching = false;
                    Messenger.Publish(message);
                }
            }
        }
    
        private void Clear()
        {
            message.DownPosition = null;
            message.MovePosition = null;
            message.IsTouching = false;
            message.IsMoving = false;
            message.MoveDelta = Vector2.zero;
        }

        private bool TryMove(Vector2 position, Vector2 rawDelta, out Vector2 moveDelta)
        {
            moveDelta = Vector2.zero;

            message.IsMoving = message.IsMoving || rawDelta.magnitude >= moveThreshold;

            if (!message.IsMoving)
            {
                return false;
            }

            if (message.MovePosition == null)
            {
                message.MovePosition = position;

                return true;
            }

            moveDelta = position - message.MovePosition.Value;
            message.MovePosition = position;

            return true;
        }
    }
}