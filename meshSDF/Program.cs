using g3;

namespace meshSDF
{
    internal class Program
    {

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Please insert the input file path");
                return;
            }

            string inputFileName = args[0];
            if (!inputFileName.ToLower().Contains("stl"))
            {
                Console.WriteLine("Please input an stl file");
                return;
            }


            DMesh3 mesh = StandardMeshReader.ReadMesh(inputFileName);

            Console.WriteLine("Finished reading mesh with " + mesh.VertexCount.ToString() + " vertices and " + mesh.TriangleCount.ToString() + " faces");

            int num_cells = 30;
            double cell_size = mesh.CachedBounds.MaxDim / num_cells;

            MeshSignedDistanceGrid sdf = new MeshSignedDistanceGrid(mesh, cell_size);
            sdf.ExpandBounds = mesh.CachedBounds.Diagonal * 0.25;        // need some values outside mesh
            sdf.ComputeMode = MeshSignedDistanceGrid.ComputeModes.FullGrid;

            var nijk = sdf.Compute();

            Console.WriteLine("Finished computing the signed distance ");

            //WriteImplicitGridToFile
            string[] lines = new string[4];
            lines[0] = nijk.x.ToString() + " " + nijk.y.ToString() + " " + nijk.z.ToString();
            lines[1] = cell_size.ToString() + " " + cell_size.ToString() + " " + cell_size.ToString();
            lines[2] = sdf.GridOrigin.x.ToString() + " " + sdf.GridOrigin.y.ToString() + " " + sdf.GridOrigin.z.ToString() 
                + " " + sdf.ComputedMax.x.ToString() + " " + sdf.ComputedMax.y.ToString() + " " + sdf.ComputedMax.z.ToString();
            string distancesStr = "";
            var buffer = sdf.Grid.Buffer;
            Console.WriteLine("build the implicit grid string with " + buffer.Length.ToString() + " items");

            for (int i = 0; i < buffer.Length; i++)
            {
                if (i == 0)
                    distancesStr += buffer[i].ToString("0.000");
                else
                    distancesStr += " " + buffer[i].ToString("0.000");
            } 
            lines[3] = distancesStr;

            Console.WriteLine("writing the implicit grid to a txt file");

            var ouptutfilename = inputFileName.Replace("stl", "txt");

            File.WriteAllLines(ouptutfilename, lines);

            //var iso = new DenseGridTrilinearImplicit(sdf.Grid, sdf.GridOrigin, sdf.CellSize);
            //MarchingCubes c = new MarchingCubes();
            //c.Implicit = iso;
            //c.Bounds = mesh.CachedBounds;
            //c.CubeSize = c.Bounds.MaxDim / 128;
            //c.Bounds.Expand(3 * c.CubeSize);
            //c.Generate();
            //DMesh3 outputMesh = c.Mesh;
            //StandardMeshWriter.WriteMesh("bunnyImplicit.obj", c.Mesh, WriteOptions.Defaults);

        }
    }
}