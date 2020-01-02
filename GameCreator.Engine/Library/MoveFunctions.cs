﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RTree;
using static System.Math;

namespace GameCreator.Engine.Library
{
    public class MoveFunctions : IMoveFunctions
    {
        public GameContext Context { get; }

        public MoveFunctions(GameContext context)
        {
            Context = context;
        }
        
        #region Helpers

        public Matrix3x2 GetSpriteTransform(GameInstance i)
        {
            return GetSpriteTransform(i, i.X, i.Y);
        }
        
        public Matrix3x2 GetSpriteTransform(GameInstance i, double x, double y)
        {
            if (i.Sprite == null) return Matrix3x2.Identity;
            
            var m = Matrix3x2.CreateTranslation(-i.Sprite.XOrigin, -i.Sprite.YOrigin);
            if (i.ImageXScale != 1.0 || i.ImageYScale != 1.0)
                m *= Matrix3x2.CreateScale((float) i.ImageXScale, (float) i.ImageYScale);
            if (i.ImageAngle != 0.0)
                m *= Matrix3x2.CreateRotation((float) (-i.ImageAngle * PI / 180));
            m *= Matrix3x2.CreateTranslation((float) x, (float) y);
            // Todo: Transform by a world matrix in the graphics context
            return m;
        }

        public BoundingBox GetSpriteTransformedBoundingBox(GameSprite s, Matrix3x2 transform)
        {
            if (s.BoundingBox.Right < s.BoundingBox.Left || s.BoundingBox.Bottom < s.BoundingBox.Top)
                return s.BoundingBox;
            
            var corners = new[]
            {
                new Vector2(s.BoundingBox.Left, s.BoundingBox.Top),
                new Vector2(s.BoundingBox.Right + 1, s.BoundingBox.Top),
                new Vector2(s.BoundingBox.Right + 1, s.BoundingBox.Bottom + 1),
                new Vector2(s.BoundingBox.Left, s.BoundingBox.Bottom + 1)
            };
            
            var transformedVectors = corners
                .Select(v => Vector2.Transform(v, transform))
                .ToArray();
            
            return new BoundingBox((int)transformedVectors.Min(v => v.X),
                (int)transformedVectors.Min(v => v.Y),
                (int)transformedVectors.Max(v => v.X - 1),
                (int)transformedVectors.Max(v => v.Y - 1));
        }

        public bool CheckSpriteCollision(GameSprite sprite1, int subImage1, Matrix3x2 modelWorldTransform1,
            GameSprite sprite2, int subImage2, Matrix3x2 modelWorldTransform2)
        {
            return GetSpriteCollisions(sprite1, subImage1, modelWorldTransform1, sprite2, subImage2, modelWorldTransform2).Any();
        }

        public RTree<int> GenerateCollisionTree(IEnumerable<GameInstance> instances)
        {
            var tree = new RTree<int>();

            foreach (var instance in instances)
            {
                var rect = GetInstanceRTreeRectangle(instance);
                
                if (rect == null) continue;
                
                tree.Add(rect, instance.Id);
            }

            return tree;
        }

        public Rectangle GetInstanceRTreeRectangle(GameInstance instance)
        {
            if (instance.Sprite == null) return null;
                
            var transform = GetSpriteTransform(instance);
            var bb = GetSpriteTransformedBoundingBox(instance.Sprite, transform);
                
            if (!bb.IsValid) return null;

            return new Rectangle(bb.Left, bb.Top, bb.Right + 1, bb.Bottom + 1, 0, 0);
        }

        public IEnumerable<int> InstancesInBoundingBox(GameInstance i, RTree<int> tree)
        {
            var rect = GetInstanceRTreeRectangle(i);
            if (rect == null) return new int[] { };

            return tree.Intersects(rect);
        }

        public void RemoveFromRTree(GameInstance i, RTree<int> tree)
        {
            var rect = GetInstanceRTreeRectangle(i);
            tree.Delete(rect, i.Id);
        }

        /// <summary>
        /// Gets the individual pixel coordinates of a collision between two sprites.
        /// </summary>
        /// <param name="sprite1">The first sprite.</param>
        /// <param name="subImage1">The subimage of the first sprite.</param>
        /// <param name="modelWorldTransform1">The transformation matrix of the first sprite.</param>
        /// <param name="sprite2">The second sprite.</param>
        /// <param name="subImage2">The subimage of the second sprite.</param>
        /// <param name="modelWorldTransform2">The transformation matrix of the second sprite.</param>
        /// <returns>An enumerable of (x, y) tuple values representing the pixels in the collision.</returns>
        public IEnumerable<(int x, int y)> GetSpriteCollisions(GameSprite sprite1, int subImage1, 
            Matrix3x2 modelWorldTransform1, GameSprite sprite2, int subImage2, Matrix3x2 modelWorldTransform2)
        {
            // First, transform the bounding boxes to account for sprite origin, rotations, scaling, and position.
            var bb1 = GetSpriteTransformedBoundingBox(sprite1, modelWorldTransform1);
            var bb2 = GetSpriteTransformedBoundingBox(sprite2, modelWorldTransform2);
            
            // Invert the transformation matrixes so we can map world coordinates into sprite coordinates.
            Matrix3x2.Invert(modelWorldTransform1, out var spriteLocalTransform1);
            Matrix3x2.Invert(modelWorldTransform2, out var spriteLocalTransform2);

            // Get the intersection of both bounding boxes.
            var bbIntersection = bb1.Intersection(ref bb2);

            // If the bounding boxes don't overlap, there is no collision.
            if (!bbIntersection.IsValid)
                yield break;

            // Get the collision mask for each sprite.
            var mask1 = sprite1.SeparateCollisionMasks ? sprite1.CollisionMasks[subImage1] : sprite1.CollisionMasks[0];
            var mask2 = sprite2.SeparateCollisionMasks ? sprite2.CollisionMasks[subImage2] : sprite2.CollisionMasks[0];
            
            // Look at each pixel in the bounding box intersection.
            for (var x = bbIntersection.Left; x <= bbIntersection.Right; x++)
            {
                for (var y = bbIntersection.Top; y <= bbIntersection.Bottom; y++)
                {
                    // Transform the pixel coordinate to sprite coordinates for both sprites
                    // using the inverted world matrix.
                    var localV1 = Vector2.Transform(new Vector2(x, y), spriteLocalTransform1);
                    var localV2 = Vector2.Transform(new Vector2(x, y), spriteLocalTransform2);

                    // Check if the respective sprite coordinates overlap solid pixels in both sprites.
                    if (mask1.HitTest((int) localV1.X, (int) localV1.Y) 
                        && mask2.HitTest((int) localV2.X, (int) localV2.Y))
                    {
                        yield return (x, y);
                    }
                }
            }
        }

