using UnityEngine;

namespace Cozy.Builder.Messaging.Messages
{
    [System.Serializable]
    public struct InputMessage    
    {
        public Vector2? DownPosition;
        public Vector2? MovePosition;
        public Vector2 MoveDelta;
        public bool IsTouching;
        public bool IsAltTouching;
        public bool IsMoving;
    }
}