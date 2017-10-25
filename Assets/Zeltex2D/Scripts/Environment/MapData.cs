﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex2D
{
    [ExecuteInEditMode]
    public class MapData : MonoBehaviour
    {
        [HideInInspector]
        public int[,] Data;
        public int MapWidth = 32;
        public int MapHeight = 32;
        public int EnemiesToSpawn = 6;
        [Header("Instantiation")]
        public List<GameObject> TilePrefabs = new List<GameObject>();
        public GameObject CharacterPrefab;
        public GameObject LevelProgressorPrefab;
        [Header("Spawned")]
        public GameObject Player;
        public List<GameObject> SpawnedCharacters = new List<GameObject>();
        public GameObject LevelProgressor;
        public List<GameObject> SpawnedTiles = new List<GameObject>();
        [Header("Actions")]
        public bool IsDebugMap;
        public bool DoFillWithTiles;
        public bool DoClearSpawned;

        private void Update()
        {
            if (DoFillWithTiles)
            {
                DoFillWithTiles = false;
                FillWithTiles();
            }
            if (DoClearSpawned)
            {
                DoClearSpawned = false;
                ClearSpawned();
            }
        }

        public void SetCharactersMovement(bool NewState)
        {
            for (int i = 0; i < SpawnedCharacters.Count; i++)
            {
                if (SpawnedCharacters[i])
                {
                    SetCharacterMovement(SpawnedCharacters[i].GetComponent<Character2D>(), NewState);
                }
            }
            SetCharacterMovement(Player.GetComponent<Character2D>(), NewState);
        }

        private void SetCharacterMovement(Character2D MyCharacter, bool NewState)
        {
            if (MyCharacter)
            {
                MyCharacter.SetMovement(NewState);
                UserControl2D MyControl = MyCharacter.gameObject.GetComponent<UserControl2D>();
                if (MyControl)
                {
                    MyControl.enabled = NewState;
                }
            }
        }

        /// <summary>
        /// Creates an empty map with a new size
        /// </summary>
        public void Empty()
        {
            Data = new int[MapWidth, MapHeight];
        }

        public void ClearSpawned()
        {
            for (int i = 0; i < SpawnedTiles.Count; i++)
            {
                if (SpawnedTiles[i])
                {
                    if (Application.isPlaying)
                    {
                        Destroy(SpawnedTiles[i]);
                    }
                    else
                    {
                        DestroyImmediate(SpawnedTiles[i]);
                    }
                }
            }
            SpawnedTiles.Clear();
            for (int i = 0; i < SpawnedCharacters.Count; i++)
            {
                if (SpawnedCharacters[i])
                {
                    if (Application.isPlaying)
                    {
                        Destroy(SpawnedCharacters[i]);
                    }
                    else
                    {
                        DestroyImmediate(SpawnedCharacters[i]);
                    }
                }
            }
            SpawnedCharacters.Clear();
        }

        public void FillWithTiles()
        {
            if (Data != null)
            {
                ClearSpawned();
                for (int x = 0; x < MapWidth; x++)
                {
                    for (int y = 0; y < MapHeight; y++)
                    {
                        if (TilePrefabs[Data[x, y]] != null)
                        {
                            GameObject NewTile = Instantiate(TilePrefabs[Data[x, y]], transform);
                            Vector3 NewPosition = new Vector3(-MapWidth / 2 + x , -MapHeight / 2 + y, 0);
                            NewTile.transform.localPosition = NewPosition;
                            SpawnedTiles.Add(NewTile);
                        }
                    }
                }
            }
            SpawnRandomEnemies();
        }

        public void SpawnRandomEnemies()
        {
            string Seed = PlayerPrefs.GetString(SeedInput.SeedKey, SeedInput.SeedDefault);
            int SeedValue = 0;
            for (int i = 0; i < Seed.Length; i++)
            {
                SeedValue += (int)Seed[i];
            }
            Random.InitState(SeedValue);
            int SpawnPositionX = Random.Range(1, MapWidth - 1);
            int SpawnPositionY = Random.Range(1, MapHeight - 1);
            while (EnemiesToSpawn >= 1)
            {
                if (IsNonSolid(SpawnPositionX, SpawnPositionY))
                {
                    SpawnCharacter(SpawnPositionX, SpawnPositionY);
                    EnemiesToSpawn--;
                }
                else
                {
                    SpawnPositionX = Random.Range(1, MapWidth - 1);
                    SpawnPositionY = Random.Range(1, MapHeight - 1);
                }
            }
            EnemiesToSpawn = 1;
            while (EnemiesToSpawn >= 1)
            {
                if (IsNonSolid(SpawnPositionX, SpawnPositionY))
                {
                    Vector3 NewPosition = new Vector3(-MapWidth / 2 + SpawnPositionX, -MapHeight / 2 + SpawnPositionY, 0);
                    Player.transform.localPosition = NewPosition;
                    EnemiesToSpawn--;
                }
                else
                {
                    SpawnPositionX = Random.Range(1, MapWidth - 1);
                    SpawnPositionY = Random.Range(1, MapHeight - 1);
                }
            }

            EnemiesToSpawn = 1;
            while (EnemiesToSpawn >= 1)
            {
                if (IsNonSolid(SpawnPositionX, SpawnPositionY) &&
                    IsCharacterAtPosition(Player.transform, SpawnPositionX, SpawnPositionY) == false)
                {
                    SpawnLevelProgressor(SpawnPositionX, SpawnPositionY);
                    EnemiesToSpawn--;
                }
                else
                {
                    SpawnPositionX = Random.Range(1, MapWidth - 1);
                    SpawnPositionY = Random.Range(1, MapHeight - 1);
                }
            }
        }

        private void SpawnLevelProgressor(int PositionX, int PositionY)
        {
            LevelProgressor = Instantiate(LevelProgressorPrefab, transform.parent);
            Vector3 NewPosition = new Vector3(-MapWidth / 2 + PositionX, -MapHeight / 2 + PositionY, 0);
            LevelProgressor.transform.localPosition = NewPosition;
        }

        public void SpawnCharacter(int PositionX, int PositionY)
        {
            GameObject NewCharacter = Instantiate(CharacterPrefab, transform.parent);
            if (NewCharacter.name.Contains("(Clone)"))
            {
                NewCharacter.name = NewCharacter.name.Substring(0, NewCharacter.name.IndexOf("(Clone)"));
            }
            Vector3 NewPosition = new Vector3(-MapWidth / 2 + PositionX, -MapHeight / 2 + PositionY, 0);
            NewCharacter.transform.localPosition = NewPosition;
            SpawnedCharacters.Add(NewCharacter);
        }

        public bool IsNonSolid(int PositionX, int PositionY)
        {
            for (int i = 0; i < SpawnedCharacters.Count; i++)
            {
                int SpawnedPositionX = Mathf.RoundToInt(SpawnedCharacters[i].transform.localPosition.x) + MapWidth / 2;
                int SpawnedPositionY = Mathf.RoundToInt(SpawnedCharacters[i].transform.localPosition.y) + MapHeight / 2;
                //Debug.LogError("Checking position: " + SpawnedPositionX + " with " + PositionX + " original: "  + SpawnedCharacters[i].transform.localPosition.x);
                if (PositionX == SpawnedPositionX && PositionY == SpawnedPositionY)
                {
                    return false;
                }
            }
            return IsNonSolid(Data[PositionX, PositionY]); // use meta data later
        }

        private bool IsCharacterAtPosition(Transform MyCharacter, int PositionX, int PositionY)
        {
            int SpawnedPositionX = Mathf.RoundToInt(MyCharacter.localPosition.x) + MapWidth / 2;
            int SpawnedPositionY = Mathf.RoundToInt(MyCharacter.localPosition.y) + MapHeight / 2;
            return (PositionX == SpawnedPositionX && PositionY == SpawnedPositionY);
        }

        public bool IsNonSolid(int TileType)
        {
            return (TileType == 0); // use meta data later
        }

        void OnDrawGizmos()
        {
            if (IsDebugMap && Data != null)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    for (int y = 0; y < MapHeight; y++)
                    {
                        Gizmos.color = (Data[x, y] == 1) ? Color.black : new Color(1, 1, 1, 0.05f);
                        Vector3 pos = new Vector3(-MapWidth / 2 + x + .5f, -MapHeight / 2 + y + .5f, 0);
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }
}