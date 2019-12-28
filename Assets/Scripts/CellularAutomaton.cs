using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomaton : MonoBehaviour
{
    private int[,] grid; // 0 = air, 1 = block
    public int gridSize = 20;
    public int generations = 0;

    public GameObject cubePrefab;
    public GameObject chipPrefab;

    private void Start()
    {
        InitializeGrid();

        IterateThroughGenerations();

        CreateVisuals();
    }

    private void InitializeGrid()
    {
        grid = new int[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = 1;
            }
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float rand = UnityEngine.Random.Range(0f, 1f);
                if(rand > 0.5) // TODO - Replace  0.5 with serialized variable
                {
                    grid[x, y] = 0;
                }
            }
        }
    } // Generates initial grid with some air and some blocks

    private void IterateThroughGenerations()
    {
        for (int i = 0; i < generations; i++) // maybe make it g instead?
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    int neighbors = getNumOfNeighbors(x,y);

                    if (neighbors == 3 && grid[x,y] == 0 ) // new cell born
                    {
                        grid[x, y] = 1;
                    }
                    else if (neighbors > 4 || neighbors < 1)
                    {
                        grid[x, y] = 0;
                    }
                }
            }
        }
    } // uses mazentric ruleset

    private int getNumOfNeighbors(int cx,int cy)
    {
        int neighbors = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {   

                if(x == 0 && y == 0) // To avoid counting the center towards neighbor
                {
                    continue;
                }

                if(OutOfBounds(cx + x) || OutOfBounds(cy + y))
                {
                    continue;
                }

                if(grid[cx+x,cy + y] == 1)
                {
                    ++neighbors;
                }
            }
        }

        return neighbors;
    }

    private bool OutOfBounds(int val)
    {
        if(val < 0 || val > gridSize - 1)
        {
            return true;
        }

        return false;
    }

    private void CreateVisuals()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if(grid[x,y] == 1)
                {
                    GameObject block = Instantiate(cubePrefab, new Vector3(transform.position.x + (x - gridSize / 2f), -0.36f, transform.position.z + (y - gridSize / 2f)), Quaternion.identity,transform);
                }
            }
        }

        int rand = UnityEngine.Random.Range(0, 101);

        if(rand > 50)
        {
            GameObject chip = Instantiate(chipPrefab, new Vector3(transform.position.x,0,transform.position.z), Quaternion.identity, transform);
        }
    }

    void DisplayGrid() // for testing purpose
    {
        string gridString = "";
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                gridString += grid[x, y];
            }
            gridString += Environment.NewLine;
        }

        Debug.Log(gridString);
    }
}
