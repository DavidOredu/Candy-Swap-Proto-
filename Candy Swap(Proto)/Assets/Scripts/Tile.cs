using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int positionInGrid;

    public SpriteRenderer tileRenderer;
    public Grid grid;

    [SerializeField]
    private GameObject highlight;

    public SpriteRenderer highlightRenderer;

    
    // Start is called before the first frame update
    void Start()
    {
        highlightRenderer = highlight.GetComponent<SpriteRenderer>();
    }
    private void OnMouseEnter()
    {
        grid.SetCurrentTile(this);
        grid.OnTileEntered();
    }
    
    private void OnMouseUpAsButton()
    {
        grid.SetCurrentTile(this);
        grid.OnTileClicked();
    }
    private void OnMouseExit()
    {
        grid.SetCurrentTile(this);
        grid.OnTileExited();
    }
    public void SetTileAlpha(float alphaValue)
    {
        tileRenderer.color = new Color(highlightRenderer.color.r, highlightRenderer.color.g, highlightRenderer.color.b, alphaValue);
    }
    public void SetTileHighlightAlpha(float alphaValue)
    {
        highlightRenderer.color = new Color(highlightRenderer.color.r, highlightRenderer.color.g, highlightRenderer.color.b, alphaValue);
    }
    public Color GetTileColor()
    {
        return tileRenderer.color;
    }
}
