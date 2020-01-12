using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLab.ShadowFramework;
using MLab.ShadowFramework.Interpolation;

namespace MLab.ShadowFramework.Interpolation
{

    public class NGonInterpolation : ICPNetInterpolator
    {
        public const int DEFAULT_VERTEX_LAYER = 0;
        
        //sides is clear
        int sides;
        int nTriangles, nInternals, M;

        //parametric coordinates shape parameters
        float[] cos_;
        float[] sin_;
        float[] val;

        float a, b;

        //Used by interpolation
        Vector3[] tmpInterpolations;
        Vector3[] tmpInterpolationsUV;

        float[] cornerCoordsMatrix = new float[4];
        int[] slicePosition;

        NGonsMeshIndicesArray meshIndicesArray = new NGonsMeshIndicesArray();

        private CPNGuideEvaluator evaluator = new CPNGuideEvaluator();
        private InterpolationBuffer[] buffers;
        private InterpolationBuffer[] backBuffers;
        private float step;
        private float relativeTriangleFactorX = 1.0f;
        private float relativeTriangleFactorY = 1.0f;
        private int interpolationSteps = 1;
        private float totalInterpolationStep = 1;

        private InterpolationMemory memory = new InterpolationMemory();
        InterpolationBuffer bufferTemp = new InterpolationBuffer();


        public NGonInterpolation(int sides)
        {
            this.sides = sides;

            this.cos_ = new float[sides];
            this.sin_ = new float[sides];

            //actually : angle dimension for the center piece
            float alpha = 2 * Mathf.PI / sides;

            //O, wow. what is dX2 = 1.5f?
            float dX2 = 1.5f;
            //Cos and sin of angle. Good
            float dx1 = (float)Mathf.Cos(alpha);
            float dy1 = (float)Mathf.Sin(alpha);

            //Need to understand the meaning of a and b
            this.a = 1.0f / (dX2 * dX2);
            this.b = ((1 - a * dx1 * dx1) / (dy1 * dy1));

            //absolutely clear this is
            for (int i = 0; i < sides; i++)
            {
                cos_[i] = (float)Mathf.Cos(-alpha * i);
                sin_[i] = (float)Mathf.Sin(-alpha * i);
            }

            {
                //here we calculate the cornerCoordsMatrix, which is 
                //the reciproc matrix to
                //  | cos_[sides-1] - cos_[0]  sin_[sides-1] - sin_[0]|
                //  | cos_[1] - cos_[0]  sin_[1] - sin_[0]|

                int i = 0;
                int next = 1;
                int prev = sides - 1;
                float a = cos_[prev] - cos_[i];
                float b = sin_[prev] - sin_[i];

                float c = cos_[next] - cos_[i];
                float d = sin_[next] - sin_[i];

                float det = a * d - b * c;
                float recDelta = 1.0f / det;
                cornerCoordsMatrix[0] = d * recDelta;
                cornerCoordsMatrix[1] = -b * recDelta;
                cornerCoordsMatrix[2] = -c * recDelta;
                cornerCoordsMatrix[3] = a * recDelta;

                //wait
                float length1 = (cornerCoordsMatrix[0] * cornerCoordsMatrix[0] + cornerCoordsMatrix[1] * cornerCoordsMatrix[1]);
                float length2 = (cornerCoordsMatrix[2] * cornerCoordsMatrix[2] + cornerCoordsMatrix[3] * cornerCoordsMatrix[3]);

                relativeTriangleFactorX = ( cornerCoordsMatrix[0] * (-1));
                relativeTriangleFactorY = (cornerCoordsMatrix[3] * (-1));
                
            }

            this.val = new float[sides];

            //cornerSurfacesSet.Init(sides);

            tmpInterpolations = new Vector3[sides];
            tmpInterpolationsUV = new Vector3[sides];
            buffers = new InterpolationBuffer[sides];
            backBuffers = new InterpolationBuffer[sides];
            for (int i = 0; i < tmpInterpolations.Length; i++)
            { 
                buffers[i] = new InterpolationBuffer();
                backBuffers[i] = new InterpolationBuffer();
            }

            slicePosition = new int[sides];
        }

        // override
        public void Clean()
        { 
        }

