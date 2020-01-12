using System;
using UnityEngine;

namespace MLab.ShadowFramework.Interpolation
{
    class CPDerivativeRotationMatrix
    {
        private float xx, xy, xz;
        private float yx, yy, yz;
        private float zx, zy, zz;

        public CPDerivativeRotationMatrix(Vector3 N, Vector3 DB0)
        {
            DB0 = DB0.normalized;

            //Remov the N component
            Vector3 correctedDB0 = DB0 - N * Vector3.Dot(N, DB0);
             
            Vector3 dev = correctedDB0;
            Vector3 dev0 = DB0;
            Vector3 Perp = Vector3.Cross(DB0, correctedDB0);
            if (Perp == Vector3.zero)
            {
                xx = 1; xy = 0; xz = 0;
                yx = 0; yy = 1; yz = 0;
                zx = 0; zy = 0; zz = 1;
                return;
            }
            Perp = Perp.normalized;

            //Subtract from dev its Perp component
            dev = dev - (Vector3.Dot(Perp, dev)) * Perp;
            //Renormalize dev
            dev = dev.normalized;

            //Check bad null case... 
            if (dev == Vector3.zero)
            {
                dev = dev0;
            }

            Vector3 dev0T = Vector3.Cross(dev0, Perp);
            Vector3 devT = Vector3.Cross(dev, Perp);

            xx = Perp.x * Perp.x + dev.x * dev0.x + devT.x * dev0T.x;
            xy = Perp.x * Perp.y + dev.x * dev0.y + devT.x * dev0T.y;
            xz = Perp.x * Perp.z + dev.x * dev0.z + devT.x * dev0T.z;

            yx = Perp.y * Perp.x + dev.y * dev0.x + devT.y * dev0T.x;
            yy = Perp.y * Perp.y + dev.y * dev0.y + devT.y * dev0T.y;
            yz = Perp.y * Perp.z + dev.y * dev0.z + devT.y * dev0T.z;

            zx = Perp.z * Perp.x + dev.z * dev0.x + devT.z * dev0T.x;
            zy = Perp.z * Perp.y + dev.z * dev0.y + devT.z * dev0T.y;
            zz = Perp.z * Perp.z + dev.z * dev0.z + devT.z * dev0T.z;
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
