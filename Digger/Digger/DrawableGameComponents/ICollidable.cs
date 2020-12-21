using Microsoft.Xna.Framework;

namespace Digger
{
    public interface ICollidable
    {
        Rectangle CollisionBox { get; }
        void HandleCollision();
    }
}