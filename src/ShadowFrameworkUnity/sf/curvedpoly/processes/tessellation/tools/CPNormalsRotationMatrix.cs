using System;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    public struct CPNormalsRotationMatrix
    {
        private float xx, xy, xz;
        private float yx, yy, yz;
        private float zx, zy, zz;
        
        public CPNormalsRotationMatrix(Vector3 N0, Vector3 N)
        {
            Vector3 n = N;
            Vector3 n0 = N0;
            Vector3 Ortho = Vector3.Cross(N0, N);
            if (Ortho == Vector3.zero) {
                xx = 1; xy = 0; xz = 0;
                yx = 0; yy = 1; yz = 0;
                zx = 0; zy = 0; zz = 1;
                return;
            }
            Ortho = Ortho.normalized;

            //Subtract from dev its Perp component
            n = n - (Vector3.Dot(Ortho, n)) * Ortho;
            //Renormalize dev
            n = n.normalized;

            //Check bad null case... 
            if (n == Vector3.zero)
            {
                n = n0;
            }

            Vector3 n0T = Vector3.Cross(n0, Ortho);
            Vector3 nT = Vector3.Cross(n, Ortho);

            xx = Ortho.x * Ortho.x + n.x * n0.x + nT.x * n0T.x;
            xy = Ortho.x * Ortho.y + n.x * n0.y + nT.x * n0T.y;
            xz = Ortho.x * Ortho.z + n.x * n0.z + nT.x * n0T.z;

            yx = Ortho.y * Ortho.x + n.y * n0.x + nT.y * n0T.x;
            yy = Ortho.y * Ortho.y + n.y * n0.y + nT.y * n0T.y;
            yz = Ortho.y * Ortho.z + n.y * n0.z + nT.y * n0T.z;

            zx = Ortho.z * Ortho.x + n.z * n0.x + nT.z * n0T.x;
            zy = Ortho.z * Ortho.y + n.z * n0.y + nT.z * n0T.y;
            zz = Ortho.z * Ortho.z + n.z * n0.z + nT.z * n0T.z;
        }

        public CPNormalsRotationMatrix(Vector3 N0, Vector3 N,Vector3 Ortho)
        {
            Vector3 n = N;
            Vector3 n0 = N0; 
            Ortho = Ortho.normalized;

            //Subtract from dev its Perp component
            n = n - (Vector3.Dot(Ortho, n)) * Ortho;
            //Renormalize dev
            n = n.normalized;

            n0 = n0 - (Vector3.Dot(Ortho, n0)) * Ortho;
            //Renormalize dev
            n0 = n0.normalized;

            //Check bad null case... 
            if (n == Vector3.zero)
            {
                n = n0;
            }

            Vector3 n0T = Vector3.Cross(n0, Ortho);
            Vector3 nT = Vector3.Cross(n, Ortho);

            xx = Ortho.x * Ortho.x + n.x * n0.x + nT.x * n0T.x;
            xy = Ortho.x * Ortho.y + n.x * n0.y + nT.x * n0T.y;
            xz = Ortho.x * Ortho.z + n.x * n0.z + nT.x * n0T.z;

            yx = Ortho.y * Ortho.x + n.y * n0.x + nT.y * n0T.x;
            yy = Ortho.y * Ortho.y + n.y * n0.y + nT.y * n0T.y;
            yz = Ortho.y * Ortho.z + n.y * n0.z + nT.y * n0T.z;

            zx = Ortho.z * Ortho.x + n.z * n0.x + nT.z * n0T.x;
            zy = Ortho.z * Ortho.y + n.z * n0.y + nT.z * n0T.y;
            zz = Ortho.z * Ortho.z + n.z * n0.z + nT.z * n0T.z;
        }


        public Vector3 Rotate(Vector3 v)
        {
            return new Vector3(
                    xx * v.x + xy * v.y + xz * v.z,
                    yx * v.x + yy * v.y + yz * v.z,
                    zx * v.x + zy * v.y + zz * v.z
                );
        }
    }
}
