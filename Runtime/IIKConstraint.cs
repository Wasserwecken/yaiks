
namespace YAIKS
{
    public interface IIKConstraint
    {
        public void SetJoint(IKJoint joint);
        public void Apply();

        #region GIZMOS
#if UNITY_EDITOR
        public void DrawGizmos();
#endif
        #endregion
    }
}