        public int GetComputedInternals()
        {
            return nInternals;
        }

        public int GetComputedTriangles()
        {
            return nTriangles;
        }

        public InterpolationMemory GetMemory()
        {
            return memory;
        }

        public void RetrieveInfos(CPNPolygon buildingPolygonData)
        { 
            this.M = 0;

            int missing = 0;
            for (int i = 0; i < sides; i++) {
                int N = buildingPolygonData.sideEdges[i].GetN();
                if (this.M < N) {
                    this.M = N;
                }
            }
            for (int i = 0; i < sides; i++)
            {
                int N = buildingPolygonData.sideEdges[i].GetN();
                missing += (M - N);
            }
             
            this.nInternals = 1 + sides * (((M) * (M - 1)) >> 1);

            //TODO : I think this is wrong. You miss a triangle when N is less then M, you know?
            this.nTriangles = sides * ((M * M)) - missing;

        }

        public void UdpdateContent(OutputMesh mesh, CPNPolygon buildingPolygonData, int internalsIndex,
            int facesIndex, bool doUpdateStructure = true)
        {

            RetrieveInfos(buildingPolygonData);
             
            this.step = 1.0f / M;
            
            totalInterpolationStep = M / relativeTriangleFactorX;
            interpolationSteps = (int)(totalInterpolationStep);
            if (interpolationSteps * relativeTriangleFactorX < M)
                interpolationSteps++;

            Vector3 vertex = Vector3.zero;
            Vector3 uv = Vector3.zero;

            int centerPiecePosition = 0;

            int verticesLayer = 0;// mesh.FindLayer(BufferType.VERTICES);
            //int normalsLayer = mesh.FindLayer(BufferType.NORMALS);
            //cornerSurfacesSet.SetNormalsLayer(normalsLayer);

            CPNSideEdge[] polylines = buildingPolygonData.sideEdges;

            for (int i = 0; i < sides; i++)
            {
                buffers[i].writeWithGuide(polylines[i], interpolationSteps, step * relativeTriangleFactorX, mesh, evaluator);
                backBuffers[i].writeWithGuideBack(polylines[i], interpolationSteps, step * relativeTriangleFactorX, mesh, evaluator);
            }

            prepareMemory(M, polylines, mesh);

            //int position = internalsIndex;

            int triangleSize = ((M) * (M + 1)) >> 1;

            for (int corner = 0; corner < sides; corner++)
            { 
                int next = corner == sides - 1 ? 0 : corner + 1;

                float p1x = cos_[corner];
                float p2x = cos_[next];
                float p1y = sin_[corner];
                float p2y = sin_[next];
                 
                int relativeMemoryIndex = triangleSize * corner + M;

                for (int i = 1; i <= M - 1; i++)
                {
                    for (int j = 0; j <= M - 1 - i; j++)
                    { 
                        float innerU = step * j;
                        float innerV = step * i;
                        float innerW = 1 - innerU - innerV;

                        float innerX_ = innerW * p1x + innerU * p2x;
                        float innerY_ = innerW * p1y + innerU * p2y;
                        
                        vertex = EvalVertex(mesh, buildingPolygonData, innerX_, innerY_, out uv); 

                        memory.vertices[relativeMemoryIndex] = vertex;
                        memory.uv[relativeMemoryIndex] = uv;
                        relativeMemoryIndex++;
                        //position++;
                    }
                }
            }

            vertex = EvalVertex(mesh, buildingPolygonData, 0, 0,out uv);
            //mesh.SetValue(0, position, vertex);
            memory.vertices[triangleSize * sides] = vertex;
            memory.uv[triangleSize * sides] = uv;
            //position++;




            //Phase 2 Normals and Tangents Evaluation

            int position = internalsIndex;
            
            for (int corner = 0; corner < sides; corner++)
            {
                int prev = corner == 0 ? sides - 1 : corner - 1;
                int next = corner == sides - 1 ? 0 : corner + 1; 
                int relativeMemoryIndex = triangleSize * corner;
                int nextPatchRelativeMemoryIndex = triangleSize * next;
                int prevPatchRelativeMemoryIndex = triangleSize * prev;
                slicePosition[corner] = position;
                for (int i = 1; i <= M - 1; i++)
                {
                    for (int j = 0; j <= M - 1 - i; j++)
                    {
                        int rowIndex = relativeMemoryIndex + M * i - (((i) * (i - 1)) >> 1);
                        int prevRowIndex = relativeMemoryIndex + M * (i - 1) - (((i - 1) * (i - 2)) >> 1);
                        int nextRowIndex = relativeMemoryIndex + M * (i + 1) - (((i +1 ) * (i)) >> 1);

                        int index = rowIndex + j;
                        vertex = memory.vertices[index];
                        uv = memory.uv[index];

                        int duIndexA = 0, duIndexB = 0, dvIndexA = 0, dvIndexB = 0; 
                        if (j > 0 && j < M - 1 - i)
                        {
                            duIndexA = prevRowIndex + j;
                            duIndexB = nextRowIndex + j;
                            dvIndexA = nextRowIndex + j - 1;
                            dvIndexB = prevRowIndex + j + 1;
                        }
                        else if (i < M - 1)
                        {
                            if (j == M - 1 - i)
                            {
                                int nextPatchnextRowIndex = nextPatchRelativeMemoryIndex + M * (i + 1) - (((i + 1) * (i)) >> 1);

                                duIndexA = prevRowIndex + j;
                                duIndexB = nextPatchnextRowIndex;
                                dvIndexA = nextRowIndex + j - 1;
                                dvIndexB = prevRowIndex + j + 1; 
                            }
                            else if (j == 0) {

                                int prevPatchprevRowIndex = prevPatchRelativeMemoryIndex + M * (i - 1) - (((i - 1) * (i - 2)) >> 1);

                                duIndexA = prevRowIndex + j;
                                duIndexB = nextRowIndex + j;
                                dvIndexA = prevPatchprevRowIndex + M - i;
                                dvIndexB = prevRowIndex + j + 1; 
                            }
                        } else {
                            int prevPatchprevRowIndex = prevPatchRelativeMemoryIndex + M * (i - 1) - (((i - 1) * (i - 2)) >> 1);

                            duIndexA = prevRowIndex + j;
                            duIndexB = triangleSize * sides;
                            dvIndexA = prevPatchprevRowIndex + M - i;
                            dvIndexB = prevRowIndex + j + 1; 
                        }
                         

                        Vector3 dSdu = memory.vertices[duIndexB] - memory.vertices[duIndexA];
                        Vector3 dSdv = memory.vertices[dvIndexB] - memory.vertices[dvIndexA];
                        Vector3 normal = Vector3.Cross(dSdu, dSdv).normalized;

                        //Tangent
                        Vector3 dTxdu = memory.uv[duIndexB] - memory.uv[duIndexA];
                        Vector3 dTxdv = memory.uv[dvIndexB] - memory.uv[dvIndexA];
                        Vector3 tangent = getTangent(dSdu, dSdv, dTxdu, dTxdv);

                        mesh.SetPNUV(position, vertex, normal, uv, tangent);
                        
                        //relativeMemoryIndex++;
                        position++;
                    }
                }
            }

            {
                int duIndexA = 2 * triangleSize - 1;
                int duIndexB = 4 * triangleSize - 1;
                int dvIndexA = triangleSize - 1;
                int dvIndexB = 3 * triangleSize - 1;
                vertex = memory.vertices[triangleSize * sides];
                uv = memory.uv[triangleSize * sides];

                Vector3 dSdu = memory.vertices[duIndexB] - memory.vertices[duIndexA];
                Vector3 dSdv = memory.vertices[dvIndexB] - memory.vertices[dvIndexA];
                Vector3 normal = Vector3.Cross(dSdu, dSdv).normalized;

                //Tangent
                Vector3 dTxdu = memory.uv[duIndexB] - memory.uv[duIndexA];
                Vector3 dTxdv = memory.uv[dvIndexB] - memory.uv[dvIndexA];
                Vector3 tangent = getTangent(dSdu, dSdv, dTxdu, dTxdv);
                mesh.SetPNUV(position, vertex, normal, uv, tangent);
            }
            
            centerPiecePosition = position;
            position++;



             



            int trPosition = facesIndex;

            for (int corner = 0; corner < sides; corner++)
            {

                int next = corner == sides - 1 ? 0 : corner + 1;
                
                if (M > 1)
                {

                    int rowPosition1 = slicePosition[corner];
                    int rowPosition2 = slicePosition[corner] + M - 1;
                    int rowPosition1Next = slicePosition[next];
                    int rowPosition2Next = slicePosition[next] + M - 1;
                    for (int i = 0; i < M - 2; i++)
                    {
                        for (int j_ = 0; j_ < M - 3 - i; j_++)
                        {

                            trPosition = mesh.WriteTriangle(trPosition, rowPosition1 + j_, rowPosition2 + j_,rowPosition1 + j_ + 1 );
                            trPosition = mesh.WriteTriangle(trPosition, rowPosition1 + j_ + 1, rowPosition2 + j_, rowPosition2 + j_ + 1 );

                        }
                        int j = M - 3 - i;
                        trPosition = mesh.WriteTriangle(trPosition, rowPosition1 + j,
                            rowPosition2 + j,rowPosition1 + j + 1);

                        trPosition = mesh.WriteTriangle(trPosition, rowPosition2 + j,
                            rowPosition2Next,rowPosition1 + j + 1);

                        trPosition = mesh.WriteTriangle(trPosition, rowPosition1 + j + 1, 
                            rowPosition2Next, rowPosition1Next);

                        rowPosition1 = rowPosition2;
                        rowPosition2 = rowPosition2 + M - 2 - i;
                        rowPosition1Next = rowPosition2Next;
                        rowPosition2Next = rowPosition2Next + M - 2 - i;
                    }
                    trPosition = mesh.WriteTriangle(trPosition, rowPosition1, centerPiecePosition, rowPosition1Next);
                    
                    meshIndicesArray.Setup(slicePosition[corner], 1, M, slicePosition[next], mesh, verticesLayer);

                    int polylineIndex = sides - 1 - corner;

                    NetPolylineIndicesArray npi = new NetPolylineIndicesArray(polylines[polylineIndex], mesh, true);
                    trPosition = MeshStructures.CreateSideTriangles(mesh, npi, meshIndicesArray,
                            trPosition);
                    
                }
                else

                {
                    trPosition = mesh.WriteTriangle(trPosition, polylines[corner].GetIndex(0), centerPiecePosition, polylines[corner].GetBackIndex(0));
                }
            }
             
        }

