using System;
using f3;

namespace cotangent
{
    public static class CotangentTypes
    {
        public const string SlicePlaneHeightGizmoType = "slice_height";



        static readonly public SOType PrintMesh =
            new SOType("PrintMeshSO", Type.GetType("cotangent.PrintMeshSO"));


        static readonly public SOType Toolpath =
            new SOType("ToolpathSO", Type.GetType("cotangent.ToolpathSO"));


        static readonly public SOType SlicePlane =
            new SOType("SlicePlaneHeightSO", Type.GetType("cotangent.SlicePlaneHeightSO"));



        public static void RegisterCotangentTypes(SORegistry registry)
        {
            registry.RegisterType(PrintMesh, PrintMeshSO_Serialization.Emit, PrintMeshSO_Serialization.Build);
        }


    }
}
