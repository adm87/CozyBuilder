namespace Cozy.Builder.Camera
{
    using Cozy.Builder.Messaging;
    using Cozy.Builder.Messaging.Messages;
    using Cozy.Builder.Utility;
    using UnityEngine;

    public class CameraPanComponent : MonoBehaviour,
        IMessageReceiver<InputMessage>
    {
        [SerializeField]
        private LayerMask inputMask;

        [SerializeField]
        private VolumeComponent bounds;

        private Vector3? panStartPosition;

        private Vector3 targetPosition;

        private bool shouldPan = false;
        private bool shouldRotate = false;

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
            PanCamera();
        }

        public void OnMessageReceived(InputMessage message)
        {
            // Note: Panning takes priority over rotating, so if both are active, only panning will occur.

            shouldPan = message.IsMoving;
            shouldRotate = message.IsAltTouching && !shouldPan;
            
            if (shouldPan)
            {
                if (message.MoveDelta == Vector2.zero)
                    return;

                if (panStartPosition == null)
                {
                    if (FindWorldPosition(message.MovePosition.Value, out Vector3 down))
                    {
                        panStartPosition = down;
                        targetPosition = transform.position;
                    }

                    return;
                }

                if (FindWorldPosition(message.MovePosition.Value, out Vector3 current))
                {
                    Vector3 delta = current - panStartPosition.Value;
                    targetPosition = bounds.ContainmentClamp(transform.position - delta);
                }
            }
            else
            {
                panStartPosition = null;
            }
        }

        private bool FindWorldPosition(Vector2 screenPosition, out Vector3 worldPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, inputMask))
            {
                worldPosition = hit.point;
                return true;
            }

            worldPosition = Vector3.zero;
            return false;
        }

        private void PanCamera()
        {
            if (targetPosition == transform.position)
                return;

            var position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

            if (Vector3.Distance(position, targetPosition) < 0.01f)
                position = targetPosition;

            transform.position = position;
        }
    }
}
