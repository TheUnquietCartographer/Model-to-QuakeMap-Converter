	public class RectAtlas {
		
	//RECT STRUCT
		public struct Rect {
			public int xPos, yPos, width, height;
			public Rect(int xPos, int yPos, int width, int height) {
				this.xPos = xPos; this.yPos = yPos; this.width = width; this.height = height;
			}
			public int area {get{return width * height;}}
			public int xExt {get{return xPos + width;}}
			public int yExt {get{return yPos + height;}}
			public bool Contains (Rect other) {
				return other.xPos >= this.xPos	&&	other.yPos >= this.yPos	&&	other.xExt <= this.xExt	&&	other.yExt <= this.yExt;
			}
			public bool DoesNotIntersect (Rect other) {
				return this.xPos >= other.xExt ||	this.xExt <= other.xPos	||	this.yPos >= other.yExt	||	this.yExt <= other.yPos;
			}
		}

	//FIELDS
		public int width;
		public int height;
		private readonly List<Rect> freeSpace = new();
		private readonly List<Rect> filledSpace = new();
		public List<Rect> rects {get{return filledSpace;}}

	//CONSTRUCTOR
		public RectAtlas(int width, int height) {
			this.width = width;
			this.height = height;
			freeSpace.Add(new Rect(0, 0, width, height));
		}

	//GETTERS
		

	//FUNCTIONS
		public bool Add(int newRectWidth, int newRectHeight) {
		//Find the best-fitting free rect
			int bestFreeRectIndex_ = -1;
			int bestAreaFit_ = int.MaxValue;
			for (int i = 0; i < freeSpace.Count; i++) {
				if (newRectWidth <= freeSpace[i].width && newRectHeight <= freeSpace[i].height) {
					int areaFit = freeSpace[i].area - newRectWidth * newRectHeight;
					if (areaFit < bestAreaFit_) {
						bestFreeRectIndex_ = i;
						bestAreaFit_ = areaFit;
					}
				}
			}
			if (bestFreeRectIndex_ == -1) return false; //Doesn't fit
			Rect newRect = new Rect(freeSpace[bestFreeRectIndex_].xPos, freeSpace[bestFreeRectIndex_].yPos, newRectWidth, newRectHeight);
		//Split any free rects that overlap with the new rect
			for (int i = freeSpace.Count-1; i > -1; i--) {
				SplitFreeRect(i, newRect);
			}
		//Add the new rect
			filledSpace.Add(newRect);
		//The rects that define the freespace will certainly overlap at this point, so...
			RemoveExtraFreespace();
			return true;
		}
		private void SplitFreeRect(int _freeRectIndex, Rect _newRect) {
			Rect _free = freeSpace[_freeRectIndex];
		//If newRect is OoB
			if (_newRect.DoesNotIntersect(_free)) return;
		//Remove old rect
			freeSpace.RemoveAt(_freeRectIndex);
		//Build new rects
			if (_newRect.yPos > _free.yPos) {
				freeSpace.Add(new Rect(_free.xPos, _free.yPos, _free.width, _newRect.yPos - _free.yPos)); //Top
			}
			if (_newRect.yExt < _free.yExt) {
				freeSpace.Add(new Rect(_free.xPos, _newRect.yExt, _free.width, _free.yExt - _newRect.yExt)); //Bottom
			}
			if (_newRect.xPos > _free.xPos) {
				freeSpace.Add(new Rect(_free.xPos, _free.yPos, _newRect.xPos - _free.xPos, _free.height)); //Left
			}
			if (_newRect.xExt < _free.xExt) {
				freeSpace.Add(new Rect(_newRect.xExt, _free.yPos, _free.xExt - _newRect.xExt, _free.height)); //Right
			}
		}
		private void RemoveExtraFreespace() {
			for (int i = freeSpace.Count-1; i > -1; i--) {
				for (int j = i-1; j > -1; j--) {
					if (freeSpace[j].Contains(freeSpace[i])) {
						freeSpace.RemoveAt(i); break;
					}
					else if (freeSpace[i].Contains(freeSpace[j])) {
						freeSpace.RemoveAt(j); i--;
					}
				}
			}
		}	
	//End RectMap
	}