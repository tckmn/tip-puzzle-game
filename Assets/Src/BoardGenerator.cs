﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour {

    enum Tile {
        Empty, Square, EnemyPawn, EnemyKnight, EnemyBishop, EnemyRook,
        EnemyQueen, EnemyKing, Start, End, Unknown
    }
    public GameObject _tileSquare;
    public GameObject _tileStart;
    public GameObject _tileEnd;

    public Material _lightSquareMat;
    public Material _enemyMat;

    public GameObject _player;
    PlayerScript playerScript;

    public GameObject _canvas;
    CanvasScript canvasScript;

    public GameObject _camera;
    Controller cameraScript;

    public GameObject _pieceBishop;
    public GameObject _pieceKing;
    public GameObject _pieceKnight;
    public GameObject _piecePawn;
    public GameObject _pieceQueen;
    public GameObject _pieceRook;

    public struct Enemy {
        public GameObject _gameObject;
        public Piece.Type _pieceType;
        public Vector2 _pos;
        public Enemy(GameObject _gameObject, Piece.Type _pieceType,
               Vector2 _pos) {
          this._gameObject = _gameObject;
          this._pieceType = _pieceType;
          this._pos = _pos;
        }
    }
    public readonly static List<Enemy> Enemies = new List<Enemy>();

    readonly static List<Vector2> passable = new List<Vector2>();

    void Awake() {
        playerScript = _player.GetComponent<PlayerScript>();
        canvasScript = _canvas.GetComponent<CanvasScript>();
        cameraScript = _camera.GetComponent<Controller>();
    }

    void Start() {
        LoadLevel();
    }

    void LoadLevel() {
        Enemies.Clear();
        passable.Clear();

        List<string> lines = GlobalData._currentLevel.text
            .Split("\n".ToCharArray()).ToList();

        playerScript._switchesLeft = 1;
        playerScript._movesLeft = new int[6];
        playerScript.ChangePiece((Piece.Type)
                Piece._pieceChars.IndexOf(lines[0][0]));
        foreach (char ch in lines[0]) {
            if (ch == '*') ++playerScript._switchesLeft;
            else ++playerScript._movesLeft[Piece._pieceChars.IndexOf(ch)];
        }
        canvasScript.RedrawNumbers();
        lines.RemoveAt(0);

        // remove existing tiles
        List<Transform> children = transform.Cast<Transform>().ToList();
        children.ForEach(child => Destroy(child.gameObject));

        // read the level file and turn it into Tile objects
        var tiles =
            from line in lines
            select (
                    from ch in line
                    select
                        ch == ' ' ? Tile.Empty :
                        ch == '#' ? Tile.Square :
                        ch == 'P' ? Tile.EnemyPawn :
                        ch == 'N' ? Tile.EnemyKnight :
                        ch == 'B' ? Tile.EnemyBishop :
                        ch == 'R' ? Tile.EnemyRook :
                        ch == 'Q' ? Tile.EnemyQueen :
                        ch == 'K' ? Tile.EnemyKing :
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
                var pos = new Vector3(x.idx, 0, y.idx);
                // this is really really bad and I'm sorry
                bool isEnemy = false;

                switch (y.tile) {
                case Tile.Square:
                case Tile.EnemyPawn:
                case Tile.EnemyKnight:
                case Tile.EnemyBishop:
                case Tile.EnemyRook:
                case Tile.EnemyQueen:
                case Tile.EnemyKing:
                    tile = (GameObject) Instantiate(_tileSquare, pos,
                            Quaternion.identity);
                    if ((x.idx + y.idx) % 2 == 0) {
                        tile.GetComponent<Renderer>().material = _lightSquareMat;
                    }
                    passable.Add(new Vector2(x.idx, y.idx));
                    isEnemy = y.tile != Tile.Square;
                    break;
                case Tile.Start:
                    tile = (GameObject) Instantiate(_tileStart, pos,
                            Quaternion.identity);
                    playerScript.ForceMove(new Vector2(x.idx, y.idx));
                    passable.Add(new Vector2(x.idx, y.idx));
                    break;
                case Tile.End:
                    tile = (GameObject) Instantiate(_tileEnd, pos,
                            Quaternion.identity);
                    passable.Add(new Vector2(x.idx, y.idx));
                    break;
                default:
                    continue;
                }

                tile.GetComponent<TileBehavior>().SetPosition(x.idx, y.idx);
                tile.transform.parent = transform;

                if (isEnemy) {
                    Piece.Type pieceType;

                    switch (y.tile) {
                    case Tile.EnemyBishop:
                        pieceType = Piece.Type.Bishop;
                        break;
                    case Tile.EnemyKing:
                        pieceType = Piece.Type.King;
                        break;
                    case Tile.EnemyKnight:
                        pieceType = Piece.Type.Knight;
                        break;
                    case Tile.EnemyPawn:
                        pieceType = Piece.Type.Pawn;
                        break;
                    case Tile.EnemyQueen:
                        pieceType = Piece.Type.Queen;
                        break;
                    case Tile.EnemyRook:
                        pieceType = Piece.Type.Rook;
                        break;
                    default: return; // unreachable
                    }

                    AddEnemy(pieceType, new Vector2(x.idx, y.idx));
                }

            }

            cameraScript.Adjust((tiles.Count() / 2.0f) - 1);
        }
    }

    public static bool IsPassable(Vector2 v) {
        return passable.Contains(v) &&
            Enemies.TrueForAll(enemy => enemy._pos != v);
    }

    public void KillAllEnemies() {
        foreach (Enemy enemy in Enemies) {
            Destroy(enemy._gameObject);
        }
        Enemies.Clear();
    }

    public void RedrawEnemies() {
        foreach (Enemy enemy in Enemies) {
            Destroy(enemy._gameObject);
        }
        var enemiesClone = new List<Enemy>(Enemies);
        Enemies.Clear();
        foreach (Enemy enemy in enemiesClone) {
            AddEnemy(enemy._pieceType, enemy._pos);
        }
    }

    void AddEnemy(Piece.Type piece, Vector2 pos) {
        GameObject pieceObj;

        switch (piece) {
        case Piece.Type.Bishop:
            pieceObj = (GameObject) Instantiate(_pieceBishop,
                    transform.position, transform.rotation);
            break;
        case Piece.Type.King:
            pieceObj = (GameObject) Instantiate(_pieceKing,
                    transform.position, transform.rotation);
            break;
        case Piece.Type.Knight:
            pieceObj = (GameObject) Instantiate(_pieceKnight,
                    transform.position, transform.rotation);
            break;
        case Piece.Type.Pawn:
            pieceObj = (GameObject) Instantiate(_piecePawn,
                    transform.position, transform.rotation);
            break;
        case Piece.Type.Queen:
            pieceObj = (GameObject) Instantiate(_pieceQueen,
                    transform.position, transform.rotation);
            break;
        case Piece.Type.Rook:
            pieceObj = (GameObject) Instantiate(_pieceRook,
                    transform.position, transform.rotation);
            break;
        default: return; // unreachable
        }

        pieceObj.transform.parent = transform;
        pieceObj.transform.localScale = _player.transform.localScale;
        pieceObj.transform.position = new Vector3(pos.x, 0, pos.y);
        Bounds bounds = Util.ChildrenBounds(pieceObj.transform);
        pieceObj.transform.Translate(0,
                bounds.extents.y - bounds.center.y, 0);

        foreach (Transform child in pieceObj.transform) {
            child.gameObject.GetComponent<Renderer>().material = _enemyMat;
        }

        Enemies.Add(new Enemy(pieceObj, piece, pos));
    }

}
