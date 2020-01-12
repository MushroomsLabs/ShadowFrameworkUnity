using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;
using MLab.ShadowFramework.Processes; 

namespace MLab.ShadowFramework
{
    public class CurvedPolyVariants{

        public CurvedPolygonsNet curvedPolygonsNet = new CurvedPolygonsNet();

        public TessellationRecord[] records = new TessellationRecord[0];

        private long changedTimestamp;

        public void SetCPN(CurvedPolygonsNet curvedPolygonsNet) {
            this.curvedPolygonsNet = curvedPolygonsNet;
        }

        public long GetChangeTimestamp() {
            return changedTimestamp;
        }

        public void SetChangeTimestamp(long timestamp)
        {
            changedTimestamp = timestamp;
        }

        private bool Invalid(int id) {
            return id < 0 || id > records.Length || !records[id].used;
        }

        public CurvedPolygonsNet GetCPN()
        {
            return curvedPolygonsNet;
        }
         
        public int GetFreeTessellationRecordId() {
            for (int i = 0; i < records.Length; i++)
            {
                if (!records[i].used) {
                    records[i].used = true;
                    return i;
                }
            }
            AddNewRecord();
            records[records.Length - 1].used = true;
            return records.Length - 1;
        }

        public void SetRecord(int id, OutputMesh mesh, TessellationOutput tessellationOutput)
        {
            records[id].outputMesh = mesh;
            records[id].tessellationOutput = tessellationOutput;
        }

        public OutputMesh GetMeshOutput(int id)
        {
            if (Invalid(id))
                return null;
            return records[id].outputMesh;
        }

        public TessellationOutput GetTessellationOutput(int id)
        {
            if (Invalid(id))
                return null;
            return records[id].tessellationOutput;
        }

        public void FreeTessellationRecordId(int id)
        {
            if(id>=0 && id<this.records.Length)
                this.records[id].Free(); 
        }

        private void AddNewRecord() {
            TessellationRecord[] newRecords = new TessellationRecord[records.Length + 1];
            for (int i = 0; i < records.Length; i++)
            {
                newRecords[i] = records[i];
            }
            newRecords[records.Length] = new TessellationRecord();
            this.records = newRecords;
        }


        public void GenerateTessellationVariant(int id,short[] loqs) {

            if (id < 0 || id > records.Length || !records[id].used)
                return;

            CPNTessellationProcess tessellationProcess = ProcessesKeeper.GetTessellationProcess();
            TessellationOutput tesellationOutput = tessellationProcess.InitProcess(
                this.curvedPolygonsNet, loqs);

            tessellationProcess.BuildProfile();

            int[] builtTrianglesCount = tesellationOutput.GetBuiltTrianglesSize();
            int builtVerticesCount = tesellationOutput.GetBuiltVerticesSize(); 
            OutputMesh outputMesh = new OutputMesh();
            outputMesh.Build(builtVerticesCount, builtTrianglesCount);
             
            SetRecord(id, outputMesh, tesellationOutput);
             
        }

        public bool IsVariantGenerated(int id) {
            return GetMeshOutput(id) != null;
        }

        public void UpdateTessellationVariant(int id, short[] loqs, CPNSubset cpnSubset=null) {

            if (id < 0 || id > records.Length || !records[id].used)
                return;

            CPNTessellationProcess tessellationProcess = ProcessesKeeper.GetTessellationProcess();
            OutputMesh mesh = GetMeshOutput(id);
            if (mesh != null) {
                tessellationProcess.InitPrebuiltProcess(this.curvedPolygonsNet, GetTessellationOutput(id), cpnSubset, loqs);
                tessellationProcess.WriteMesh(mesh);
            } 
        }
    }
}