        /*  (u,v,w): triangle Homogeneous Coordinates, S(u,v) = (x(u,v),y(u,v),z(u,v)) patch model, Tx(u, v) = (s(u,v),t(u,v)) texture coordinates*/
        private Vector3 getTangent(Vector3 dSdu, Vector3 dSdv, Vector3 dTxdu, Vector3 dTxdv)
        {
            //Looking for dSds, where Tx = (s,t), so s is the first coordinate in the Tex Coords Array 
            float det = dTxdu.x * dTxdv.y - dTxdu.y * dTxdv.x;
            Vector3 tangent = (dSdu * dTxdv.y - dSdv * dTxdu.y).normalized;
            return det > 0 ? tangent : -tangent;
        }


        private void prepareMemory(int M, CPNSideEdge[] guides, OutputMesh mesh)
        {
            int triangleSize = ((M) * (M + 1)) >> 1;
            int totalSize = (triangleSize) * sides + 1;

            memory.requestSize(totalSize);

            for (int i = 0; i < sides; i++) {

                int relativePosition = triangleSize * (sides - 1 - i);

                bufferTemp.requestSize(M + 1);
                bufferTemp.writeWithGuideBack(guides[i], M, mesh, evaluator);

                for (int j = 0; j < M; j++)  {
                    memory.vertices[relativePosition + j] = bufferTemp.vertices[j];
                    memory.uv[relativePosition + j] = bufferTemp.uvs[j];
                }
            }
        }

