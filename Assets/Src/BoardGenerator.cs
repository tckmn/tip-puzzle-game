﻿using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour {

    enum Tile {
        Empty, Square, Start, End, Unknown
    }
    public GameObject tileSquare;
    public GameObject tileStart;
    public GameObject tileEnd;

    public Material lightSquareMat;

    public GameObject player;
    PlayerScript playerScript;

    public GameObject canvas;
    CanvasScript canvasScript;

    static List<Vector2> passable = new List<Vector2>();

    // called on initialization
    void Start() {

        playerScript = player.GetComponent<PlayerScript>();
        canvasScript = canvas.GetComponent<CanvasScript>();

        List<string> lines = File.ReadAllLines("Assets/Levels/level0.txt").ToList();
        foreach (char ch in lines[0]) {
            ++playerScript.movesLeft[PlayerScript.pieceChars.IndexOf(ch)];
        }
        canvasScript.RedrawNumbers();
        lines.RemoveAt(0);

        // read the level file and turn it into Tile objects
        var tiles =
            from line in lines
            select (
                    from ch in line
                    select
                        ch == ' ' ? Tile.Empty :
                        ch == '#' ? Tile.Square :
                        ch == 'S' ? Tile.Start :
                        ch == 'E' ? Tile.End :
                        Tile.Unknown // ???
                   );

        // loop through the grid of Tiles and place them in the world
        foreach (var x in tiles.Select((row, idx) => new { row, idx })) {
            foreach (var y in x.row.Select((tile, idx) => new { tile, idx })) {

                // the tile being placed
                GameObject tile;
                // the position of the tile
                Vector3 pos = new Vector3(x.idx, 0, y.idx);

                switch (y.tile) {
                case Tile.Square:
                    tile = (GameObject) Instantiate(tileSquare, pos, Quaternion.identity);
                    if ((x.idx + y.idx) % 2 == 0) {
                        tile.GetComponent<Renderer>().material = lightSquareMat;
                    }
                    passable.Add(new Vector2(x.idx, y.idx));
                    break;
                case Tile.Start:
                    tile = (GameObject) Instantiate(tileStart, pos, Quaternion.identity);
                    playerScript.ForceMove(new Vector2(x.idx, y.idx));
                    passable.Add(new Vector2(x.idx, y.idx));
                    break;
                case Tile.End:
                    tile = (GameObject) Instantiate(tileEnd, pos, Quaternion.identity);
                    passable.Add(new Vector2(x.idx, y.idx));
                    break;
                default:
                    continue;
                }

                tile.GetComponent<TileBehavior>().SetPosition(x.idx, y.idx);

            }
        }

    }

    public static bool IsPassable(Vector2 v) {
        return passable.Contains(v);
    }

}
