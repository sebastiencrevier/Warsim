using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Newtonsoft.Json;

using Warsim.Core.Game;
using Warsim.Core.Game.Entities;

namespace Warsim.Core.Helpers.Graphics
{
    public static class MapThumbnailDrawer
    {
        public const int TableWidth = 640;
        public const int TableHeight = 320;
        public const int Padding = 80;

        public static Image Draw(Map map)
        {
            var bitmap = new Bitmap(TableWidth + 2*Padding, TableHeight + 2*Padding, PixelFormat.Format32bppRgb);
            var g = System.Drawing.Graphics.FromImage(bitmap);

            // Draw background
            g.Clear(Color.SteelBlue);

            // Draw table
            g.FillRectangle(Brushes.White, Padding, Padding, TableWidth, TableHeight);

            // Draw scene objects
            DrawNodes(g, map.DatabaseMap());

            return bitmap;
        }

        private static PointF ToPointF(this Vector vector)
        {
            return new PointF((float)vector.X, (float)vector.Z);
        }

        private static PointF FromTableCenter(this PointF point)
        {
            var ratio = 2.0f;

            return new PointF(
                Padding + TableWidth / 2.0f + point.X * ratio,
                Padding + TableHeight / 2.0f + point.Y * ratio
            );
        }

        private static void DrawNodes(System.Drawing.Graphics g, IList<Node> nodes)
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                DrawNodes(g, node.Children);

                switch (node.Type)
                {
                    case "poteau":
                        DrawPost(g, node.Position.ToPointF(), (float)node.Scale.X);
                        break;
                    case "mur":
                        DrawWall(g, node.Position.ToPointF(), (float)node.Scale.X, (float)node.Scale.Z, (float)node.Rotation.Y);
                        break;
                    case "lignes":
                        DrawLines(g, node.Position.ToPointF(), (float)node.Rotation.Y, node.Children);
                        break;
                }
            }
        }

        private static void DrawPost(System.Drawing.Graphics g, PointF center, float size)
        {
            var point = center.FromTableCenter();
            size *= 2.0f;

            g.FillEllipse(Brushes.Green, point.X - size/2.0f, point.Y - size/2.0f, size, size);
        }

        private static void DrawWall(System.Drawing.Graphics g, PointF startingPoint, float size, float length, float rotation)
        {
            var point = startingPoint.FromTableCenter();
            length *= 2.12f;
            size *= 0.28f;

            g.TranslateTransform(point.X, point.Y);
            g.RotateTransform(-rotation);
            g.FillRectangle(Brushes.DarkBlue, -size/2.0f, -length, size, length);
            g.ResetTransform();
        }

        private static void DrawLines(System.Drawing.Graphics g, PointF startingPoint, float rotation, IList<Node> lines)
        {
            var point = startingPoint.FromTableCenter();

            foreach (var line in lines)
            {
                g.TranslateTransform(point.X, point.Y);
                g.RotateTransform(-rotation);

                DrawLine(g, line.Position.ToPointF(), (float)line.Scale.X, (float)line.Scale.Z, (float)line.Rotation.Y);

                g.ResetTransform();
            }
        }

        private static void DrawLine(System.Drawing.Graphics g, PointF startingPoint, float size, float length, float rotation)
        {
            var point = new PointF(2.0f*startingPoint.X, 2.0f*startingPoint.Y);
            length *= 2.0f;
            size *= 0.6f;

            g.TranslateTransform(point.X, point.Y, MatrixOrder.Prepend);
            g.RotateTransform(-rotation, MatrixOrder.Prepend);
            g.FillRectangle(Brushes.Black, -size / 2.0f, -length, size, length);
        }
    }
}