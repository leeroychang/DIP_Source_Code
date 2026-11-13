using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;            // NavMeshAgent, NavMesh
using Unity.AI.Navigation;       // NavMeshSurface
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviour
{

    [SerializeField] Tilemap collidermap;
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap heatmap;
    [SerializeField] Tilemap eventmap;

    [Header("Generation Variables")]
    [SerializeField] int noiseDensity = 50;
    [SerializeField] int iterations = 7;
    [SerializeField] int tileX = 80;
    [SerializeField] int tileY = 80;
    [SerializeField] int eventCount = 8;
    [SerializeField] int enemySpawnRatio = 5;


    [Header("NavMesh")]
    [Tooltip("Drag the NavMeshSurface component (on your floor tilemap GameObject) here. " +
             "It will be rebuilt automatically after each map generation.")]
    [SerializeField] NavMeshSurface navMeshSurface;

    [Header("Spawner")]
    [Tooltip("Drag the EnemySpawner GameObject here. " +
             "NotifyMapReady() will be called on it after map + NavMesh are ready.")]
    [SerializeField] EnemySpawner enemySpawner;

    [Header("Tile Assets")]
    [SerializeField] List<TileBase> collderBases;
    [SerializeField] List<TileBase> tileBases;
    [SerializeField] List<TileBase> heatTileBases;
    [SerializeField] List<TileBase> eventTileBases;


    public void initMap()
    {
        if (tilemap==null || iterations<0) return;
        int[,] tiles = NoiseMapGen(tileX, tileY, noiseDensity);

        for (int i=0; i<iterations; i++) tiles = Iteration(tiles);

        //Connecting regions
        //tiles = ConnectRegions(tiles);

        int[,] heatTiles = HeatmapGeneration(tiles,heatTileBases.Count-2);

        int[,] enemyTiles = GenerateEvents(tiles,1,tileX*tileY*noiseDensity/100*enemySpawnRatio/100,2);

        int[,] eventTiles = GenerateEvents(heatTiles,heatTileBases.Count-1,eventCount,15);

        for (int y=0; y<tileY; y++)
            for (int x=0; x<tileX; x++)
                eventTiles[y,x] = eventTiles[y,x]==0 ? enemyTiles[y,x] : eventTiles[y,x]+1;


        UpdateTilemap(tiles, collidermap, collderBases);
        UpdateTilemap(tiles, tilemap, tileBases);
        UpdateTilemap(heatTiles, heatmap, heatTileBases);
        UpdateTilemap(eventTiles, eventmap, eventTileBases);

        // Rebuild NavMesh so enemies can pathfind on the newly generated map.
        // Must happen AFTER tilemaps are written so the surface sees the new geometry.
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("MapGeneration: NavMesh rebuilt.");
        }
        else
        {
            Debug.LogWarning("MapGeneration: navMeshSurface not assigned — " +
                             "enemies will not pathfind correctly! " +
                             "Drag your floor NavMeshSurface into this field.");
        }

        // Tell the spawner the map is ready so it scans spawn points and begins polling.
        if (enemySpawner != null)
        {
            enemySpawner.NotifyMapReady();
        }
        else
        {
            Debug.LogWarning("MapGeneration: enemySpawner not assigned — " +
                             "enemies will not spawn. " +
                             "Drag your EnemySpawner into this field.");
        }
    }



    //Noise Map Init
    private int[,] NoiseMapGen(int mx, int my, int noiseDensity)
    {
        int[,] tiles = new int[my,mx];
        for (int y=0; y<my; y++)
            for (int x=0; x<mx; x++)
                tiles[y,x] = UnityEngine.Random.Range(0,100)<noiseDensity ? 1 : 0; //Additional conditions and states can be added
            
        return tiles;
    }



    //Cellular Automata Iteration (Checks neighbouring tiles with a conditional of > 4)
    private int[,] Iteration(int[,] tiles)
    {
        int mx = tiles.GetLength(1);
        int my = tiles.GetLength(0);

        int[,] newtiles = (int[,]) tiles.Clone(); //Deep copy as int is a primative type

        for (int y=0; y<my; y++)
            for (int x=0; x<mx; x++)
                newtiles[y,x] = Check(tiles, x, y) ? 1 : 0;
        
        return newtiles;
    }



    //NeighbouringCheck
    private bool Check(int[,] tiles, int px, int py, int count=4, int threshold=1)
    {
        int n = 0;
        for (int iy=-1; iy<=1; iy++)
            for (int ix=-1; ix<=1; ix++)
            {
                if (px+ix<0 || px+ix>=tiles.GetLength(1) || py+iy<0 || py+iy>=tiles.GetLength(0)) n++;
                else if (tiles[py+iy,px+ix]<threshold) n += 1;
            }
        
        return n<=count;
    }


    //Heatmap-Based Generation
    private int[,] HeatmapGeneration(int[,] tiles, int iterations=3)
    {
        if (iterations==0) return tiles;
        int mX = tiles.GetLength(1);
        int mY = tiles.GetLength(0);
        int[,] heatmap = (int[,]) tiles.Clone();

        for (int i=0; i<iterations; i++)
        {
            int[,] heatmapClone = (int[,]) heatmap.Clone();;

            for (int y=0; y<mY; y++)
                for (int x=0; x<mX; x++)
                {
                    if (Check(heatmap,x,y,0,i+1)) heatmapClone[y,x] = i+2;
                }
            
            heatmap = (int[,]) heatmapClone.Clone();
        }
        return heatmap;
    }

    private class EventCell
    {
        public int x { get; private set; }
        public int y { get; private set; }
        public int eventType;

        public EventCell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private int[,] GenerateEvents(int[,] heatmap, int req, int eventCount=5, int minDist=10)
    {
        int mX = heatmap.GetLength(1);
        int mY = heatmap.GetLength(0);
        List<EventCell> eventList = new List<EventCell>();
        List<EventCell> generatedCells = new List<EventCell>();
        EventCell[,] eventMap = new EventCell[mY,mX];

        for (int y=0; y<mY; y++)
            for (int x=0; x<mX; x++)
                if (heatmap[y,x]==req)
                {
                    EventCell cell = new EventCell(x,y);
                    eventMap[y,x] = cell;
                    eventList.Add(cell);
                }
            
        
        for (int i=0; i<eventCount; i++)
        {
            if (eventList.Count==0) break;
            EventCell cell = eventList[UnityEngine.Random.Range(0,eventList.Count)];

            //Temp
            cell.eventType = 1;

            generatedCells.Add(cell);
            eventList.Remove(cell);
            for (int y=cell.y-minDist; y<=cell.y+minDist; y++)
                for (int x=cell.x-minDist; x<=cell.x+minDist; x++)
                {
                    if (x<0 || y<0 || x>=mX || y>=mY) continue;
                    if ((cell.x-x)*(cell.x-x) + (cell.y-y)*(cell.y-y) > minDist*minDist) continue;

                    EventCell removeCell = eventMap[y,x];
                    if (removeCell==null) continue;
                    eventMap[y,x] = null;
                    eventList.Remove(removeCell);
                }
        }

        int[,] eventTiles = new int[mY,mX];
        foreach (EventCell cell in generatedCells) eventTiles[cell.y,cell.x] = cell.eventType;

        return eventTiles;
    }



    //Pattern Recognition




    //Tileset Update
    private void UpdateTilemap(int[,] tiles, Tilemap tilemap, List<TileBase> bases)
    {
        int mx = tiles.GetLength(1);
        int my = tiles.GetLength(0);

        if (tilemap==null) return;
        tilemap.size = new Vector3Int(mx,my);
        for (int y=0; y<my; y++)
            for (int x=0; x<mx; x++)
            {
                tilemap.SetTile(new Vector3Int(x,y), tiles[y,x]<bases.Count ? bases[tiles[y,x]] : bases.Last());
            }
    }



}