        Vector3 EvalVertex(OutputMesh builder, CPNPolygon polylines, float innerX, float innerY, out Vector3 uv)
        {
            updateVals(innerX, innerY);
            
            for (int k = 0; k < sides; k++) {
                int kprev = k == 0 ? sides - 1 : k - 1;

                float dx = (innerX * cos_[k] - innerY * sin_[k]) - 1;
                float dy = (innerX * sin_[k] + innerY * cos_[k]);

                float U = cornerCoordsMatrix[0] * dx + cornerCoordsMatrix[2] * dy;
                float V = cornerCoordsMatrix[1] * dx + cornerCoordsMatrix[3] * dy;
                 
                int indexU = (int)((U + 0.0001f) * totalInterpolationStep);
                int indexV = (int)((V + 0.0001f) * totalInterpolationStep);

                if (indexU > interpolationSteps)
                    indexU = interpolationSteps;
                if (indexV > interpolationSteps)
                    indexV = interpolationSteps;

                tmpInterpolations[k] = evalVertex(polylines.sideEdges[k], polylines.sideEdges[kprev], U, V);
                tmpInterpolationsUV[k] = evalUV(polylines.sideEdges[k], polylines.sideEdges[kprev], U, V);
            }

            Vector3 vertex = val[0] * tmpInterpolations[0];
            uv = val[0] * tmpInterpolationsUV[0];

            for (int k = 1; k < sides; k++) {
                vertex = vertex + tmpInterpolations[k] * val[k];
                uv = uv + tmpInterpolationsUV[k] * val[k];
            }
             
            return vertex;
        }



