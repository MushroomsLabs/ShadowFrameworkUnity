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
    class CurvedPolyMeshItemDB {
          
        private Dictionary<LoDs, CurvedPolyMeshItem[]> tessellationModels = new Dictionary<LoDs, CurvedPolyMeshItem[]>();

        private CurvedPolyVariants curvedPolyVariants=new CurvedPolyVariants();

        public CurvedPolygonsNet GetCPN() {
            return curvedPolyVariants.GetCPN();
        }

        public long GetChangedTimestamp()
        {
            return curvedPolyVariants.GetChangeTimestamp();
        }
         
        public void MarkChanged()
        {
            curvedPolyVariants.SetChangeTimestamp(DateTime.Now.Ticks);
        }

        public void InitLods(LoDs lods) {
            if (!tessellationModels.ContainsKey(lods)) {
                CurvedPolyMeshItem[] items = new CurvedPolyMeshItem[lods.availableLoqs.Count];
                for (int i = 0; i < items.Length; i++) {
                    items[i] = new CurvedPolyMeshItem(curvedPolyVariants);
                }
                tessellationModels[lods] = items;
            }
        }

        public int GetItemsCount() {
            return tessellationModels.Count;
        }

        public CurvedPolyMeshItem[] GetItem(LoDs lods)
        {
            if(tessellationModels.ContainsKey(lods))
                return tessellationModels[lods];
            return null;
        }

        public void ClearMeshes(LoDs lods)
        {
            CurvedPolyMeshItem[] items = GetItem(lods);
            if (items != null) {
                for (int i = 0; i<items.Length;i++)
                {
                    items[i].storedMesh = null;
                }
            }
        }

        public TessellationOutput GetTessellationOutput(LoDs lods, int index)
        {
            if (tessellationModels.ContainsKey(lods)) {
                CurvedPolyMeshItem[] items = tessellationModels[lods];
                return this.curvedPolyVariants.GetTessellationOutput(items[index].id);
            }
            return null;
        }

        public void RecomputeShape(LoDs lods, int index, CPNSubset cpnSubset=null) {
            
            CurvedPolyMeshItem[] items = tessellationModels[lods];
            if (items != null) {
                if (index < items.Length && items[index].WasGenerated())
                {
                    short[] loqs = lods.availableLoqs[index].values;
                    items[index].UpdateTessellationItem(loqs, cpnSubset);
                }
            }
        }

        public Mesh GetMesh(LoDs lods, int index,  bool dynamicMode, Mesh input=null) {
             
            InitLods(lods);
                
            CurvedPolyMeshItem[] items = tessellationModels[lods];
             
            if (items[index].storedMesh != null && !dynamicMode && !items[index].NeedUpdate()) {
                return items[index].storedMesh;
            } else
            {    
                short[] loqs =lods.availableLoqs[index].values;

                if (input!=null && input != items[index].storedMesh) {
                    items[index].storedMesh = input;
                    if (dynamicMode)
                        items[index].storedMesh.MarkDynamic();
                }

                if (items[index].storedMesh == null) {
                    items[index].storedMesh = new Mesh();
                    if (dynamicMode)
                        items[index].storedMesh.MarkDynamic();
                }

                if (items[index].NeedUpdate()) {
                    items[index].AsTessellationItem(loqs);
                }
                 
                return items[index].storedMesh;
            }
             
        }
        
    }
}


