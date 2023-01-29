﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class WorldGen : MonoBehaviour
{
    public GameObject[] blockPrefabs;

    public Transform instantiationPoint;

    public Vector2 startingPlane;

    public static WorldGen instance;

    public LayerMask blockLayer;

    public void GenerateFlatPlane(int x, int z)
    {
        for (int i = 0; i < x; i++)
        {
            CreateBlock(new Vector3(i, 0f, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            for (int i2 = 1; i2 < z; i2++)
            {
                CreateBlock(new Vector3(i, 0f, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            }
        }
    }

    public IEnumerator GenerateFlatPlanePreformance(int x, int z)
    {
        for (int i = 0; i < x; i++)
        {
            yield return null;
            CreateBlock(new Vector3(i, 0f, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            for (int i2 = 1; i2 < z; i2++)
            {
                CreateBlock(new Vector3(i, 0f, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            }
        }
    }

    public IEnumerator GenerateFlatPlanePreformancePerlinNoise(int x, int z, float multiplier)
    {
        for (int i = 0; i < x; i++)
        {
            yield return null;
            float perlin = (float)Math.Round(Mathf.PerlinNoise(UnityEngine.Random.Range(0, 0.99f), UnityEngine.Random.Range(0, 0.99f)) * multiplier);
            CreateBlock(new Vector3(i, perlin, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
            if (perlin > 0f)
            {
                for (int j = (int)perlin; j > 0f; j--)
                {
                    CreateBlock(new Vector3(i, j, 0f), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
                }
            }
            for (int i2 = 1; i2 < z; i2++)
            {
                float perlin2 = (float)Math.Round(Mathf.PerlinNoise(UnityEngine.Random.Range(0, 0.99f), UnityEngine.Random.Range(0, 0.99f)) * multiplier);
                CreateBlock(new Vector3(i, perlin2, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
                if (perlin2 > 0f)
                {
                    for (int j = (int)perlin2; j > 0f; j--)
                    {
                        CreateBlock(new Vector3(i, j, i2), blockPrefabs[UnityEngine.Random.Range(0, blockPrefabs.Length)]);
                    }
                }
            }
        }
    }

    public IEnumerator GenerateFromUVBMapPreformace(string fileName)
    {
        string contents = "";
        try
        {
            contents = File.ReadAllText((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
        }
        catch
        {
            UIController.instance.Popup("could not find uvbmap file!", 2f);
            yield break;
        }
        DestroyAllBlocks();
        int pause = 0;
        foreach (string line in contents.Split('\r', '\n'))
        {
            pause++;
            if (pause > 19)
            {
                yield return null;
                pause = 0;
            }
            string[] UVBParams = line.Split(',');
            try
            {
                CreateBlock(new Vector3(float.Parse(UVBParams[1]), float.Parse(UVBParams[2]), float.Parse(UVBParams[3])), GameData.instance.availableBlocks[int.Parse(UVBParams[0])]);
            }
            catch (Exception e)
            {
                Debug.Log("could not create block! : " + e.ToString());
            }
        }
        UIController.instance.Popup("generated map from " + (fileName.EndsWith(".uvbmap") ? fileName : fileName + ".uvbmap"), 2f);
    }

    public static Mesh CombineBlocks()
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (GameObject block in GameObject.FindGameObjectsWithTag("block"))
        {
            meshFilters.Add(block.GetComponent<MeshFilter>());
        }
         CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < combine.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        return mesh;
    }

    public void GenerateFromUVBMap(string fileName)
    {
        string contents = File.ReadAllText((fileName.EndsWith(".uvbmap") ? StreamingAssets.UVBMapPath + "/" + fileName : StreamingAssets.UVBMapPath + "/" + fileName + ".uvbmap"));
        DestroyAllBlocks();
        foreach (string line in contents.Split('\r', '\n'))
        {
            string[] UVBParams = line.Split(',');
            CreateBlock(new Vector3(float.Parse(UVBParams[1]), float.Parse(UVBParams[2]), float.Parse(UVBParams[3])), GameData.instance.availableBlocks[int.Parse(UVBParams[0])]);
        }
        UIController.instance.Popup("generated map from " + (fileName.EndsWith(".uvbmap") ? fileName : fileName + ".uvbmap"), 2f);
    }

    public void DestroyAllBlocks()
    {
        foreach (GameObject block in GameObject.FindGameObjectsWithTag("block"))
        {
            if (block.layer == 9)
            {
                Destroy(block);
            }
        }
        UIController.instance.Popup("destroyed all blocks.", 1f);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        blockPrefabs[0] = GameData.instance.planeBlock;
        startingPlane = GameData.instance.planeSize;
        StartCoroutine(GenerateFlatPlanePreformance((int)startingPlane.x, (int)startingPlane.y));
    }

    public GameObject test;

    void Update()
    {
        //foreach (GameObject block in GameObject.FindGameObjectsWithTag("block"))
        //{
        //    block.GetComponent<MeshRenderer>().enabled = Vector3.Distance(PlayerMovement.instance.transform.position, block.transform.position) < GameData.renderDistance;
        //}
    }

    public void CreateBlock(Vector3 pos, GameObject blockPrefab)
    {
        GameObject block = Instantiate(blockPrefab, instantiationPoint);
        block.layer = 9;
        block.transform.localPosition = pos;
        block.name = block.name.Replace("(Clone)", "");
        GameData.instance.blockPlacedAmount++;
    }
}
