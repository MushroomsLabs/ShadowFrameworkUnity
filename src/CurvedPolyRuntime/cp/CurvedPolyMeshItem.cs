using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes;

namespace MLab.CurvedPoly
{  
    class CurvedPolyMeshItem
    { 
        public int id;
        public Mesh storedMesh;
        public long timestamp;
        private CurvedPolyVariants curvedPolyVariants;
        //TODO: here you may put other informations about this guy

        public CurvedPolyMeshItem(CurvedPolyVariants curvedPolyVariants,Mesh storedMesh = null)
        {
            this.curvedPolyVariants = curvedPolyVariants;
            this.id = -1;
            this.timestamp = -1;
            this.storedMesh = storedMesh;
        }

        public bool WasGenerated() {
            return id >= 0;
        }

        public bool NeedUpdate() {
            return id<0 || curvedPolyVariants.IsVariantGenerated(id) ||
                this.timestamp < curvedPolyVariants.GetChangeTimestamp();
        }

        /*this.storedMesh must be different from null */
        public void AsTessellationItem( short[] loqs)
        {
            if (this.storedMesh == null)
                return;
            
            if (id < 0) {
                id = curvedPolyVariants.GetFreeTessellationRecordId();
            }
            
            if (timestamp < curvedPolyVariants.GetChangeTimestamp() || 
                !curvedPolyVariants.IsVariantGenerated(id)) {
                
                this.timestamp = curvedPolyVariants.GetChangeTimestamp();
                curvedPolyVariants.GenerateTessellationVariant(id, loqs);
                curvedPolyVariants.UpdateTessellationVariant(id, loqs);
                WriteMesh(storedMesh, curvedPolyVariants.GetMeshOutput(id)/*,useUnityNormals*/);
            } 
        }
        
        public void UpdateTessellationItem(short[] loqs, CPNSubset subset=null)
        {
            curvedPolyVariants.UpdateTessellationVariant(id, loqs, subset);
            WriteMesh(storedMesh, curvedPolyVariants.GetMeshOutput(id)/*,useUnityNormals*/);
        }
         

        private void WriteMesh(Mesh storedMesh, OutputMesh output/*,bool useUnityNormals*/)
        { 
            if (output.GetVertices().Length > CPNTessellationProcess.MAX_VERTICES_SIZE) { 
                return;
            }

            storedMesh.Clear();
            storedMesh.vertices = output.GetVertices();
            storedMesh.uv = output.GetUVs();
            storedMesh.normals = output.GetNormals();

            int[][] triangles = output.GetTriangles();
            if (triangles.Length == 0) {
                storedMesh.SetTriangles(new int[0], 0);
            } else {
                storedMesh.subMeshCount = triangles.Length;
                for (int i = 0; i < triangles.Length; i++)
                {
                    storedMesh.SetTriangles(triangles[i], i);
                }
            } 
        }
    }
      
}