        public IEnumerable<GameInstance> GetCollisions(GameInstance i, IEnumerable<GameInstance> instances,
            bool onlySolid)
        {
            return GetCollisions(i, i.X, i.Y, instances, onlySolid);
        }
        
        public bool PlaceFree(GameInstance i, double x, double y, bool onlySolid)
        {
            return PlaceFree(i, x, y, Context.PresortedInstances, onlySolid);
        }

        public bool PlaceFree(GameInstance i, double x, double y, IEnumerable<GameInstance> instances,
            bool onlySolid)
        {
            return !GetCollisions(i, x, y, instances, onlySolid).Any();
        }
        
        public IEnumerable<GameInstance> GetCollisions(GameInstance i, double x, double y, 
            IEnumerable<GameInstance> instances, bool onlySolid)
        {
            if (i.Sprite == null) yield break;

            var transform1 = GetSpriteTransform(i, x, y);

            foreach (var other in instances)
            {
                if (other.Id == i.Id) continue;
                if (other.Sprite == null) continue; // Can change, so check again
                if (onlySolid && !other.Solid) continue;
                if (!(i.AssignedObject.IsEventRegistered(other.AssignedObject)
                      || other.AssignedObject.IsEventRegistered(i.AssignedObject))) continue;
                var transform2 = GetSpriteTransform(other);

                if (CheckSpriteCollision(i.Sprite, i.ComputeSubimage(), transform1, other.Sprite, 
                    other.ComputeSubimage(), transform2))
                {
                    yield return other;
                }
            }
        }

        public void MoveContactPosition(GameInstance i, double direction, double maxDist, bool onlySolid)
        {
            if (i.Speed <= 0.00001 || !PlaceFree(i, i.X, i.Y, onlySolid))
                return;
            
            var xstart = i.X;
            var ystart = i.Y;
            var xstep = Cos(direction * PI / 180);
            var ystep = -Sin(direction * PI / 180);

            for (var j = 0; j <= (int) Ceiling(maxDist); j++)
            {
                var dist = Min(j, maxDist);
                var newx = xstart + dist * xstep;
                var newy = ystart + dist * ystep;
                if (!PlaceFree(i, newx, newy, onlySolid))
                {
                    break;
                }
                i.X = newx;
                i.Y = newy;
            }
        }
        #endregion

        public void MotionSet(GameInstance self, double direction, double speed)
        {
            throw new NotImplementedException();
        }

        public void MotionAdd(GameInstance self, double direction, double speed)
        {
            throw new NotImplementedException();
        }

        public bool PlaceFree(GameInstance self, double x, double y)
        {
            throw new NotImplementedException();
        }

        public bool PlaceEmpty(GameInstance self, double x, double y)
        {
            throw new NotImplementedException();
        }

        public bool PlaceMeeting(GameInstance self, double x, double y, int anyId)
        {
            throw new NotImplementedException();
        }

        public bool PlaceSnapped(GameInstance self, int hsnap, int vsnap)
        {
            throw new NotImplementedException();
        }

        public void MoveRandom(GameInstance self, int hsnap, int vsnap)
        {
            throw new NotImplementedException();
        }

        public void MoveSnap(GameInstance self, int hsnap, int vsnap)
        {
            throw new NotImplementedException();
        }

        public void MoveWrap(GameInstance self, bool hor, bool vert, double margin)
        {
            throw new NotImplementedException();
        }

        public void MoveTowardsPoint(GameInstance self, double x, double y, double speed)
        {
            throw new NotImplementedException();
        }

        public void MoveBounceSolid(GameInstance self, bool advanced)
        {
            throw new NotImplementedException();
        }

        public void MoveBounceAll(GameInstance self, bool advanced)
        {
            throw new NotImplementedException();
        }

        public void MoveContactSolid(GameInstance self, double dir, double maxdist)
        {
            throw new NotImplementedException();
        }

        public void MoveContactAll(GameInstance self, double dir, double maxdist)
        {
            throw new NotImplementedException();
        }

        public void MoveOutsideSolid(GameInstance self, double dir, double maxdist)
        {
            throw new NotImplementedException();
        }

        public void MoveOutsideAll(GameInstance self, double dir, double maxdist)
        {
            throw new NotImplementedException();
        }

        public void DistanceToPoint(GameInstance self, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void DistanceToObject(GameInstance self, int anyId)
        {
            throw new NotImplementedException();
        }

        public void PositionEmpty(double x, double y)
        {
            throw new NotImplementedException();
        }

        public void PositionMeeting(double x, double y, int anyId)
        {
            throw new NotImplementedException();
        }
    }
}