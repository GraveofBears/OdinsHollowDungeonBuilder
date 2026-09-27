using UnityEngine;

namespace OdinsHollow.DungeonGen
{
    // Oriented bounding box used to keep generated rooms from overlapping.
    internal struct Obb
    {
        public Vector3 Center;
        public Quaternion Rotation;
        public Vector3 HalfExtents;

        public Obb(Vector3 center, Quaternion rotation, Vector3 halfExtents)
        {
            Center = center;
            Rotation = rotation;
            HalfExtents = halfExtents;
        }

        public Obb Transformed(Vector3 position, Quaternion rotation) => new(position + rotation * Center, rotation * Rotation, HalfExtents);

        public Vector3[] Corners()
        {
            Vector3[] corners = new Vector3[8];
            int i = 0;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
                corners[i++] = Center + Rotation * Vector3.Scale(HalfExtents, new Vector3(x, y, z));
            return corners;
        }

        // Separating axis test. Each box is shrunk by shrink/2 per side so rooms may overlap by up to 'shrink' meters.
        public static bool Intersects(Obb a, Obb b, float shrink)
        {
            Vector3 ha = Shrink(a.HalfExtents, shrink * 0.5f);
            Vector3 hb = Shrink(b.HalfExtents, shrink * 0.5f);
            Vector3[] axesA = { a.Rotation * Vector3.right, a.Rotation * Vector3.up, a.Rotation * Vector3.forward };
            Vector3[] axesB = { b.Rotation * Vector3.right, b.Rotation * Vector3.up, b.Rotation * Vector3.forward };

            float[,] r = new float[3, 3];
            float[,] absR = new float[3, 3];
            for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                r[i, j] = Vector3.Dot(axesA[i], axesB[j]);
                absR[i, j] = Mathf.Abs(r[i, j]) + 1e-5f;
            }

            Vector3 d = b.Center - a.Center;
            float[] t = { Vector3.Dot(d, axesA[0]), Vector3.Dot(d, axesA[1]), Vector3.Dot(d, axesA[2]) };

            for (int i = 0; i < 3; i++)
            {
                float ra = ha[i];
                float rb = hb[0] * absR[i, 0] + hb[1] * absR[i, 1] + hb[2] * absR[i, 2];
                if (Mathf.Abs(t[i]) > ra + rb) return false;
            }

            for (int j = 0; j < 3; j++)
            {
                float ra = ha[0] * absR[0, j] + ha[1] * absR[1, j] + ha[2] * absR[2, j];
                float rb = hb[j];
                if (Mathf.Abs(t[0] * r[0, j] + t[1] * r[1, j] + t[2] * r[2, j]) > ra + rb) return false;
            }

            for (int i = 0; i < 3; i++)
            {
                int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                for (int j = 0; j < 3; j++)
                {
                    int j1 = (j + 1) % 3, j2 = (j + 2) % 3;
                    float ra = ha[i1] * absR[i2, j] + ha[i2] * absR[i1, j];
                    float rb = hb[j1] * absR[i, j2] + hb[j2] * absR[i, j1];
                    if (Mathf.Abs(t[i2] * r[i1, j] - t[i1] * r[i2, j]) > ra + rb) return false;
                }
            }

            return true;
        }

        private static Vector3 Shrink(Vector3 half, float amount) => new(Mathf.Max(0.1f, half.x - amount), Mathf.Max(0.1f, half.y - amount), Mathf.Max(0.1f, half.z - amount));
    }
}
