using System;

	public static class Settings {

	//SETTING STRUCT & DEFAULT SETTINGS
		public struct Set {

		//CONSTRUCTOR
			public Set () {}

		//.3DO input
			public bool useFaceNormals;
			public bool normalizeTextureVertices = false;

		//MaterialDirectory (uses data from an instance of the Input class to find a directory. Materials can either be in the same directory as the input file or in a subdirectory with the same name as the file.)
			public bool usingSubdirectory = true;


		//
		//	OUTPUT
		//
		//Output modifiers (used by the functions in the Output namespace to modify values from a UniversalMesh and write them to an output file.)
			public double scaleFactor;
			public double rounding;
			public bool reverseVertexOrder;
			public bool invertFaceNormals;
			public bool trianglifyFaces = true;
			public bool swapYZCoordinates;

		//.MAP output
			public int brushThickness;
			private Output.Map.Format format_ = Output.Map.Format.Valve220;
			public Output.Map.Format format {
				get{return format_;}
				set{
					Log.Single("Forcing Valve220 format because it is the only one that is supported!", ConsoleColor.Yellow);
					format_ = Output.Map.Format.Valve220; //Force Valve220 format for now.
				}
			}
			public bool bakeAxes;
		
		//QMDL output
			public bool isAnimated; //If the mesh has multiple materials, tell the QMDL to animate them. If not, they will be composited to a single texture atlas. 
			public int atlas_maxWidth = 256;
			public int atlas_maxHeight = 256;
		}



	//CURRENT (USER) SETTINGS
		public static Set current = new Set();



	//SETTING PRESETS
		public static class Presets {

			public readonly static Set SWJK = new Set {
				useFaceNormals = true,
				normalizeTextureVertices = false
			};
						
		}

	}

