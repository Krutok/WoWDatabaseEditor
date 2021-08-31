using System;
using System.Collections.Generic;
using WowPacketParser.Proto;

namespace WDE.PacketViewer.Utils
{
    public static class VectorExtensions
    {
        public static float Distance2D(this Vec3 a, Vec3 b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
        
        public static float Distance3D(this Vec3 a, Vec3 b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z));
        }

        public static float TotalDistance(this Vec3 start, IList<Vec3> points)
        {
            float distance = 0;
            var prev = start;
            for (var i = 0; i < points.Count; ++i)
            {
                var cur = points[i];
                distance += cur.Distance2D(prev);
                prev = cur;
            }
            return distance;
        }

        public static float TotalDistance(this IList<Vec3> points)
        {
            if (points.Count == 0)
                return 0;

            float distance = 0;
            var prev = points[0];
            for (var i = 1; i < points.Count; ++i)
            {
                var cur = points[i];
                distance += cur.Distance2D(prev);
                prev = cur;
            }
            return distance;
        }

        public static float TotalDistance(this Vec3 start, IList<Vec3> points, Vec3 end)
        {
            
            float distance = 0;
            var prev = start;
            for (var i = 0; i < points.Count; ++i)
            {
                var cur = points[i];
                distance += cur.Distance2D(prev);
                prev = cur;
            }
            distance += prev.Distance2D(end);
            return distance;
        }
    }
}