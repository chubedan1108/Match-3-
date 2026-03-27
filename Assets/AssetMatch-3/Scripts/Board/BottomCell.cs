using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class BottomCell 
{

    private float boardSizeX;

    private float boardSizeY;

    private int m_capacity;

    private Cell[] m_bottomCells;
   
    private Transform m_root;

    private int m_matchMin;

    public BottomCell(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_capacity = gameSettings.BottomSlot;
        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = -m_capacity * 0.5f + 0.5f;
        this.boardSizeY = -gameSettings.BoardSizeY* 0.5f - 1f;

        m_bottomCells = new Cell[gameSettings.BottomSlot];

        CreateBoard();
    }

    private void CreateBoard()
    {
  
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for(int i = 0; i < m_capacity; i++)
        {

            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.SetParent(m_root);
            go.transform.position = new Vector3(boardSizeX + i, boardSizeY, 0);
            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, 0);

            m_bottomCells[i] = cell;

        }

        RemoveCollider();
    }

    private void RemoveCollider()
    {
        for(int i = 0; i < m_capacity; i++)
        {
            m_bottomCells[i].GetComponent<Collider2D>().enabled = false;
        }
    }
   
    //Tim slot trong
    public Cell GetEmptyCell()
    {
        for (int i = 0; i < m_capacity; i++)
        {
            if (m_bottomCells[i].IsEmpty)
            {
                return m_bottomCells[i];
            }
        }
        return null;
    }

    public List<Cell> CheckMatch()
    {
        Dictionary<string, List<Cell>> map = new Dictionary<string, List<Cell>>();

        foreach (var cell in m_bottomCells)
        {
            if (cell.IsEmpty) continue;

            if (cell.Item is NormalItem normalItem)
            {
                string key = normalItem.ItemType.ToString();
                if (!map.ContainsKey(key))
                    map[key] = new List<Cell>();

                map[key].Add(cell);
            } 
        }

        foreach (var pair in map)
        {
            if (pair.Value.Count >= m_matchMin)
            {
                return pair.Value;
            }
        }

        return null;
    }

    public void RemoveMatch(List<Cell> matchCells)
    {
        foreach (var cell in matchCells)
        {
            cell.ExplodeItem();
            cell.Free();
        }
    }

    public void Compact()
    {
        List<Item> items = new List<Item>();

        foreach (var cell in m_bottomCells)
        {
            if (!cell.IsEmpty)
            {
                items.Add(cell.Item);
                cell.Free();
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            m_bottomCells[i].Assign(items[i]);
            items[i].AnimationMoveToPosition();
        }
    }

    public bool IsBottomFull()
    {
        foreach (var cell in m_bottomCells)
        {
            if (cell.IsEmpty)
                return false;
        }
        return true;
    }
}
