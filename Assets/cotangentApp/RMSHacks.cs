using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static class RMSHacks
    {
 



        public static void TestMarchingCubes()
        {
            MarchingCubes mc = new MarchingCubes();

            LocalProfiler p = new LocalProfiler();
            p.Start("GENERATE");
            mc.ParallelCompute = true;
            mc.Generate();
            p.Stop("GENERATE");
            DebugUtil.Log(2, p.AllTimes());

            MeshNormals.QuickCompute(mc.Mesh);

            DebugUtil.WriteDebugMesh(mc.Mesh, "c:\\scratch\\MARCHING_CUBES.obj");

            DMeshSO meshSO = new DMeshSO();
            meshSO.Create(mc.Mesh, CC.ActiveScene.DefaultMeshSOMaterial);
            CC.ActiveScene.AddSceneObject(meshSO, false);
        }









        class SparseGridBit : IGridElement3
        {
            public bool value;
            public IGridElement3 CreateNewGridElement(bool bCopy) {
                SparseGridBit c = new SparseGridBit();
                if (bCopy)
                    c.value = value;
                return c;
            }
        }


//        public static void VoxelHack()
//        {
//            float voxelSize = 1.9f;

//            DMeshSO meshSO = CC.ActiveScene.FindSceneObjectsOfType<DMeshSO>()[0];
//            Frame3f frameL = meshSO.GetLocalFrame(CoordSpace.ObjectCoords);
//            Vector3f scale = meshSO.GetLocalScale();
//            AxisAlignedBox3f aabb_local = meshSO.GetLocalBoundingBox();
//            Box3f box_local = new Box3f(aabb_local.Center, aabb_local.Extents);
//            // [RMS] GetLocalBoundingBox() is already scaled...??
//            //box_local.Scale(scale);
//            Box3f box_scene = frameL.FromFrame(ref box_local);
//            AxisAlignedBox3f bounds = box_scene.ToAABB();

//            Vector3d base_pt = bounds.Center - bounds.Height * 0.5 * Vector3d.AxisY;
//            base_pt.y += voxelSize * 0.5f;      // VoxelSurfaceGenerator cells are centered on grid coords, not aligned w/ grid faces 

//            // construct coordinate mapper between scene coords and full-res grid-cell indices
//            FrameGridIndexer3 GridMap = new FrameGridIndexer3() {
//                GridFrame = new Frame3f(base_pt),
//                CellSize = voxelSize * Vector3f.One
//            };


//            // construct a sparse grid that provides blocks of bitmaps
//            Vector3i block_size = new Vector3i(8, 8, 8);
//            BiGrid3<Bitmap3> bigrid = new BiGrid3<Bitmap3>(new Bitmap3(block_size));

//            // index space that we want to check
//            bounds.Expand(2*voxelSize);
//            AxisAlignedBox3i cell_range = new AxisAlignedBox3i(
//                GridMap.ToGrid(bounds.Min), GridMap.ToGrid(bounds.Max));

//            // Iterate over full-res grid index space, mapping to scene coords
//            // and then into mesh coords, to check if we are inside mesh.
//            // If so, map into sub-block of multiscale grid and set as occupied
//            Action<Bitmap3, Vector3i> updateF = (bmp, local_idx) => { bmp.Set(local_idx, true); };
//            foreach ( Vector3i idx in cell_range.IndicesInclusive() ) {
//                Vector3d pt = GridMap.FromGrid(idx);
//                pt = frameL.ToFrameP(pt);
//                pt /= scale;
//                if (meshSO.Spatial.IsInside(pt)) {
//                    bigrid.Update(idx, updateF);
//                }
//            }


//            // iterate over allocated sub-blocks, generate mesh and
//            // transform back to scene coords
//            foreach ( var pair in bigrid.AllocatedBlocks() ) { 
//                Vector3i block_idx = pair.Key;
//                Bitmap3 bits = pair.Value;

//                // this is index of min-corner of block in full-res space
//                Vector3i block_offset = bigrid.Indexer.FromBlock(block_idx);

//                VoxelSurfaceGenerator gen = new VoxelSurfaceGenerator() {
//                    Voxels = bits, Clockwise = false, CapAtBoundary = true
//                };
//                gen.Generate();
//                foreach (DMesh3 mesh in gen.Meshes) {

//                    // mesh coords are local to sub-grid-block index space, so
//                    // we have to map to full-res grid space, and then back to scene coords
//                    foreach (int vid in mesh.VertexIndices()) {
//                        Vector3d v = mesh.GetVertex(vid);       
//                        v += (Vector3d)block_offset;
//                        v = GridMap.FromGrid(v);
//                        mesh.SetVertex(vid, v);
//                    }

//                    DMeshSO voxelSO = new DMeshSO();
//                    voxelSO.Create(mesh, CC.ActiveScene.DefaultMeshSOMaterial);
//                    CC.ActiveScene.AddSceneObject(voxelSO, false);
//                }

//            }



//#if false
//            // transfer to a single bitmap

//            Bitmap3d bitmap = new Bitmap3d(sparse_grid.Dimensions * block_size);
//            Vector3i min_corner = BlockMap.BlockToWorld(sparse_grid.BoundsInclusive.Min);

//            foreach ( var pair in sparse_grid.Allocated()) {
//                Vector3i block_idx = pair.Key;
//                Bitmap3d bits = pair.Value;
//                Vector3i block_offset = BlockMap.BlockToWorld(block_idx) - min_corner;
//                foreach ( Vector3i local_idx in bits.NonZeros() ) {
//                    bitmap.Set(block_offset + local_idx, true);
//                }
//            }


//            VoxelSurfaceGenerator gen = new VoxelSurfaceGenerator() {
//                Voxels = bitmap, Clockwise = false
//            };
//            gen.Generate();
//            foreach (DMesh3 mesh in gen.Meshes) {

//                foreach (int vid in mesh.VertexIndices()) {
//                    Vector3d v = mesh.GetVertex(vid);
//                    v += (Vector3d)min_corner;
//                    v = GridMap.FromGrid(v);
//                    mesh.SetVertex(vid, v);
//                }

//                DMeshSO voxelSO = new DMeshSO();
//                voxelSO.Create(mesh, CotangentUI.ActiveScene.DefaultMeshSOMaterial);
//                CotangentUI.ActiveScene.AddSceneObject(voxelSO, false);
//            }
//#endif



//        }
    }
}
