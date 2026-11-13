using System;
using System.Collections.Generic;
using UnityEngine;
public class MapConnection : MonoBehaviour
{

        //Connecting Regions
    private class Cell
    {
        public MapRegion mapRegion;
        public int x;
        public int y;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    private class EdgeCell : Cell
    {
        public Dictionary<string,int> dist;

        public EdgeCell(int x, int y) : base(x, y)
        {
        }
    }


    private class MapRegion : IComparable<MapRegion>
    {
        public List<Cell> cells {get; private set;}
        public Dictionary<string, EdgeCell> edges;
        public int size() { return cells.Count; }

    
        public void SetEdges(int mX, int mY)
        {
            edges = new Dictionary<string, EdgeCell>();

            foreach (Cell cell in cells)
            {
                EdgeCell edgeCell = new EdgeCell(cell.x,cell.y);
                edgeCell.dist["NW"] = cell.x*cell.x+cell.y*cell.y;
                edgeCell.dist["NE"] = (mX-cell.x)*(mX-cell.x)+cell.y*cell.y;
                edgeCell.dist["SW"] = cell.x*cell.x+(mY-cell.y)*(mY-cell.y);
                edgeCell.dist["SE"] = (mX-cell.x)*(mX-cell.x)+(mY-cell.y)*(mY-cell.y);

                foreach (string key in new string[] {"NW","NE","SW","SE"})
                    if (!edges.ContainsKey(key) || edgeCell.dist[key]<edges[key].dist[key]) edges[key]=edgeCell;
            }
        }

        public void AddCell(Cell cell)
        {
            cells.Add(cell);
            cell.mapRegion = this;
        }

        public int CompareTo(MapRegion other)
        {
            if (other==null) return 1;
            return other.size().CompareTo(size()); //Sort descending
        }
    }

    private int[,] ConnectRegions(int[,] tiles)
    {
        int mX = tiles.GetLength(1);
        int mY = tiles.GetLength(0);
        
        List<MapRegion> regions = GetRegions(tiles);
        regions.Sort();

        MapRegion newRegion = null;

        foreach (MapRegion mapRegion in regions)
        {
            if (mapRegion.size()<32) tiles = FillRegion(tiles,mapRegion);
            else if (newRegion==null) newRegion = mapRegion;
            else
            {
                EdgeCell pointA = null;
                EdgeCell pointB = null;
                int sqrDist = -1;
                foreach (EdgeCell cellA in newRegion.edges.Values)
                    foreach (EdgeCell cellB in mapRegion.edges.Values)
                    {
                        int newDist = (cellA.x-cellB.x)*(cellA.x-cellB.x)+(cellA.y-cellB.y)*(cellA.y-cellB.y);
                        if (sqrDist==-1 || newDist<sqrDist)
                        {
                            pointA = cellA;
                            pointB = cellB;
                            sqrDist = newDist;
                        }
                    }
                
                //Connect Points

                //Combine cells of both regions and connector



            }
            
        }
        
        return null;
    }

    private List<MapRegion> GetRegions(int[,] tiles)
    {
        int mX = tiles.GetLength(1);
        int mY = tiles.GetLength(0);
        List<MapRegion> regions = new List<MapRegion>();
        Cell[,] cells = new Cell[mY,mX];

        for (int y=0; y<mY; y++)
            for (int x=0; x<mX; x++)
            {
                if (tiles[y,x]==0)
                {
                    cells[y,x] = new Cell(x,y);
                    if (x-1<0 || y-1<0 || tiles[y,x-1]!=0 || tiles[y-1,x]!=0)
                    {
                        MapRegion mapRegion = new MapRegion();
                        regions.Add(mapRegion);
                        mapRegion.AddCell(cells[y,x]);
                    }
                    else if (tiles[y,x-1]==0) cells[y,x-1].mapRegion.AddCell(cells[y,x]);
                    else if (tiles[y-1,x]==0) cells[y-1,x].mapRegion.AddCell(cells[y,x]);
                }
            }
        
        return regions;
    }

    private int[,] FillRegion(int[,] tiles, MapRegion mapRegion)
    {
        foreach (Cell cell in mapRegion.cells) tiles[cell.y,cell.x]=1;
        return tiles;
    }




}
