using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Processes;

namespace MLab.CurvedPoly
{ 
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class CurvedPoly : MonoBehaviour {

        public CurvedPolyAsset curvedPoly;
        public LoDs lods;
         
        public int LodIndex {
            get {
                return itemIndex;
            } set {
                if ((value >= 0) && (value <= lods.availableLoqs.Count)) {
                    if (itemIndex != value) {
                        itemIndex = value;
                        UpdateMesh(true);
                    }
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private int itemIndex;

        [SerializeField]
        [HideInInspector]
        public MeshesRefAsset meshesRefAsset = null;
         
        private Mesh actualMesh;
        private long actualMeshTimestamp;

        public CurvedPolygonsNet GetCPN()
        {
            //Check Tessellation Process First
            if (curvedPoly == null) {
                curvedPoly = ScriptableObject.CreateInstance<CurvedPolyAsset>();
            }
            return curvedPoly.GetCPN();
        }

        private void CheckMeshFilter() { 
            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            filter.hideFlags |= HideFlags.HideAndDontSave | HideFlags.HideInInspector | HideFlags.DontSaveInBuild;
        }

        private void Awake() {
            CheckMeshFilter();
            CheckMeshUpdate();
        }

        private void CheckMeshUpdate()
        {
            if (curvedPoly != null) {
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter!=null && meshFilter.sharedMesh != null)
                {
                    if (meshFilter.sharedMesh.triangles.Length == 0)
                    {
                        curvedPoly.MarkChanged();
                        UpdateMesh(true);
                        Update();
                    }
                }
                else
                {
                    curvedPoly.MarkChanged();
                    UpdateMesh(true);
                    Update();
                }
            } 
        }

        void Start()
        {
            CheckMeshFilter();
            GetCPN(); 
            UpdateMesh(false);
        }

        public void RecalculateNormals() {
            actualMesh.RecalculateNormals();
        }

        public void UpdateMesh(bool dynamicMode) {

            doUpdateMesh = true;
            doUpdateMeshDynamicMode = dynamicMode;
        }

        private bool doUpdateMesh;
        private bool doUpdateMeshDynamicMode;

        public void Update()
        {
            if (doUpdateMesh)
            {
                //long ticks = DateTime.Now.Ticks;
                CheckMeshFilter();
                doUpdateMesh = false;
                bool dynamicMode = doUpdateMeshDynamicMode;

                if (curvedPoly != null && lods != null)
                {

                    //Can't be null, it's required (with RequireComponent)
                    MeshFilter meshFilter = GetComponent<MeshFilter>();

                    Mesh input = null;
                    if (this.meshesRefAsset!=null) {
                        input = this.meshesRefAsset.meshes[itemIndex];
                    }

                    actualMesh = curvedPoly.GetMesh(lods, itemIndex, dynamicMode,input);

                    meshFilter.mesh = actualMesh;
                    actualMeshTimestamp = Environment.TickCount;

                    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

                    Material[] oldMaterials = meshRenderer.sharedMaterials;

                    if (oldMaterials.Length != actualMesh.subMeshCount)
                    {
                        Material[] materials = new Material[actualMesh.subMeshCount];
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (i < oldMaterials.Length)
                            {
                                materials[i] = oldMaterials[i];
                            }
                            else if (oldMaterials.Length > 0)
                            {
                                materials[i] = oldMaterials[oldMaterials.Length - 1];
                            }
                        }
                        meshRenderer.sharedMaterials = materials;
                    }
                    
                }
                else if (actualMesh == null)
                {
                    actualMesh = new Mesh(); 
                }

                //long ticks2 = DateTime.Now.Ticks;
                //Debug.Log("Update Mesh Total Ticks Amount " + (ticks2 - ticks));
            }
        }

        public void UpdateAllMeshes() {

            this.curvedPoly.MarkChanged();
            for (int i = 0; i < lods.availableLoqs.Count; i++) {
                Mesh input = null;
                if (this.meshesRefAsset != null) {
                    input = this.meshesRefAsset.meshes[i];
                }
                curvedPoly.GetMesh(lods, i, doUpdateMeshDynamicMode, input);
            }
            this.UpdateMesh(doUpdateMeshDynamicMode);
        }

        public void RecomputeShape(CPNSubset subset=null) {
            curvedPoly.RecomputeShape(this.lods, this.itemIndex, subset);
        }

        public Mesh GetMesh() {
            if (actualMesh == null) {
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                return meshFilter.sharedMesh;
            } 
            return actualMesh;
        }

        public long GetActualMeshTimestamp() {
            return actualMeshTimestamp;
        }

        public TessellationOutput GetTessellationOutput()
        {
            return curvedPoly.GetTessellationOutput(this.lods, this.itemIndex);
        }
    } 
}
