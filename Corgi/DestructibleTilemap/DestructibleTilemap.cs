using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DestructibleTilemap : MonoBehaviour {

    [Header("Tile Respawn Rate")] 
    public bool RespawnTiles = true;
    public float RespawnMin = 5f;
    public float RespawnMax = 30f;

    [Header("Effects")] 
    public GameObject DestroyEffect;

    [Header("")] 
    public AudioClip DestroySfx;
    public AudioClip RespawnSfx;
    
    public MMObjectPooler ObjectPooler { get; set; }
    private Tilemap _tilemap;

    private void Awake() {
        _tilemap = GetComponent<Tilemap>();
        ObjectPooler = GetComponent<MMSimpleObjectPooler>();
    }

    public void DamageTiles(Vector3 origin, int damage, int radius) {
        
        Vector3Int position = _tilemap.WorldToCell(origin);
        DestroyTile(position);
        
    }

    private void DestroyTile(Vector3Int position) {
        
        if (DestroySfx!=null) 
            SoundManager.Instance.PlaySound(DestroySfx,position);
        
        TileBase tile = _tilemap.GetTile(position);
        _tilemap.SetTile(position, null);

        SpawnDestroyEffect(position);

        if(RespawnTiles)
            StartCoroutine(RespawnTile(position, tile));

    }
    
    private IEnumerator RespawnTile(Vector3Int position, TileBase tile) {

        if (tile != null) {
            yield return new WaitForSeconds(Random.Range(RespawnMin, RespawnMax));
            _tilemap.SetTile(position, tile);
            
            if (RespawnSfx!=null) 
                SoundManager.Instance.PlaySound(RespawnSfx,position);

        }
        
    }

    private GameObject SpawnDestroyEffect(Vector3Int position) {
        
        /// we get the next object in the pool and make sure it's not null
        GameObject nextGameObject = ObjectPooler.GetPooledGameObject();
        
        if (nextGameObject==null)	{ return null; }
        if (nextGameObject.GetComponent<MMPoolableObject>()==null)
            throw new Exception(gameObject.name+" is trying to spawn objects that don't have a PoolableObject component.");

        nextGameObject.transform.position = position + Vector3.one * 0.5f;
        
        nextGameObject.gameObject.SetActive(true);
        nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
        
        return nextGameObject;

    }

}
