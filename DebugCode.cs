using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

public static class DebugCode
{
    // Draw the debug path.
    public static void DrawDebugPath(SpriteBatch spriteBatch, Vector2[] relativePattern, Vector2 startingPosition, int currentWaypointIndex, Vector2 Position)
    {
        Color color = Color.AliceBlue;
        if (relativePattern != null && relativePattern.Length > 0)
        {
            Vector2 previousPosition = startingPosition;

            // Draw lines between each waypoint.
            for (int i = 0; i < relativePattern.Length; i++)
            {
                Vector2 waypointPosition = startingPosition + relativePattern[i];

                // Draw a line from the previous position to the current waypoint.
                DebugCode.DrawLine(spriteBatch, previousPosition, waypointPosition, color, 2f);

                // Optionally, draw a circle at each waypoint for clarity.
                DebugCode.DrawCircle(spriteBatch, waypointPosition, 10f, color, 20);

                previousPosition = waypointPosition;
            }

            // Optionally, draw a line from the alien's current position to the next waypoint.
            if (currentWaypointIndex < relativePattern.Length)
            {
                Vector2 nextWaypointPosition = startingPosition + relativePattern[currentWaypointIndex];
                DebugCode.DrawLine(spriteBatch, Position, nextWaypointPosition, color, 2f);
            }
        }
    }

    // Draw a circle outline with the specified number of segments
    public static void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int segments = 100)
    {
        // Set a minimum radius for debugging visibility
        if (radius < 5f)
        {
            DrawLine(spriteBatch, center, center + new Vector2(1, 1), color, 2f); // Small dot for very small circles
            return;
        }

        // Adjust segments based on the radius to avoid over-segmentation for small circles
        segments = (int)Math.Max(12, radius * 2); // Minimum of 12 segments or a fraction of radius

        float step = MathHelper.TwoPi / segments;
        Vector2 previousPoint = new Vector2(center.X + radius, center.Y);

        for (float theta = step; theta <= MathHelper.TwoPi; theta += step)
        {
            Vector2 nextPoint = new Vector2(
                center.X + radius * (float)Math.Cos(theta),
                center.Y + radius * (float)Math.Sin(theta)
            );

            DrawLine(spriteBatch, previousPoint, nextPoint, color, thickness: 1f);
            previousPoint = nextPoint;
        }

        // Close the circle by drawing a line back to the starting point
        Vector2 firstPoint = new Vector2(center.X + radius, center.Y);
        DrawLine(spriteBatch, previousPoint, firstPoint, color);
    }

    // Draw a rectangle outline by connecting each corner
    public static void DrawRectangle(SpriteBatch spriteBatch, RectangleF rectangle, Color color, float thickness = 1f)
    {
        // Calculate the positions of the corners of the rectangle
        Vector2 topLeft = new Vector2(rectangle.Left, rectangle.Top);
        Vector2 topRight = new Vector2(rectangle.Right, rectangle.Top);
        Vector2 bottomLeft = new Vector2(rectangle.Left, rectangle.Bottom);
        Vector2 bottomRight = new Vector2(rectangle.Right, rectangle.Bottom);

        // Draw each of the four sides of the rectangle
        DrawLine(spriteBatch, topLeft, topRight, color, thickness);      // Top
        DrawLine(spriteBatch, topRight, bottomRight, color, thickness);  // Right
        DrawLine(spriteBatch, bottomRight, bottomLeft, color, thickness); // Bottom
        DrawLine(spriteBatch, bottomLeft, topLeft, color, thickness);    // Left
    }

    // Draw a simple line between two points (using a 1x1 white texture)
    public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(AssetManager.WhiteTexture,
                         new Rectangle((int)start.X, (int)start.Y, (int)length, (int)thickness),
                         null,
                         color,
                         angle,
                         Vector2.Zero,
                         SpriteEffects.None,
                         0);
    }

}
