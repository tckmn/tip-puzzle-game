﻿using UnityEngine;

public class PlayerScript : MonoBehaviour {

    public enum PieceType {
        Bishop, King, Knight, Pawn, Queen, Rook
    }

    PieceType pieceType;
    public GameObject pieceBishop;
    public GameObject pieceKing;
    public GameObject pieceKnight;
    public GameObject piecePawn;
    public GameObject pieceQueen;
    public GameObject pieceRook;

    Vector2 pos = new Vector2(0, 0);

    // called on initialization
    void Start() {
        ChangePiece(PieceType.Knight);
    }

    public void ChangePiece(PieceType piece) {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        pieceType = piece;

        GameObject pieceObj;
        switch (piece) {
        case PieceType.Bishop: pieceObj = Instantiate(pieceBishop); break;
        case PieceType.King:   pieceObj = Instantiate(pieceKing);   break;
        case PieceType.Knight: pieceObj = Instantiate(pieceKnight); break;
        case PieceType.Pawn:   pieceObj = Instantiate(piecePawn);   break;
        case PieceType.Queen:  pieceObj = Instantiate(pieceQueen);  break;
        case PieceType.Rook:   pieceObj = Instantiate(pieceRook);   break;
        default: return; // unreachable
        }

        pieceObj.transform.parent = transform;
        pieceObj.transform.localScale = transform.localScale;
        pieceObj.transform.Translate(0, Util.ChildrenBounds(pieceObj.transform)
                .extents.y, 0);
    }

    // returns whether the move was successful
    public bool MoveTo(Vector2 movePos) {
        if (CanMove(movePos)) {
            Vector3 oldPos = transform.position;
            oldPos.x = movePos.x;
            oldPos.z = movePos.y;
            transform.position = oldPos;
            pos = movePos;
            return true;
        } else return false;
    }

    bool CanMove(Vector2 movePos) {
        int dx = Mathf.RoundToInt(movePos.x - pos.x),
            dy = Mathf.RoundToInt(movePos.y - pos.y);
        int adx = Mathf.Abs(dx), ady = Mathf.Abs(dy);

        switch (pieceType) {
        case PieceType.Bishop:
            // TODO
            return false;
        case PieceType.King:
            // TODO
            return false;
        case PieceType.Knight:
            return adx + ady == 3 && (adx == 1 || adx == 2);
        case PieceType.Pawn:
            // TODO
            return false;
        case PieceType.Queen:
            // TODO
            return false;
        case PieceType.Rook:
            // TODO
            return false;
        }

        return false; //unreachable
    }

}
