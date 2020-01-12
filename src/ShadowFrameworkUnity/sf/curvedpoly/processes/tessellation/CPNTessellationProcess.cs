using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Processes
{ 
    public class CPNTessellationProcess  {
        
        public const int MAX_VERTICES_SIZE = 65000;
        public const int TESSELLATION_PROCESS_NET_INTERPOLATORS = 10;
         
        private SFInterpolationSchemaManager manager;
        
        private CurvedPolygonsNet curvedPolygonsNet = new CurvedPolygonsNet();
        private TessellationOutput tessellationOutput = null;
        private CPNSubset subSet=null;
        private InterpolationSchemaMap map = null;

        public CPNTessellationProcess(SFInterpolationSchemaManager manager)
        {
            this.manager = manager; 
        }

        public TessellationOutput InitProcess(CurvedPolygonsNet curvedPolygonsNet, short[] loqs,
            InterpolationSchemaMap map=null)
        {
            tessellationOutput = new TessellationOutput();
            CPNGuideEvaluator.CPNGuide_loqs = loqs;
            this.curvedPolygonsNet = curvedPolygonsNet;
            this.subSet = null;
            this.map = map;
            return tessellationOutput;
        }

        public TessellationOutput InitProcess(CurvedPolygonsNet curvedPolygonsNet, short[] loqs, 
            CPNSubset subSet,
            InterpolationSchemaMap map = null) {
            tessellationOutput = new TessellationOutput();
            CPNGuideEvaluator.CPNGuide_loqs = loqs;
            this.curvedPolygonsNet = curvedPolygonsNet;
            this.subSet = subSet;
            this.map = map;
            return tessellationOutput;
        }
          
        public void InitPrebuiltProcess(CurvedPolygonsNet curvedPolygonsNet, 
            TessellationOutput output, CPNSubset subSet, short[] loqs,
            InterpolationSchemaMap map = null)
        {
            this.tessellationOutput = output;
            CPNGuideEvaluator.CPNGuide_loqs = loqs;
            this.curvedPolygonsNet = curvedPolygonsNet;
            this.subSet = subSet;
            this.map = map;
        } 

        public void BuildProfile()
        { 

            this.tessellationOutput.InitBuiltTrianglesSize(this.curvedPolygonsNet.GetGeometriesCount());
            this.tessellationOutput.SetBuiltVerticesSize(this.curvedPolygonsNet.GetNumberOfVertices());

            ExtractEdgesProfile();
            ExtractPolygonsProfile();
        }

        private IGuideModel GetGuideModel()
        {
            IGuideModel guideEvaluator = this.map == null ? null : this.map.GetGuideModel();
            guideEvaluator = guideEvaluator == null ? new CPNGuideEvaluator() : guideEvaluator;
            return guideEvaluator;
        }
         
        void ExtractEdgesProfile()
        {
            IGuideModel guideEvaluator = GetGuideModel();

            int edgesSize = curvedPolygonsNet.GetEdgesCount();
            int polylinesSize = curvedPolygonsNet.GetPolylinesCount();

            int totalESize = edgesSize + polylinesSize;

            CPNGuide[] guides = this.tessellationOutput.InitGuides(totalESize);
            int[] edgesProfile = this.tessellationOutput.InitEdgesProfile(totalESize * 4);
            int builtVerticesSize = this.tessellationOutput.GetBuiltVerticesSize();

            for (int i = 0; i < edgesSize; i++) {
                 
                short[] edge = curvedPolygonsNet.GetEdges(/* i */);
                int edgeLength = curvedPolygonsNet.GetEdgeLength(i);  
                short[] edgeHints = curvedPolygonsNet.GetEdgeHints(/* i */);

                int edgesIndex = curvedPolygonsNet.GetEdgePosition(i);

                int size = guideEvaluator.GetCurveTessellationSteps( edgeLength, edge, edgeHints,
                        edgesProfile, i);

                int index = (i) << 2;
                int n = edgesProfile[index + 3];
                int firstIndex = edgesIndex ;
                int lastIndex = edgeLength == 2 ? (firstIndex + 1) : (firstIndex + 3);
                edgesProfile[index] = edge[firstIndex];
                edgesProfile[index + 1] = edge[lastIndex];
                edgesProfile[index + 2] = builtVerticesSize;
                if(n>0)
                    builtVerticesSize += n - 1;
                CPNGuide polyline = new CPNGuide(edgesProfile, i, size);

                guides[i] = polyline; 
            }

            for (int i = 0; i < polylinesSize; i++)
            { 
                short[] polylines = curvedPolygonsNet.GetPolylines();
                int polylineLength = curvedPolygonsNet.GetPolylineLength(i); 

                int polylineIndex = curvedPolygonsNet.GetPolylinePosition(i);
                
                int index = (i + edgesSize) << 2;
                int n = polylineLength;// 
                edgesProfile[index + 3] = n - 1;
                int firstIndex = polylineIndex;
                int lastIndex = polylineIndex + polylineLength - 1;
                edgesProfile[index] = polylines[firstIndex];
                edgesProfile[index + 1] = polylines[lastIndex];
                edgesProfile[index + 2] = polylines[firstIndex+1]; 
                CPNGuide guide = new CPNGuide();
                guide.SetPolyline(polylineIndex, polylines, polylineLength); 
                guides[i+edgesSize] = guide;
            }

            this.tessellationOutput.SetBuiltVerticesSize(builtVerticesSize);
        }
         
        public void ExtractPolygonsProfile()
        {
            int geometriesCount = this.curvedPolygonsNet.GetGeometriesCount();
            int polygonsCount = this.curvedPolygonsNet.GetTotalPolygonsCount();
             
            CPNPolygon[][] polygonsData = tessellationOutput.InitPolygons(geometriesCount);
            int[] builtTrianglesSize = tessellationOutput.GetBuiltTrianglesSize();
            CPNGuide[] guides = tessellationOutput.GetGuides();
            int builtVerticesSize = this.tessellationOutput.GetBuiltVerticesSize();
            int[][] polygonsProfile = tessellationOutput.InitPolygonsProfile(geometriesCount);
            int[][] polygonsVerticesProfile = tessellationOutput.GetPolygonsVerticesProfile();
            
            for (int k = 0; k < geometriesCount; k++)
            {
                CPNGeometry geometry = this.curvedPolygonsNet.GetGeometries()[k];

                int overallPolygonsPosition = 0;
                int geomPolygonsCount = geometry.GetPolygonsCount();

                builtTrianglesSize[k] = 0;
                //geometriesProfile[k] = builtTrianglesSize;
                polygonsProfile[k] = new int[geomPolygonsCount + 1];
                polygonsVerticesProfile[k] = new int[geomPolygonsCount + 1];
                short[] polygons = geometry.GetPolygons();
                short[] schemas = geometry.polygonsSchemas; 
                polygonsData[k] = new CPNPolygon[geomPolygonsCount];

                for (int i = 0; i < geomPolygonsCount; i++)
                { 
                    int polygonPosition = geometry.GetPolygonPosition(i);
                    int polygonLength = geometry.GetPolygonLength(i);
                     
                    int effectivePolygonSize = 0;
                    for (int j = 0; j < polygonLength; j++)
                    {
                        effectivePolygonSize++;
                        int pIndex = polygons[polygonPosition + j];
                        if (pIndex == 0)
                        {
                            j += (1 + polygons[polygonPosition + j + 1]);
                        } 
                    }

                    CPNSideEdge[] iEdges = new CPNSideEdge[effectivePolygonSize];
                    int effectivePolygonIndex = 0;
                    for (int j = 0; j < polygonLength; j++)
                    {
                        short pIndex = polygons[polygonPosition + j];
                        if (pIndex == 0)
                        {
                            int size = polygons[polygonPosition + j + 1];

                            CPNGuide[] guide = new CPNGuide[size];
                            bool[] direct = new bool[size];
                                 
                            for (int l = 0; l < size; l++) {
                                int subPIndex = polygons[polygonPosition + j + 2 + l];
                                int edgeIndex = subPIndex > 0 ? subPIndex - 1 : -subPIndex - 1;
                                guide[l] = guides[edgeIndex];
                                direct[l] = subPIndex > 0; 
                            } 
                            CPNSideEdge sideEdge = new CPNSideEdge();
                            sideEdge.Set(guide, direct);
                            iEdges[effectivePolygonIndex] = sideEdge;

                            j += (1 + polygons[polygonPosition + j + 1]);
                        }
                        else
                        {
                            int edgeIndex = pIndex > 0 ? pIndex - 1 : -pIndex - 1;
                            bool direct = pIndex > 0;
                            CPNSideEdge sideEdge = new CPNSideEdge();
                            sideEdge.Set(guides[edgeIndex],direct);
                            iEdges[effectivePolygonIndex] = sideEdge;
                        }
                        effectivePolygonIndex++;
                    }

                    CPNPolygon polygonData = new CPNPolygon();
                    int id = map == null ? schemas[i] : map.GetMappedInterpolatorId(schemas[i]);
                    polygonData.schemaIndex = manager.GetSchemaIndex(id);
                    polygonsData[k][i] = polygonData; 
                    polygonData.sideEdges = iEdges;
                    polygonData.computeSkip();
                    
                }
                
                for (int i = 0; i < geomPolygonsCount; i++)
                {
                    polygonsProfile[k][ i] = builtTrianglesSize[k] + overallPolygonsPosition;
                    polygonsVerticesProfile[k][i] = builtVerticesSize;

                    CPNPolygon polygonData = polygonsData[k][i];
                    int polygonLength = polygonData.sideEdges.Length;
                    if (polygonLength > 2 && polygonLength < TESSELLATION_PROCESS_NET_INTERPOLATORS
                        && !polygonData.skip)
                    {
                        ICPNetInterpolator netInterpolator = manager.GetSchema(polygonData.schemaIndex).
                            interpolators[polygonLength];
                        netInterpolator.RetrieveInfos(polygonData);
                        builtVerticesSize += netInterpolator.GetComputedInternals();
                        builtTrianglesSize[k] += netInterpolator.GetComputedTriangles();
                    } 
                }
                polygonsProfile[k][geomPolygonsCount] = builtTrianglesSize[k] + overallPolygonsPosition;
                polygonsVerticesProfile[k][geomPolygonsCount] = builtTrianglesSize[k];

                 
                builtTrianglesSize[k] += geometry.GetTrianglesCount();
                builtTrianglesSize[k] += geometry.GetQuadsCount() << 1;

                overallPolygonsPosition = polygonsProfile[k][geomPolygonsCount]; 
            }

            this.tessellationOutput.SetBuiltVerticesSize(builtVerticesSize);

        }

        public void WriteMesh(OutputMesh mesh)
        { 
            int position = WriteVertices(mesh);
                 
            position = WriteEdges(mesh, position);
                 
            WriteEdgesNormals(mesh);
             
            position = WritePolygons(mesh, position); 
        }


        int WriteVertices(OutputMesh mesh){

            int numberOfVertices = subSet==null?curvedPolygonsNet.GetNumberOfVertices():
                subSet.vertices.Length;
             
            Vector3[] vertices = curvedPolygonsNet.GetVertices();
            Vector3[] normals = curvedPolygonsNet.GetNormals();
            Vector3[] uvs = curvedPolygonsNet.GetUv();
            Vector3[][] properties = curvedPolygonsNet.GetProperties3();
            int countProperties = mesh.CountProperties();
            //Vector3[] tangents = curvedPolygonsNet.GetTangents();
            //bool doTangents = tangents != null;
            bool doUvs = uvs != null && mesh.DoUseUVs();
            bool doNormals = normals != null && mesh.DoNormals();

            for (int i = 0; i < numberOfVertices; i++){
                int index = subSet == null ? i : subSet.vertices[i];
                mesh.SetVertex(index, vertices[index]); 
                if (doUvs)
                    mesh.SetUV(index, uvs[index]);
                if (doNormals)
                    mesh.SetNormal(index, normals[index]);
                for (int k = 0; k < countProperties;k++) {
                    mesh.SetProperty3(index, k, properties[k][index]);
                }
            }
            
            
            return numberOfVertices;
        }
        
        int WriteEdges(OutputMesh mesh, int position)
        {
            IGuideModel guideEvaluator = GetGuideModel();
            int edgesSize = subSet == null ? curvedPolygonsNet.GetEdgesCount():subSet.edges.Length; 

            CPNGuide[] guides = this.tessellationOutput.GetGuides();
            int[] edgesProfile = this.tessellationOutput.GetEdgesProfile();

            for (int i = 0; i < edgesSize; i++)
            {
                int index = subSet == null ? i : subSet.edges[i];
                CPNGuide polyline = guides[index];
                if (polyline.GetN() > 0) { 
                    short[] edge = curvedPolygonsNet.GetEdges(/* i */);
                    int edgeLength = curvedPolygonsNet.GetEdgeLength(index); 
                    float[] edgeWeights = curvedPolygonsNet.GetEdgeWeights(/* i */);
                    short[] edgeHints = curvedPolygonsNet.GetEdgeHints(/* i */);
                    
                    int edgePosition = curvedPolygonsNet.GetEdgePosition(index);

                    guideEvaluator.EvaluateEdge(curvedPolygonsNet, mesh, polyline, (short)edgeLength, edge, edgePosition,
                            edgeHints,  edgeWeights,
                            edgesProfile, index); 
                    position += polyline.GetN() - 1;
                }
            }

            edgesSize =  curvedPolygonsNet.GetEdgesCount(); 
            int polylinesSize = subSet == null ? curvedPolygonsNet.GetPolylinesCount() : subSet.polylines.Length;
            
            //No need to compute polylines
            for (int i = 0; i < polylinesSize; i++)
            {
                int index = subSet == null ? i : subSet.edges[i];
                index +=edgesSize;
                CPNGuide polyline = guides[index]; 
                guideEvaluator.EvaluatePolyline(curvedPolygonsNet, mesh, polyline);
                
            }
            return position;
        }

        void WriteEdgesNormals(OutputMesh mesh) {

            IGuideModel guideEvaluator = GetGuideModel();

            CPNGuide[] guides = this.tessellationOutput.GetGuides();
            int edgesSize = subSet == null ? curvedPolygonsNet.GetEdgesCount() : subSet.edges.Length;

            for (int i = 0; i < edgesSize; i++)
            {
                int index = subSet == null ? i : subSet.edges[i];
                if (guides[index].GetN() > 0) {
                    guideEvaluator.EvaluateNormals(mesh, guides[index]);
                }
            } 
        } 

        int WritePolygons(OutputMesh mesh, int position)
        { 
            int geometriesCount = this.curvedPolygonsNet.GetGeometriesCount();
            CPNPolygon[][] polygons = tessellationOutput.GetPolygons();
            int[][] polygonsProfile = tessellationOutput.GetPolygonsProfile();
            int[][] polygonsVerticesProfile = tessellationOutput.GetPolygonsVerticesProfile();

            for (int k = 0; k < geometriesCount; k++) {
                
                mesh.SetGeometry(k); 
                CPNGeometry geometry = this.curvedPolygonsNet.GetGeometries()[k];
                  
                int geomPolygonsCount = subSet==null? geometry.GetPolygonsCount():subSet.polygons[k].Length;
                 
                for (int i = 0; i < geomPolygonsCount; i++)
                {
                    int index = subSet == null ? i : subSet.polygons[k][i];
                    int countEffectiveSize = polygons[k][index].sideEdges.Length;
                    CPNPolygon polygonData = polygons[k][index];
                        
                    if (countEffectiveSize > 2 && countEffectiveSize < TESSELLATION_PROCESS_NET_INTERPOLATORS &&
                        !polygonData.skip)
                    {
                        polygonData.computeSideEdgesSizes();
                        ICPNetInterpolator netInterpolator = manager.GetSchema(polygonData.schemaIndex).
                            interpolators[countEffectiveSize]; 
                        
                        netInterpolator.UdpdateContent(mesh, polygonData, polygonsVerticesProfile[k][index],
                            polygonsProfile[k][index]); 
                          
                    } 
                }
                position += polygonsVerticesProfile[k][geometry.GetPolygonsCount()];

                int triangleIndex = polygonsProfile[k][geometry.GetPolygonsCount()];
                int trianglesCount = geometry.GetTrianglesCount();
                short[] triangles = geometry.GetTriangles();
                for (int i = 0; i < trianglesCount; i++)
                {
                    mesh.WriteTriangle(triangleIndex, triangles[3 * i], triangles[3 * i + 1], triangles[3 * i + 2]);
                    triangleIndex++;
                }
                int quadsCount = geometry.GetQuadsCount();
                short[] quads = geometry.GetQuads();
                for (int i = 0; i < quadsCount; i++)
                {
                    mesh.WriteQuad(triangleIndex, quads[3 * i], quads[3 * i + 1], quads[3 * i + 2], quads[3 * i + 3]);
                    triangleIndex += 2;
                }
            }
             
            return position;
        }

        
    }

}