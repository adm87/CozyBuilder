using UnityEngine;

namespace Cozy.Editor.Kenney.Assets.Editor.Kenney.Windows
{
    internal class SpriteEntry
    {
        public Sprite sprite;
        public string name;
        public string lowerName;
        public Vector2 size;
        public Rect rect;

        public SpriteEntry(Sprite s)
        {
            sprite = s;
            name = s.name;
            lowerName = s.name.ToLowerInvariant();
            size = new Vector2(s.rect.width, s.rect.height);
            rect = s.rect;
        }
    }
}