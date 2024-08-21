namespace Network
{
    public struct TransformSnapshot : ISnapshot
    {
        public double remoteTime { get; set; }
        public double localTime { get; set; }
        public Network.Vector3 position;
        public Network.Quaternion rotation;
        public Network.Vector3 scale;

        public TransformSnapshot(double remoteTime, double localTime, Network.Vector3 position,
            Network.Quaternion rotation, Network.Vector3 scale)
        {
            this.remoteTime = remoteTime;
            this.localTime = localTime;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override string ToString() =>
            $"TransformSnapshot(remoteTime={remoteTime:F2}, localTime={localTime:F2}, pos={position}, rot={rotation}, scale={scale})";

        public static TransformSnapshot Interpolate(TransformSnapshot from, TransformSnapshot to, double t)
        {
            return new TransformSnapshot(
                // interpolated snapshot is applied directly. don't need timestamps.
                0, 0,
                // lerp position/rotation/scale unclamped in case we ever need
                // to extrapolate. atm SnapshotInterpolation never does.
                Vector3.LerpUnclamped(from.position, to.position, (float)t),
                // IMPORTANT: LerpUnclamped(0, 60, 1.5) extrapolates to ~86.
                //            SlerpUnclamped(0, 60, 1.5) extrapolates to 90!
                //            (0, 90, 1.5) is even worse. for Lerp.
                //            => Slerp works way better for our euler angles.
                Quaternion.SlerpUnclamped(from.rotation, to.rotation, (float)t),
                Vector3.LerpUnclamped(from.scale, to.scale, (float)t)
            );
        }
    }
}