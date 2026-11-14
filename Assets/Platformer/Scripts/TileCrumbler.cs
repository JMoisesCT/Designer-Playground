using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer
{
    // Va en el Jugador: hace caer los tiles de suelo frágil (CrumbleTile) que pisa.
    public class TileCrumbler : MonoBehaviour
    {
        private const float PieceLifetime = 1.5f;
        private const float ShakeOffset = 1f / 16f; // un píxel

        private readonly List<ContactPoint2D> _contacts = new();
        private readonly HashSet<(Tilemap, Vector3Int)> _crumbling = new();

        private void OnCollisionStay2D(Collision2D collision)
        {
            Tilemap tilemap = collision.collider.GetComponent<Tilemap>();
            if (tilemap == null) return;

            collision.GetContacts(_contacts);
            foreach (ContactPoint2D contact in _contacts)
            {
                if (contact.normal.y < 0.5f) continue; // solo lo que hay bajo los pies

                Vector3Int cell = tilemap.WorldToCell(contact.point - contact.normal * 0.05f);
                if (tilemap.GetTile(cell) is CrumbleTile tile && _crumbling.Add((tilemap, cell)))
                    StartCoroutine(Crumble(tilemap, cell, tile));
            }
        }

        private IEnumerator Crumble(Tilemap tilemap, Vector3Int cell, CrumbleTile tile)
        {
            // Tiembla un píxel a cada lado.
            tilemap.SetTileFlags(cell, TileFlags.None);
            for (float time = 0f; time < tile.Delay; time += Time.deltaTime)
            {
                float x = Mathf.Repeat(time * 20f, 2f) < 1f ? ShakeOffset : -ShakeOffset;
                tilemap.SetTransformMatrix(cell, Matrix4x4.Translate(new Vector3(x, 0f, 0f)));
                yield return null;
            }

            Vector3 center = tilemap.GetCellCenterWorld(cell);
            SpawnFallingPiece(tilemap, tile.sprite, center);
            tilemap.SetTile(cell, null);
            Sound.Play(tile.CrumbleSound);

            if (tile.RespawnTime <= 0f) yield break;
            yield return new WaitForSeconds(tile.RespawnTime);

            // No vuelve mientras el jugador esté en ese hueco (se quedaría atrapado).
            while (Physics2D.OverlapBox(center, Vector2.one * 0.9f, 0f, LayerMask.GetMask("Player")) != null)
            {
                yield return null;
            }
            tilemap.SetTile(cell, tile);
            _crumbling.Remove((tilemap, cell));
        }

        // Copia del tile que cae por la gravedad, sin colisión, y desaparece.
        private static void SpawnFallingPiece(Tilemap tilemap, Sprite sprite, Vector3 position)
        {
            var piece = new GameObject("Trozo");
            piece.transform.position = position;
            var renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            TilemapRenderer tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null) renderer.sortingLayerID = tilemapRenderer.sortingLayerID;
            piece.AddComponent<Rigidbody2D>().gravityScale = 3f;
            Destroy(piece, PieceLifetime);
        }
    }
}
