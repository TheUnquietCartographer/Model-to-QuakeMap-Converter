using System;
using System.Collections.Generic;
using System.Linq;

namespace Output {

	internal class QMDL_UVTranslation {

		private struct UVRef {
			public int x, y;
			public int triInd, corner;
			public UVRef (Vector2 _UV, int triInd, int corner) {
				this.x = (int)Math.Round(_UV.x); this.y = (int)Math.Round(_UV.y);
				this.triInd = triInd; this.corner = corner;
			}
		}

		public struct QMDL_UV : IEquatable<QMDL_UV> {
			public int x, y;
			public bool onSeam;
		//Constructor
			public QMDL_UV (int x, int y, bool onSeam) {
				this.x = x; this.y = y;
				this.onSeam = onSeam;
			}
		//Equality checks
			public bool Equals(QMDL_UV other) => x == other.x && y == other.y && onSeam == other.onSeam;
    		public override bool Equals(object? obj) => obj is QMDL_UV other && Equals(other);
    		public override int GetHashCode() => HashCode.Combine(x, y, onSeam);
		}



		public List<QMDL_UV> QMDL_UVs {get; private set;}
		public UniversalMesh.Face[] QMDL_triangles {get; private set;}
		public bool[] QMDL_facesFront {get; private set;}



	//////////////////////////////////////////////////
	//
	//	CONSTRUCTOR
	//
	//////////////////////////////////////////////////
		public QMDL_UVTranslation (UniversalMesh _mesh, int _skinWidth, in UniversalMesh.Face[]? _triangles = null) {
		// ! There is conflicting information as to whether texture vertices are stored as short (2 bytes) or int (4 bytes) - assume int.
		// * Each texture coordinate has an onseam flag, an x, and a y value.
		// * The onseam flag describes a texture vertex that is shared by a more than one triangle, where some of the triangles straddle the seam where the texture would wrap.
		// * * In other words, it allows the QMDL to record backface UVs with a flag, thereby saving space.
		// * * The onseam flag is valid where the backface UV would equal (x+width/2, y), due to the way textures wrap.
		// * * It's not intuitive but don't worry about it.

			if ((_skinWidth & (_skinWidth - 1)) != 0) {
    			throw new ArgumentException("Skin width must be a power-of-two for QMDL export.", nameof(_skinWidth));
			}

		//
		//	HELPER FUNCTIONS
		//

		//Wraps value
			int Wrap(int x) {
				int r = x % _skinWidth;
				if (r < 0) r += _skinWidth;
				return r;
			}

		//Shortest distance on a circular domain
			int WrapDistance(int a, int b) {
				int d = Math.Abs(a - b);
				return Math.Min(d, _skinWidth - d);
			}

		//
		//	COMPUTE QMDL TEXTURE VERTICES
		//
		
		//GET TRIANGLES
			if (_triangles != null) this.QMDL_triangles = _triangles!;
			else this.QMDL_triangles = _mesh.faces.SelectMany(x => _mesh.GetTriangles(x)).ToArray();

		//FOR EACH VERTEX IN THE MESH, GET ALL ASSOCIATED UVs
			List<UVRef>[] UVsPerVertex = new List<UVRef>[_mesh.vertexCount];
			for (int i = 0; i < UVsPerVertex.Length; i++) UVsPerVertex[i] = new List<UVRef>();
			for (int i = 0; i < this.QMDL_triangles.Length; i++) {
				for (int j = 0; j < 3; j++) {
					int vi = this.QMDL_triangles[i].vertexIndices[j];
					int tvi = this.QMDL_triangles[i].textureVertexIndices[j];
					UVsPerVertex[vi].Add(new UVRef(_mesh.textureVertices[tvi], i, j));
				}
			}

		//FOR EACH VERTEX IN THE MESH, SOLVE ONSEAM
		//Check for values that cross the seam
		// * Consider rounding UVs_thisVertex (already reduced from double to float) to eliminate near-duplicates.
		// * * QMDL uses int (texel offsets) so we will need to round anyway.
			this.QMDL_UVs = new List<QMDL_UV>();
			Dictionary<QMDL_UV, int> QMDL_UV_indexLookup = new Dictionary<QMDL_UV, int>();
			for (int i = 0; i < _mesh.vertexCount; i++) {
				List<UVRef> UVs_thisVertex = UVsPerVertex[i];
			//Check Y
				int yRef = UVs_thisVertex[0].y;
				for (int j = 1; j < UVs_thisVertex.Count; j++) {
					if (Math.Abs(UVs_thisVertex[j].y - yRef) != 0) {
						goto NEXT;
					}
				}
			//Check X
				{
					bool SOLVED = false;
					for (int j = 0; j < UVs_thisVertex.Count; j++) {
						int xRef = Wrap(UVs_thisVertex[j].x);
						int backfaceValue = Wrap(xRef + _skinWidth / 2);
						bool allFit = true;
						bool usesBackface = false;
						for (int k = 0; k < UVs_thisVertex.Count; k++) {
							int x = Wrap(UVs_thisVertex[k].x);
							if (WrapDistance(x, xRef) == 0) continue; //Doesn't cross
							else if (WrapDistance(x, backfaceValue) == 0) usesBackface = true; //Does cross
							else {
								allFit = false; //Doesn't fit either; incompatible with MDL seam model.
								break;
							}
						}
						if (!allFit) continue;
					//Determine onSeam requirement (no errors)
						if (allFit) {
							QMDL_UV newUV = new QMDL_UV(xRef, yRef, usesBackface);
							int newUVIndex;
							if (!QMDL_UV_indexLookup.TryGetValue(newUV, out newUVIndex)) {
								newUVIndex = this.QMDL_UVs.Count;
								this.QMDL_UVs.Add(newUV);
								QMDL_UV_indexLookup[newUV] = newUVIndex;
							}
							foreach (UVRef _UV in UVs_thisVertex) {
								this.QMDL_triangles[_UV.triInd].textureVertexIndices[_UV.corner] = newUVIndex;
							}
							SOLVED = true;
							break;
						}
					}
					if (SOLVED) continue;
					else goto NEXT;
				}

			//If onSeam requirement cannot be resolved with a single UV...
				NEXT:
				
			//SINGLE QMDL UV DOES NOT SUFFICE; CLUSTER UVs INSTEAD
				List<List<UVRef>> UVClustersPerVertex = new List<List<UVRef>>();
				foreach (UVRef _UV in UVs_thisVertex) {
					int thisX = Wrap(_UV.x);
					int thisY = _UV.y;
					bool added = false;
				//Add to cluster if within tolerance
					foreach (List<UVRef> UVCluster in UVClustersPerVertex) {
						int clusterX = Wrap(UVCluster[0].x);
						int clusterY = UVCluster[0].y;
						if (WrapDistance(thisX, clusterX) == 0 && Math.Abs(thisY - clusterY) == 0) {
							UVCluster.Add(_UV);
							added = true;
							break;
						}
					}
				//Else create new cluster
					if (!added) {
						UVClustersPerVertex.Add(new List<UVRef>{_UV});
					}
				}

			//FOR EACH CLUSTER, CREATE A QMDL UV (try to match frontface/backface)
				bool[] clusterAssignedToUV = new bool[UVClustersPerVertex.Count];
				for (int j = 0; j < UVClustersPerVertex.Count; j++) {
					if (clusterAssignedToUV[j]) continue;
					List<UVRef> _UVCluster_this = UVClustersPerVertex[j];
					int theseX = Wrap(_UVCluster_this[0].x);
					int theseY = _UVCluster_this[0].y;
				//Check for a cluster which whose x/y values equal (x+width/2, y)
				// * This is a backface and can be assigned to the same QMDL UV, with the onSeam flag.
					int backfacePartnerIndex = -1;
					for (int k = j+1; k < UVClustersPerVertex.Count; k++) {
						if (clusterAssignedToUV[k]) continue;
						List<UVRef> _UVCluster_other = UVClustersPerVertex[k];
						int othersX = Wrap(_UVCluster_other[0].x);
						int othersY = _UVCluster_other[0].y;
					//Check y
						if (Math.Abs(theseY - othersY) != 0) continue;
					//Check x
						int targetValue = Wrap(theseX + _skinWidth / 2);
						if (WrapDistance(othersX, targetValue) == 0) {
							backfacePartnerIndex = k;
							break;
						}
					}

				//ASSIGN CLUSTER(S)
				//Assign cluster to a new QMDL UV index
					QMDL_UV newUV = new QMDL_UV(theseX, theseY, true);
					int newUVIndex;
					if (!QMDL_UV_indexLookup.TryGetValue(newUV, out newUVIndex)) {
						newUVIndex = this.QMDL_UVs.Count;
						this.QMDL_UVs.Add(newUV);
						QMDL_UV_indexLookup[newUV] = newUVIndex;
					}
					foreach (UVRef _UV in _UVCluster_this) {
						this.QMDL_triangles[_UV.triInd].textureVertexIndices[_UV.corner] = newUVIndex;
					}
					clusterAssignedToUV[j] = true;
				//If backface cluster was found, assign that to the same QMDL UV index
					if (backfacePartnerIndex != -1) {
						foreach (UVRef _UV in UVClustersPerVertex[backfacePartnerIndex]) {
							this.QMDL_triangles[_UV.triInd].textureVertexIndices[_UV.corner] = newUVIndex;
						}
						clusterAssignedToUV[backfacePartnerIndex] = true;
					}
				}

				//Written QMDL_UVs
				//Written QMDL_triangles

			//End iterating vertices
			}

		//DETERMINE TRIANGLE FACINGS
			this.QMDL_facesFront = new bool[this.QMDL_triangles.Length];
			for (int i = 0; i < this.QMDL_triangles.Length; i++) {
			//Compare grouping (high score indicates less grouping, greater likelihood of being a backface)
				int Score(int a, int b, int c) {
					return WrapDistance(a,b) + WrapDistance(b,c) + WrapDistance(c,a);
				}
				UniversalMesh.Face tri = this.QMDL_triangles[i];
				QMDL_UV UV0 = this.QMDL_UVs[tri.textureVertexIndices[0]];
				QMDL_UV UV1 = this.QMDL_UVs[tri.textureVertexIndices[1]];
				QMDL_UV UV2 = this.QMDL_UVs[tri.textureVertexIndices[2]];
				int frontScore = Score(
					UV0.x,
					UV1.x,
					UV2.x
				);
				int backScore = Score(
					UV0.onSeam ? Wrap(UV0.x + _skinWidth / 2) : UV0.x,
					UV1.onSeam ? Wrap(UV1.x + _skinWidth / 2) : UV1.x,
					UV2.onSeam ? Wrap(UV2.x + _skinWidth / 2) : UV2.x
				);
				if (frontScore <= backScore) this.QMDL_facesFront[i] = true;
			}

		//Close constructor
		}

	}

}
