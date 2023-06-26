using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerScript : MonoBehaviour {

    // List of different Fruit Icons
    public List<Sprite> cellSprites = new List<Sprite>();

    // Prefab to contain the fruit icon and the properties of a clickable cell
    public GameObject cellPrefab;

    // Properties related to Grid System
    public int gridSize = 8;

    // Gap between cells
    public float gapVal = 1f;

    // 2 dimensional array to deal with the game logic
    private GameObject[,] gameGrid;

    // To decide the end of game
    public int totalMoves = 30;

    public static GameManagerScript Instance { get; private set; }

    void Awake() {
        // Init values
        Instance = this;
        scoreVal = 0;
        numOfMoves = totalMoves;
    }

    void Start() {
        // Init Views

        gameGrid = new GameObject[gridSize, gridSize];

        createGameGrid();
    }

    // To create the Grid view by randmizing the Cells calculating the Constraits ie. Initally no 3 cells should be together.
    void createGameGrid() {

        Vector3 positionOffset = transform.position - new Vector3(gridSize * gapVal / 2.0f, gridSize * gapVal / 2.0f, 0);

        for (int row = 0; row < gridSize; row++) {
            for (int column = 0; column < gridSize; column++) {
                GameObject newCell = Instantiate(cellPrefab);

                List<Sprite> possibleCellSprites = new List<Sprite>(cellSprites);

                // Which Sprite to use for this cell
                Sprite left1 = getCellSpriteAt(column - 1, row);
                Sprite left2 = getCellSpriteAt(column - 2, row);
                if (left2 != null && left1 == left2) {
                    possibleCellSprites.Remove(left1);
                }

                Sprite down1 = getCellSpriteAt(column, row - 1);
                Sprite down2 = getCellSpriteAt(column, row - 2);
                if (down2 != null && down1 == down2){
                    possibleCellSprites.Remove(down1);
                }

                SpriteRenderer sRenderer = newCell.GetComponent<SpriteRenderer>();
                sRenderer.sprite = possibleCellSprites[Random.Range(0, possibleCellSprites.Count)];

                Cell cell = newCell.AddComponent<Cell>();
                cell.vector2IntPosVal = new Vector2Int(column, row);

                newCell.transform.parent = transform;
                newCell.transform.position = new Vector3(column * gapVal, row * gapVal, 0) + positionOffset;
                
                gameGrid[column, row] = newCell;
            }
        }
    }

    public void generateNewLevel() {

        for (int row = 0; row < gridSize; row++) {

            for (int column = 0; column < gridSize; column++) {

                List<Sprite> possibleCellSprites = new List<Sprite>(cellSprites);

                // Which Sprite to use for this cell
                Sprite left1 = getCellSpriteAt(column - 1, row);
                Sprite left2 = getCellSpriteAt(column - 2, row);
                if (left2 != null && left1 == left2){
                    possibleCellSprites.Remove(left1);
                }

                Sprite down1 = getCellSpriteAt(column, row - 1);
                Sprite down2 = getCellSpriteAt(column, row - 2);
                if (down2 != null && down1 == down2){
                    possibleCellSprites.Remove(down1);
                }

                GameObject newCell = gameGrid[column, row];
                SpriteRenderer sRenderer = newCell.GetComponent<SpriteRenderer>();
                sRenderer.sprite = possibleCellSprites[Random.Range(0, possibleCellSprites.Count)];
            }
        }
    }

    public void swapCells(Vector2Int cell1Pos, Vector2Int cell2Pos) {

        GameObject cell1 = gameGrid[cell1Pos.x, cell1Pos.y];
        SpriteRenderer sRenderer1 = cell1.GetComponent<SpriteRenderer>();
        
        GameObject cell2 = gameGrid[cell2Pos.x, cell2Pos.y];
        SpriteRenderer sRenderer2 = cell2.GetComponent<SpriteRenderer>();

        Sprite temp = sRenderer1.sprite;
        sRenderer1.sprite = sRenderer2.sprite;
        sRenderer2.sprite = temp;

        bool isChanging = findMatches();

        if(!isChanging) {

            temp = sRenderer1.sprite;
            sRenderer1.sprite = sRenderer2.sprite;
            sRenderer2.sprite = temp;

            Debug.Log("Moved");
        } else {

            Debug.Log("Poped");

            numOfMoves--;

            do{

                fillEmptyCells();

            } while (findMatches());

            if (numOfMoves <= 0){
                numOfMoves = 0;
                endGame();
            }
        }
    }

    private bool findMatches() {

        HashSet<SpriteRenderer> matchedCells = new HashSet<SpriteRenderer>();

        for (int row = 0; row < gridSize; row++) {

            for (int column = 0; column < gridSize; column++) {

                SpriteRenderer currentSr = getCellSpriteRendererAt(column, row);

                // Horizontal
                List<SpriteRenderer> hMatches = searchForColumnMatch(column, row, currentSr.sprite);
                if (hMatches.Count >= 2) {

                    matchedCells.UnionWith(hMatches);
                    matchedCells.Add(currentSr);
                }

                // Vertical
                List<SpriteRenderer> vMatches = searchForRowMatch(column, row, currentSr.sprite);
                if (vMatches.Count >= 2) {

                    matchedCells.UnionWith(vMatches);
                    matchedCells.Add(currentSr);
                }
            }
        }

        foreach (SpriteRenderer renderer in matchedCells) {
            renderer.sprite = null;
        }

        scoreVal += matchedCells.Count;
        return matchedCells.Count > 0;
    }

    private List<SpriteRenderer> searchForColumnMatch(int col, int row, Sprite sprite) {

        List<SpriteRenderer> result = new List<SpriteRenderer>();

        for (int i = col + 1; i < gridSize; i++){

            SpriteRenderer nextColumn = getCellSpriteRendererAt(i, row);

            if (nextColumn.sprite != sprite) {
                break;
            }

            result.Add(nextColumn);
        }

        return result;
    }

    private List<SpriteRenderer> searchForRowMatch(int col, int row, Sprite sprite) {

        List<SpriteRenderer> result = new List<SpriteRenderer>();

        for (int i = row + 1; i < gridSize; i++){

            SpriteRenderer nextRow = getCellSpriteRendererAt(col, i);

            if (nextRow.sprite != sprite){
                break;
            }

            result.Add(nextRow);
        }

        return result;
    }

    private void fillEmptyCells() {

        for (int column = 0; column < gridSize; column++) {

            for (int row = 0; row < gridSize; row++) {

                while (getCellSpriteRendererAt(column, row).sprite == null) {

                    SpriteRenderer current = getCellSpriteRendererAt(column, row);

                    SpriteRenderer next = current;

                    for (int filler = row; filler < gridSize - 1; filler++) {

                        next = getCellSpriteRendererAt(column, filler + 1);
                        current.sprite = next.sprite;
                        current = next;
                    }

                    next.sprite = cellSprites[Random.Range(0, cellSprites.Count)];
                }
            }
        }
    }

    private void endGame() {

        Debug.Log("Game Ended!");
    }

    // Utility methods to access the sprite / renderer from the grid
    private Sprite getCellSpriteAt(int column, int row) {

        SpriteRenderer renderer = getCellSpriteRendererAt(column, row);

        return renderer?.sprite;
    }

    private SpriteRenderer getCellSpriteRendererAt(int column, int row) {
        if (column < 0 || column >= gridSize || row < 0 || row >= gridSize){
            return null;
        }
            
        GameObject tile = gameGrid[column, row];
        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        return renderer;
    }

    private int _numberOfMoves;
    public int numOfMoves {
        get {
            return _numberOfMoves;
        }

        set {
            _numberOfMoves = value;
            Debug.Log("Number of Moves = " + numOfMoves);
        }
    }

    private int _score;
    public int scoreVal {
        get {
            return _score;
        }

        set {
            _score = value;
            Debug.Log("Score = " + _score);
        }
    }
}
