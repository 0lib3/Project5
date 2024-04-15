using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    public GameObject[] tiles;

    public GameObject player;
    public GameObject enemyPrefab;

    const int N = 1;
    const int E = 2;
    const int S = 4;
    const int W = 8;

    Dictionary<Vector2, int> cell_walls = new Dictionary<Vector2, int>();

    float tile_size = 10;
    public int width = 10;   // Width of map  
    public int height = 10;  // Height of map

    List<List<int>> map = new List<List<int>>();

    



    // Start is called before the first frame update
    void Start()
    {
        cell_walls[new Vector2(0, -1)] = N;
        cell_walls[new Vector2(1, 0)] = E;
        cell_walls[new Vector2(0, 1)] = S;
        cell_walls[new Vector2(-1, 0)] = W;

        MakeMaze();

        GameObject p = GameObject.Instantiate(player);
        p.transform.position = new Vector3(2.91f, 1f, 4.6f);

        SpawnEnemies(8);
    }

    private List<Vector2> CheckNeighbors(Vector2 cell, List<Vector2> unvisited) {
        // Returns a list of cell's unvisited neighbors
        List<Vector2> list = new List<Vector2>();

        foreach (var n in cell_walls.Keys)
        {
            if (unvisited.IndexOf((cell + n)) != -1) { 
                list.Add(cell+ n);
            }
                    
        }
        return list;
    }


    private void MakeMaze()
    {
        List<Vector2> unvisited = new List<Vector2>();
        List<Vector2> stack = new List<Vector2>();

        // Fill the map with #15 tiles
        for (int i = 0; i < width; i++)
        {
            map.Add(new List<int>());
            for (int j = 0; j < height; j++)
            {
                map[i].Add(N | E | S | W);
                unvisited.Add(new Vector2(i, j));
            }

        }

        Vector2 current = new Vector2(0, 0);

        unvisited.Remove(current);

        while (unvisited.Count > 0) {
            List<Vector2> neighbors = CheckNeighbors(current, unvisited);

            if (neighbors.Count > 0)
            {
                Vector2 next = neighbors[UnityEngine.Random.RandomRange(0, neighbors.Count)];
                stack.Add(current);

                Vector2 dir = next - current;

                int current_walls = map[(int)current.x][(int)current.y] - cell_walls[dir];

                int next_walls = map[(int)next.x][(int)next.y] - cell_walls[-dir];

                map[(int)current.x][(int)current.y] = current_walls;

                map[(int)next.x][(int)next.y] = next_walls;

                current = next;
                unvisited.Remove(current);

            }
            else if (stack.Count > 0) { 
                current = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
            
            }

            
        }

        for (int i = 0; i < width; i++)
        {
            
            for (int j = 0; j < height; j++)
            {
                GameObject tile = GameObject.Instantiate(tiles[map[i][j]]);
                tile.transform.parent = gameObject.transform;

                tile.transform.Translate(new Vector3 (j*tile_size, 0, i * tile_size));
                tile.name += " " + i.ToString() + ' ' + j.ToString();
                tile.GetComponentInChildren<NavMeshSurface>().BuildNavMesh();
               
            }

        }

    }

    private void SpawnEnemies(int numberOfEnemies){
    List<Vector2> possibleLocations = new List<Vector2>();

    // every cell could spawn an enemy
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++)
            {
                // Check if the cell is open enough; for example, not surrounded by walls
                if (map[i][j] != (N | E | S | W)) // adjust the condition based on your maze logic
                {
                    possibleLocations.Add(new Vector2(i, j));
                }
            }
        }

    // Randomly pick locations and spawn enemies
        for (int i = 0; i < numberOfEnemies; i++){
            if (possibleLocations.Count > 0){
                int randomIndex = UnityEngine.Random.Range(0, possibleLocations.Count);
                Vector2 spawnPos = possibleLocations[randomIndex];
                GameObject enemy = Instantiate(enemyPrefab, new Vector3(spawnPos.y * tile_size, 0, spawnPos.x * tile_size), Quaternion.identity);
                enemy.transform.parent = gameObject.transform; // Optional: Set maze as parent
                possibleLocations.RemoveAt(randomIndex); // Optional: Remove the location from the list
            }
        }
    }
}
