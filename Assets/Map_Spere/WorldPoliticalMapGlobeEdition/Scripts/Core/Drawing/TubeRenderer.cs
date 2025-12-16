using UnityEngine;
using UnityEngine.Serialization;

namespace WPM {
    [ExecuteInEditMode]
    public class TubeRenderer : MonoBehaviour {
        public int positionCount;
        public Vector3[] positions;
        public int sides;
        public float startWidth;
        public float endWidth;
        public bool useWorldSpace = true;

        Vector3[] vertices;
        int[] indices;
        int indicesCount;
        Vector2[] uvs;
        int uvsCount;

        Mesh mesh;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        bool needsGenerateMesh;
        Vector3[] circle;
        Vector2[] circleTri;

        public Material material {
            get { return meshRenderer.material; }
            set { meshRenderer.material = value; }
        }

        void Awake() {
            if (!TryGetComponent<MeshFilter>(out meshFilter)) {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (!TryGetComponent<MeshRenderer>(out meshRenderer)) {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            mesh = new Mesh();
            meshFilter.mesh = mesh;

            needsGenerateMesh = true;
        }

        void OnEnable() {
            meshRenderer.enabled = true;
        }

        void OnDisable() {
            meshRenderer.enabled = false;
        }

        void LateUpdate() {
            if (needsGenerateMesh) {
                needsGenerateMesh = false;
                GenerateMesh();
            }
        }

        void OnValidate() {
            needsGenerateMesh = true;
        }

        public void SetPosition(int index, Vector3 position) {
            if (index < 0 || index >= positionCount) return;
            CheckPositions();
            if (positions[index] != position) {
                positions[index] = position;
                needsGenerateMesh = true;
            }
        }

        void CheckPositions() {
            if (positions == null || positions.Length < positionCount) {
                positions = new Vector3[positionCount];
            }
        }

        void GenerateMesh() {
            CheckPositions();
            if (mesh == null || positionCount <= 1) {
                mesh = new Mesh();
                return;
            }

            sides = Mathf.Max(3, sides);

            var verticesLength = sides * positionCount;
            if (vertices == null || vertices.Length < verticesLength) {
                vertices = new Vector3[verticesLength];
            }

            GenerateIndices();
            GenerateUVs();

            if (verticesLength > mesh.vertexCount) {
                mesh.SetVertices(vertices);
                mesh.SetTriangles(indices, 0, indicesCount, 0);
            } else {
                mesh.SetTriangles(indices, 0, indicesCount, 0);
                mesh.SetVertices(vertices);
            }
            mesh.SetUVs(0, uvs, 0, uvsCount);

            int verticesCount = 0;
            for (int i = 0; i < positionCount; i++) {
                CalculateCircle(i);
                foreach (var vertex in circle) {
                    vertices[verticesCount++] = useWorldSpace ? transform.InverseTransformPoint(vertex) : vertex;
                }
            }

            mesh.SetVertices(vertices, 0, verticesCount);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;
        }

        void GenerateUVs() {
            uvsCount = positionCount * sides;
            if (uvs == null || uvs.Length < uvsCount) {
                uvs = new Vector2[uvsCount];
            }

            for (int segment = 0; segment < positionCount; segment++) {
                for (int side = 0; side < sides; side++) {
                    var vertIndex = (segment * sides + side);
                    var u = side / (sides - 1f);
                    var v = segment / (positionCount - 1f);
                    uvs[vertIndex].x = u;
                    uvs[vertIndex].x = v;
                }
            }
        }


        void GenerateIndices() {
            // Two triangles and 3 vertices
            indicesCount = positionCount * sides * 2 * 3;
            if (indices == null || indices.Length < indicesCount) {
                indices = new int[indicesCount];
            }

            var k = 0;
            for (int segment = 1; segment < positionCount; segment++) {
                for (int side = 0; side < sides; side++) {
                    var vertIndex = (segment * sides + side);
                    var prevVertIndex = vertIndex - sides;

                    // Triangle one
                    indices[k++] = prevVertIndex;
                    indices[k++] = (side == sides - 1) ? (vertIndex - (sides - 1)) : (vertIndex + 1);
                    indices[k++] = vertIndex;


                    // Triangle two
                    indices[k++] = (side == sides - 1) ? (prevVertIndex - (sides - 1)) : (prevVertIndex + 1);
                    indices[k++] = (side == sides - 1) ? (vertIndex - (sides - 1)) : (vertIndex + 1);
                    indices[k++] = prevVertIndex;
                }
            }
        }

        Vector3[] CalculateCircle(int index) {
            var dirCount = 0;
            var forward = Vector3.zero;

            // If not first index
            if (index > 0) {
                forward += (positions[index] - positions[index - 1]).normalized;
                dirCount++;
            }

            // If not last index
            if (index < positionCount - 1) {
                forward += (positions[index + 1] - positions[index]).normalized;
                dirCount++;
            }

            // Forward is the average of the connecting edges directions
            forward = (forward / dirCount).normalized;
            var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
            var up = Vector3.Cross(forward, side).normalized;

            if (circle == null || circle.Length < sides) {
                circle = new Vector3[sides];
                circleTri = new Vector2[sides];
                var angle = 0f;
                var angleStep = (2 * Mathf.PI) / sides;
                for (int i = 0; i < sides; i++, angle += angleStep) {
                    circleTri[i].x = Mathf.Cos(angle);
                    circleTri[i].y = Mathf.Sin(angle);
                }
            }

            var t = index / (positionCount - 1f);
            var radius = Mathf.Lerp(startWidth, endWidth, t);

            for (int i = 0; i < sides; i++) {
                var cos = circleTri[i].x;
                var sin = circleTri[i].y;

                circle[i] = positions[index] + side * cos * radius + up * sin * radius;
            }

            return circle;
        }
    }
}