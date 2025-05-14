using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public float spacing = 1.1f;
    public GameObject[] blockPrefabs;

    private GameObject[,] grid;
    private Block selectedBlock = null;

    private float xOffset;
    private float yOffset;

    private void Start()
    {
        grid = new GameObject[width, height];
        xOffset = -width / 2f + 0.5f;
        yOffset = -height / 2f + 0.5f;

        GenerateBoard();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.back, Vector3.zero); // Z=0 düzlemi

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                HandleClickAtWorldPosition(hitPoint);
            }
        }

        CheckFall();
    }

    void HandleClickAtWorldPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - xOffset) / spacing);
        int y = Mathf.RoundToInt((worldPos.y - yOffset) / spacing);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            GameObject clickedObj = grid[x, y];
            if (clickedObj != null)
            {
                Block blockScript = clickedObj.GetComponent<Block>();
                BlockClicked(blockScript);
            }
        }
    }

    void GenerateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                List<int> possibleIndexes = new List<int>();
                for (int i = 0; i < blockPrefabs.Length; i++)
                {
                    possibleIndexes.Add(i);
                }

                if (x >= 2)
                {
                    GameObject left1 = grid[x - 1, y];
                    GameObject left2 = grid[x - 2, y];

                    if (left1 != null && left2 != null && left1.name == left2.name)
                    {
                        int removeIndex = GetPrefabIndexByName(left1.name);
                        possibleIndexes.Remove(removeIndex);
                    }
                }

                if (y >= 2)
                {
                    GameObject down1 = grid[x, y - 1];
                    GameObject down2 = grid[x, y - 2];

                    if (down1 != null && down2 != null && down1.name == down2.name)
                    {
                        int removeIndex = GetPrefabIndexByName(down1.name);
                        possibleIndexes.Remove(removeIndex);
                    }
                }

                int selectedIndex = possibleIndexes[Random.Range(0, possibleIndexes.Count)];
                GameObject prefab = blockPrefabs[selectedIndex];

                Vector3 spawnPos = new Vector3((x * spacing) + xOffset, (y * spacing) + yOffset, -y * 0.01f);
                GameObject block = Instantiate(prefab, spawnPos, Quaternion.identity);
                block.transform.parent = this.transform;
                block.GetComponent<SpriteRenderer>().sortingOrder = y;

                Block blockScript = block.GetComponent<Block>();
                blockScript.boardManager = this;
                blockScript.SetPosition(x, y);

                grid[x, y] = block;
            }
        }
    }

    int GetPrefabIndexByName(string name)
    {
        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            if (blockPrefabs[i].name + "(Clone)" == name)
            {
                return i;
            }
        }
        return -1;
    }

    public void BlockClicked(Block clickedBlock)
    {
        if (selectedBlock == null)
        {
            selectedBlock = clickedBlock;
        }
        else
        {
            if (AreNeighbors(selectedBlock, clickedBlock))
            {
                SwapBlocks(selectedBlock, clickedBlock);
            }

            selectedBlock = null;
        }
    }

    bool AreNeighbors(Block a, Block b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy == 1);
    }

    void SwapBlocks(Block a, Block b)
    {
        // Grid takası
        grid[a.x, a.y] = b.gameObject;
        grid[b.x, b.y] = a.gameObject;

        // Koordinat takası
        int tempX = a.x;
        int tempY = a.y;

        a.SetPosition(b.x, b.y);
        b.SetPosition(tempX, tempY);

        // Sorting Order takası (görsel derinlik)
        SpriteRenderer srA = a.GetComponent<SpriteRenderer>();
        SpriteRenderer srB = b.GetComponent<SpriteRenderer>();
        int tempOrder = srA.sortingOrder;
        srA.sortingOrder = srB.sortingOrder;
        srB.sortingOrder = tempOrder;

        // ✔️ Eşleşme kontrolü yap
        CheckMatches();
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3((x * spacing) + xOffset, (y * spacing) + yOffset, -y * 0.01f);
    }

    public void CheckMatches()
    {
        List<Block> blocksToDestroy = new List<Block>();

        // YATAY eşleşmeleri kontrol et
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Block b1 = grid[x, y]?.GetComponent<Block>();
                Block b2 = grid[x + 1, y]?.GetComponent<Block>();
                Block b3 = grid[x + 2, y]?.GetComponent<Block>();

                if (b1 != null && b2 != null && b3 != null &&
                    b1.name == b2.name && b2.name == b3.name)
                {
                    blocksToDestroy.Add(b1);
                    blocksToDestroy.Add(b2);
                    blocksToDestroy.Add(b3);
                }
            }
        }

        // DİKEY eşleşmeleri kontrol et
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                Block b1 = grid[x, y]?.GetComponent<Block>();
                Block b2 = grid[x, y + 1]?.GetComponent<Block>();
                Block b3 = grid[x, y + 2]?.GetComponent<Block>();

                if (b1 != null && b2 != null && b3 != null &&
                    b1.name == b2.name && b2.name == b3.name)
                {
                    blocksToDestroy.Add(b1);
                    blocksToDestroy.Add(b2);
                    blocksToDestroy.Add(b3);
                }
            }
        }

        // Eşleşen blokları sil
        foreach (Block block in blocksToDestroy)
        {
            int x = block.x;
            int y = block.y;
            Destroy(grid[x, y]);
            grid[x, y] = null;
        }
    }
    void CheckFall()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++) // en alt satır zaten düşemez
            {
                if (grid[x, y] != null && grid[x, y - 1] == null)
                {
                    // Yukarıdaki bloğu al
                    GameObject blockAbove = grid[x, y];
                    Block blockScript = blockAbove.GetComponent<Block>();

                    // Grid içindeki yerlerini değiştir
                    grid[x, y - 1] = blockAbove;
                    grid[x, y] = null;

                    // Koordinat güncelle
                    blockScript.SetPosition(x, y - 1);
                }
            }
        }
    }


}
