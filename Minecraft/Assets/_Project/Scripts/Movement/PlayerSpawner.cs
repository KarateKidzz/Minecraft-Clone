using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public Camera LoadingCamera;
    public GameObject PlayerPrefab;
    public ChunkGenerator ChunkGenerator;
    public InfiniteTerrain InfiniteTerrain;
    public ChunkMap ChunkMap;

    void Start()
    {
        StartCoroutine(WaitForLoad());
    }

    IEnumerator WaitForLoad ()
    {
        yield return new WaitForSeconds(2);

        while (ChunkGenerator.IsLoading) yield return new WaitForSeconds(1);

        float groundHeight = 256;

        if (Physics.Raycast(new Vector3(0.5f, 400, 0.5f), Vector3.down, out RaycastHit hit, 400f))
        {
            groundHeight = hit.point.y + 1;
        }

        GameObject player = Instantiate(PlayerPrefab, new Vector3(0.5f, groundHeight, 0.5f), Quaternion.identity);

        LoadingCamera.gameObject.SetActive(false);

        InfiniteTerrain.Viewer = player.transform;
        ChunkMap.Player = player.transform;

        player.GetComponent<PlayerInteract>().ChunkGenerator = ChunkGenerator;

        yield return null;
    }
}
