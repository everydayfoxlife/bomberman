﻿using UnityEngine;
using System.Collections;
using Pixelbox;
using Bomberman.Tiles;

public class Stage : MonoBehaviour {

	private const int TILE_SIZE = 16;

	public GameObject[] tilesheet;

	[HideInInspector] public int width;
	[HideInInspector] public int height;
	[HideInInspector] public int[,] grid;
	[HideInInspector] public Tile[,] tiles;

	public static Stage instance;

	private static Tile empty = new Tile(0, 0, true);

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	public void Init(string stage) {
		Map map = Maps.GetMap(stage);

		width  = map.width;
		height = map.height;
		grid   = new int[width, height];
		tiles  = new Tile[width, height];

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				MapItem item = map.Get(i, j);
				if (item == null) {
					// FIXME MonoBehaviour cannot be created with 'new' keyword. should use AddComponent()
					tiles[i, j] = new Tile(i, j, true); // empty tile
					continue;
				}
				GameObject tileObj = (GameObject)Instantiate(tilesheet[item.sprite], new Vector3(i * TILE_SIZE, -j * TILE_SIZE, 0f), Quaternion.identity);
				tiles[i, j] = tileObj.GetComponent<Tile>();
				tiles[i, j].Init(item, this);
				tileObj.transform.SetParent(transform);
			}
		}

		instance = this;
		Camera.main.GetComponent<CameraMan>().InitPosition(this);
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	// This function is primaly used by players to add bombs on the stage
	public Tile AddTile(int i, int j, int item) {
		if (i < 0 || j < 0 || i >= width || j >= height) return empty;
		GameObject tileObj = (GameObject)Instantiate(tilesheet[item], new Vector3(i * TILE_SIZE, -j * TILE_SIZE, 0f), Quaternion.identity);
		tiles[i, j] = tileObj.GetComponent<Tile>();
		//tiles[i, j].Init(item, this); // TODO
		tileObj.transform.SetParent(transform);
		return tiles[i, j];
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	public void SetTile(int i, int j, Tile tile = null) {
		if (i < 0 || j < 0 || i >= width || j >= height) return;
		if (tile == null) tile = empty; 
		tiles[i, j] = tile;
	}

	//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
	public Tile GetTile(int i, int j) {
		if (i < 0 || j < 0 || i >= width || j >= height) return empty;
		return tiles[i, j];
	}
}
