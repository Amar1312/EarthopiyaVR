using UnityEngine;

namespace WPM {

    public struct Vector3d {
        public double x, y, z;

        public Vector3d (double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
        public Vector3d (Vector3 v) { this.x = v.x; this.y = v.y; this.z = v.z; }

        public static Vector3d zero => new Vector3d(0, 0, 0);
        public static Vector3d one => new Vector3d(1, 1, 1);
        public static Vector3d up => new Vector3d(0, 1, 0);
        public static Vector3d forward => new Vector3d(0, 0, 1);
        public static Vector3d right => new Vector3d(1, 0, 0);

        public double magnitude => System.Math.Sqrt(x * x + y * y + z * z);
        public double sqrMagnitude => x * x + y * y + z * z;

        public Vector3d normalized {
            get {
                double mag = magnitude;
                if (mag > 1e-15) // Use a small epsilon for comparison
                    return new Vector3d(x / mag, y / mag, z / mag);
                return zero;
            }
        }

        public Vector3 ToVector3 () => new Vector3((float)x, (float)y, (float)z);

        public static Vector3d operator + (Vector3d a, Vector3d b) => new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3d operator - (Vector3d a, Vector3d b) => new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3d operator - (Vector3d a) => new Vector3d(-a.x, -a.y, -a.z);
        public static Vector3d operator * (Vector3d a, double d) => new Vector3d(a.x * d, a.y * d, a.z * d);
        public static Vector3d operator * (double d, Vector3d a) => new Vector3d(a.x * d, a.y * d, a.z * d);
        public static Vector3d operator / (Vector3d a, double d) => new Vector3d(a.x / d, a.y / d, a.z / d);

        public static double Dot (Vector3d a, Vector3d b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3d Cross (Vector3d a, Vector3d b) => new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

        // Rotate a vector by a quaternion
        public static Vector3d operator * (QuaternionD q, Vector3d v) {
            Vector3d u = new Vector3d(q.x, q.y, q.z);
            double s = q.w;
            return 2.0 * Dot(u, v) * u
                 + (s * s - Dot(u, u)) * v
                 + 2.0 * s * Cross(u, v);
        }
        public override string ToString () => $"({x:F3}, {y:F3}, {z:F3})";

    }

    public struct QuaternionD {
        public double x, y, z, w;

        public QuaternionD (double x, double y, double z, double w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public QuaternionD (Quaternion q) { this.x = q.x; this.y = q.y; this.z = q.z; this.w = q.w; }

        public static QuaternionD identity => new QuaternionD(0, 0, 0, 1);

        public void Normalize () {
            double mag = System.Math.Sqrt(x * x + y * y + z * z + w * w);
            if (mag > 1e-15) // Use a small epsilon
            {
                x /= mag; y /= mag; z /= mag; w /= mag;
            } else {
                // This case should ideally not be reached with valid rotations.
                // If it is, it implies a zero quaternion, which is not a valid rotation.
                // Reset to identity or handle as an error.
                this = identity;
            }
        }
        public QuaternionD normalized {
            get {
                QuaternionD q = this;
                q.Normalize();
                return q;
            }
        }


        public static QuaternionD AngleAxis (double angleDegrees, Vector3d axis) {
            if (axis.sqrMagnitude == 0.0)
                return identity;

            double rad = angleDegrees * (System.Math.PI / 180.0) * 0.5;
            axis = axis.normalized; // Ensure axis is normalized
            double s = System.Math.Sin(rad);
            return new QuaternionD(axis.x * s, axis.y * s, axis.z * s, System.Math.Cos(rad));
        }

        /// <summary>
        /// Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).
        /// This matches Unity's Quaternion.Euler(x, y, z) convention (pitch, yaw, roll).
        /// </summary>
        public static QuaternionD Euler (double x, double y, double z) {
            double xRad = x * (System.Math.PI / 180.0) * 0.5;
            double yRad = y * (System.Math.PI / 180.0) * 0.5;
            double zRad = z * (System.Math.PI / 180.0) * 0.5;

            double cx = System.Math.Cos(xRad);
            double sx = System.Math.Sin(xRad);
            double cy = System.Math.Cos(yRad);
            double sy = System.Math.Sin(yRad);
            double cz = System.Math.Cos(zRad);
            double sz = System.Math.Sin(zRad);

            return new QuaternionD(
                sx * cy * cz - cx * sy * sz, // qx
                cx * sy * cz + sx * cy * sz, // qy
                cx * cy * sz - sx * sy * cz, // qz
                cx * cy * cz + sx * sy * sz  // qw
            );
        }

        /// <summary>
        /// Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).
        /// </summary>
        public static QuaternionD Euler (Vector3d eulerAngles) {
            return Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }

        public static QuaternionD operator * (QuaternionD lhs, QuaternionD rhs) {
            return new QuaternionD(
                lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x,
                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public Quaternion ToQuaternion () => new Quaternion((float)x, (float)y, (float)z, (float)w);
        public override string ToString () => $"({x:F3}, {y:F3}, {z:F3}, {w:F3})";
    }


    public interface IHighPrecisionTransform {
        Quaternion rotation { get; set; }
        Quaternion localRotation { get; set; }
        void Rotate (Vector3 axis, float angleDegrees, Space relativeTo = Space.Self);
        void Rotate (Vector3 eulers);
        void RefreshFromUnityTransform ();
    }

    public class HighPrecisionTransform : IHighPrecisionTransform {

        private Transform unityTransform;
        QuaternionD rotationD;

        public Quaternion rotation {
            get {
                return new Quaternion((float)rotationD.x, (float)rotationD.y, (float)rotationD.z, (float)rotationD.w);
            }
            set {
                unityTransform.rotation = value;
                rotationD = new QuaternionD(value);
            }
        }

        public Quaternion localRotation {
            get {
                return unityTransform.localRotation;
            }
            set {
                unityTransform.localRotation = value;
                rotationD = new QuaternionD(value);
            }
        }

        public HighPrecisionTransform (Transform target) {
            unityTransform = target;
            RefreshFromUnityTransform();
        }

        /// <summary>
        /// Reads the current state from the Unity Transform.
        /// </summary>
        public void RefreshFromUnityTransform () {
            rotationD = new QuaternionD(unityTransform.rotation);
        }

        /// <summary>
        /// Rotates the transform.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angleDegrees">The angle in degrees.</param>
        /// <param name="relativeTo">Whether to rotate in world or self space.</param>
        public void Rotate (Vector3 axis, float angleDegrees, Space relativeTo = Space.Self) {
            QuaternionD rotationDelta = QuaternionD.AngleAxis(angleDegrees, new Vector3d(axis));
            if (relativeTo == Space.World) {
                rotationD = rotationDelta * rotationD;
            } else { // Space.Self
                rotationD = rotationD * rotationDelta;
            }
            rotationD.Normalize();
            unityTransform.rotation = rotationD.ToQuaternion();
        }

        public void Rotate (Vector3 eulers) {
            Vector3d eulerAnglesD = new Vector3d(eulers);
            QuaternionD rotationDelta = QuaternionD.Euler(eulerAnglesD);
            rotationD = rotationD * rotationDelta;
            rotationD.Normalize();
            unityTransform.rotation = rotationD.ToQuaternion();
        }

    }

    public class NormalPrecisionTransform : IHighPrecisionTransform {

        private Transform unityTransform;

        public Quaternion rotation {
            get {
                return unityTransform.rotation;
            }
            set {
                unityTransform.rotation = value;
            }
        }

        public Quaternion localRotation {
            get {
                return unityTransform.localRotation;
            }
            set {
                unityTransform.localRotation = value;
            }
        }

        public NormalPrecisionTransform (Transform target) {
            unityTransform = target;
        }

        /// <summary>
        /// Reads the current state from the Unity Transform.
        /// </summary>
        public void RefreshFromUnityTransform () {
        }


        /// <summary>
        /// Rotates the transform.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angleDegrees">The angle in degrees.</param>
        /// <param name="relativeTo">Whether to rotate in world or self space.</param>
        public void Rotate (Vector3 axis, float angleDegrees, Space relativeTo = Space.Self) {
            unityTransform.Rotate(axis, angleDegrees, relativeTo);
        }

        public void Rotate (Vector3 eulers) {
            unityTransform.Rotate(eulers);
        }

    }

}