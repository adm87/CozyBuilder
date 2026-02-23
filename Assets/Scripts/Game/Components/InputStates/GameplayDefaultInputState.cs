namespace Cozy.Builder.Game.Components.InputStates
{
    using Cozy.Builder.Game.Components;
    using Cozy.Builder.Messaging;
    using Cozy.Builder.Messaging.Messages;
    using Cozy.Builder.Utility.Components;
    using UnityEngine;

    public class GameplayDefaultState : IMonoState<GameInputComponent>,
        IMessageReceiver<InputMessage>
    {
        private Vector3? dragAnchor;

        private Vector3 dragPosition;

        private bool shouldDrag = false;
        
        public override void Enter(GameInputComponent context)
        {
            base.Enter(context);
            Messenger.Subscribe(this);
        }

        public override void Exit(GameInputComponent context)
        {
            Messenger.Unsubscribe(this);
            base.Exit(context);
        }

        public void OnMessageReceived(InputMessage message)
        {
            shouldDrag = message.IsMoving;
            
            if (shouldDrag)
            {
                if (FindWorldPosition(message.MovePosition.Value, out Vector3 world))
                {
                    if (dragAnchor == null)
                    {
                        dragAnchor = world;
                        dragPosition = Context.GameCamera.transform.position;
                    }

                    var worldOffset = dragAnchor.Value - world;
                    var targetPosition = Context.GameCamera.transform.position + worldOffset;

                    dragPosition = Context.Bounds.ContainmentClamp(targetPosition);
                }
            }
            else
            {
                dragAnchor = null;
            }
        }

        private void LateUpdate()
        {
            UpdateCameraPosition();
        }

        private bool FindWorldPosition(Vector2 screenPosition, out Vector3 worldPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Context.InputMask))
            {
                worldPosition = hit.point;
                return true;
            }

            worldPosition = Vector3.zero;
            return false;
        }

        private void UpdateCameraPosition()
        {
            var position = Context.GameCamera.transform.position;

            if (position == dragPosition)
                return;

            position = Vector3.Lerp(position, dragPosition, Time.deltaTime * 15f);

            if (Vector3.Distance(position, dragPosition) < 0.01f)
                position = dragPosition;

            Context.GameCamera.transform.position = position;
        }
    }
}