        private Vector3 evalVertex(CPNSideEdge sidedEdgeA,
            CPNSideEdge sidedEdgeB, float U, float V)
        {
            //int backBIndex = bufferPrev.N - Bindex;

            evaluator.EvalAt(0, sidedEdgeA);
            Vector3 v0 = evaluator.EvalVertex(sidedEdgeA);
            Vector3 dA0 = evaluator.EvalDev(sidedEdgeA);
            evaluator.EvalAt(U, sidedEdgeA);
            Vector3 vA = evaluator.EvalVertex(sidedEdgeA);
            Vector3 nA = evaluator.EvalNormal(sidedEdgeA);
            evaluator.EvalAt(1 - V, sidedEdgeB);
            Vector3 vB = evaluator.EvalVertex(sidedEdgeB);
            Vector3 nB = evaluator.EvalNormal(sidedEdgeB);
            evaluator.EvalAt(1, sidedEdgeB);
            Vector3 dB0 = -evaluator.EvalDev(sidedEdgeB);

            float kA = -Vector3.Dot(dB0, nA);
            Vector3 part1 = vA + /*sidedEdgeA.guide[0].thickness **/ ((vB - v0) + kA * V * nA);
            
            float kB = -Vector3.Dot(dA0, nB);
            Vector3 part2 = vB + /*sidedEdgeB.guide[0].thickness * */ ((vA - v0) + kB * U * nB);

            float U2 = U * U;
            float V2 = V * V;

            return (part1 * U2 + part2 * V2) / (U2 + V2);

        }



        private Vector3 evalUV(CPNSideEdge sidedEdgeA,
            CPNSideEdge sidedEdgeB, float U, float V)
        {
            //int backBIndex = bufferPrev.N - Bindex;

            evaluator.EvalAt(0, sidedEdgeA);
            Vector3 v0 = evaluator.EvalUV(sidedEdgeA); 
            evaluator.EvalAt(U, sidedEdgeA);
            Vector3 vA = evaluator.EvalUV(sidedEdgeA); 
            evaluator.EvalAt(1 - V, sidedEdgeB);
            Vector3 vB = evaluator.EvalUV(sidedEdgeB); 
            evaluator.EvalAt(1, sidedEdgeB);

            return vB + vA - v0;

        }

        //Vals are the weights of each corner function 
        private void updateVals(float innerX, float innerY)
        {
            float sum = 0;
            for (int k = 0; k < sides; k++)
            {
                float dx = (innerX * cos_[k] - innerY * sin_[k]) - 1;
                float dy = (innerX * sin_[k] + innerY * cos_[k]);
                val[k] = 1 - (a * dx * dx + b * dy * dy);
                if (val[k] < 0)
                    val[k] = 0;
                val[k] = val[k] * val[k] * val[k];
                sum += val[k];
            }
            float rec = 1.0f / sum;
            for (int k = 0; k < sides; k++)
            {
                val[k] *= rec;
            }
        }

    }
} 