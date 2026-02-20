namespace Cozy.Builder.Messaging.Messages
{
    using UnityEngine;

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