using UnityEngine;

public class Cell : MonoBehaviour {
    
    private SpriteRenderer cellView;

    private void Start() {
        cellView = GetComponent<SpriteRenderer>();
    }

    private static Cell selected;

    public Vector2Int vector2IntPosVal;

    // On selection/Click of a Cell
    private void OnMouseDown() {
        if (selected != null) {
            if (selected == this) {
                return;
            }
                
            selected.unSelectCell();

            if (Vector2Int.Distance(selected.vector2IntPosVal, vector2IntPosVal) == 1) {
                GameManagerScript.Instance.swapCells(vector2IntPosVal, selected.vector2IntPosVal);
                selected = null;
            } else {
                
                selected = this;
                selectCell();
            }
        } else {
            
            selected = this;
            selectCell();
        }
    }

    // View changes for the cell on select
    public void selectCell() {
        cellView.color = Color.blue;
    }

    public void unSelectCell() {
        cellView.color = Color.white;
    }
}
