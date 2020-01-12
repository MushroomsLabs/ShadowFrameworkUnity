using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLab.ShadowFramework
{
    public class CPNGeometry 
    {
        public int polygonsCount;
        /*polygonsIndex.Length==polygonsCount+1*/
        public short[] polygonsIndex = new short[1];
        /*polygons.Length==polygonsIndex[polygonsCount]*/
        public short[] polygons = new short[0];

        //interpolation schema used on 
        public short[] polygonsSchemas = new short[0];
         
        //Separated Triangles
        short[] triangles = new short[0];

        //Separated Quads (seriously)
        short[] quads = new short[0];

        bool[] updatePolygons;

        public CPNGeometry()
        {
            polygonsIndex = null;
            polygons = null;
        }
         
        public void Clean()
        {
            if (polygonsIndex != null)
            {
                // delete[] polygonsIndex;
                polygonsIndex = null;
            }
            if (polygons != null)
            {
                // delete[] polygons;
                polygons = null;
            }
        }

        public void Setup(int polygonsCount, short[] polygonsIndex, short[] polygons, short[] polygonSchemas)
        {
            this.polygonsCount = polygonsCount;
            this.polygonsIndex = polygonsIndex;
            this.polygons = polygons;
            this.polygonsSchemas = polygonSchemas; 
        }

        public void setTriangles(short[] triangles)
        {
            this.triangles = triangles;
        }

        public void setQuads(short[] quads)
        {
            this.quads = quads;
        }
         
        public int GetTrianglesCount()
        {
            return this.triangles.Length / 3;
        }

        public short[] GetTriangles()
        {
            return this.triangles;
        }

        public int GetQuadsCount()
        {
            return this.quads.Length >> 2;
        }

        public short[] GetQuads()
        {
            return this.triangles;
        }

        public int GetPolygonsCount()
        {
            return this.polygonsCount;
        }

        public int GetPolygonPosition(int index)
        {
            return this.polygonsIndex[index];
        }

        public short GetPolygonLength(int index)
        {
            return (short)(this.polygonsIndex[index + 1] - this.polygonsIndex[index]);
        }

        public short[] GetPolygonsIndex()
        {
            return this.polygonsIndex;
        }

        public short[] GetPolygons()
        {
            return this.polygons;
        }

        public short[] GetPolygonsSchemas()
        {
            return this.polygonsSchemas;
        }
        
    }
}