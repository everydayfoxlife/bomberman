﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pixelbox;
using Bomberman.Tiles;

namespace Bomberman.Entities {
	public class Player : MonoBehaviour {

		public GameObject bombPrefab;

		// dimensions and speed are stored in millipixels
		private const int PIXEL =  1000;
		private const int WIDTH =  8000;
		private const int TILE  = 16000;
		private const int FACE  =  7800;
		private const int SIDE  =  7800;

		// player properties
		public int speed;
		public int flame;
		public int bomb;
		public bool canKick;
		public bool canThrow;
		public bool canPunch;

		public bool isInvicible;
		public bool hasDisease;

		[HideInInspector] public int id; // bomberman number (0 - 3)
		[HideInInspector] public bool alive;

		[HideInInspector] public int i; // position in tile
		[HideInInspector] public int j;

		[HideInInspector] private int x; // position in pixels
		[HideInInspector] private int y;

		[HideInInspector] public int droppedBomb; // how many bombs this bomberman have on the stage

		private int joystick; // number of the joystick that control this bomberman

		private Stage stage;
		private SpriteAnimator animator;
		private string facing;
		private bool walking;

		private List<Tile> collectedPowerups = new List<Tile>();

		//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
		public void Init(int id, MapItem spawnpoint, string variation, Stage stage) {
			this.id = id;
			this.stage = stage;
			joystick = id + 1;
			animator = new SpriteAnimator("men", variation, GetComponent<SpriteRenderer>());
			x = WIDTH + TILE * spawnpoint.x;
			y = WIDTH + TILE * spawnpoint.y;
		}

		//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
		// Use this for initialization
		void Start() {
			alive = true;
			facing = "Down";
			walking = false;
			droppedBomb = 0;
			animator.Start("stand" + facing);
		}

		//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
		// Update is called once per frame
		void Update() {
			if (!alive) return;

			// read joystick inputs
			bool goR = Input.GetAxis("joy" + joystick + "_H") > 0;
			bool goL = Input.GetAxis("joy" + joystick + "_H") < 0;
			bool goU = Input.GetAxis("joy" + joystick + "_V") > 0;
			bool goD = Input.GetAxis("joy" + joystick + "_V") < 0;

			// speed on x and y axis
			int sx = 0;
			int sy = 0;

			string previousFacing = facing;

			if (goL) { sx -= speed; facing = "Left";  }
			if (goR) { sx += speed; facing = "Right"; }
			if (goU) { sy -= speed; facing = "Up";    }
			if (goD) { sy += speed; facing = "Down";  }

			// update animation
			if (sx == 0 && sy == 0) {
				if (walking) {
					animator.Start("stand" + facing);
					walking = false;
				}
			} else if (!walking) {
				animator.Start("walk" + facing);
				walking = true;
			} else if (facing != previousFacing) {
				animator.Start(( walking ? "walk" : "stand") + facing);
			}
			animator.Play();

			// target position (in millipixels)
			int tx = x + sx;
			int ty = y + sy;

			// final position (in millipixels)
			int fx = tx;
			int fy = ty;

			Tile tile;
			Tile tileA;
			Tile tileB;


			// TODO avoid bomberman to get stuck on corners when user go in diagonal

			//-----------------------------------------------
			// horizontal movement
			if (sx > 0) {
				int ti = (tx + FACE) / TILE;
				tileA = stage.GetTile(ti, (ty - SIDE) / TILE);
				tileB = stage.GetTile(ti, (ty + SIDE) / TILE);
				if ((!tileA.isWalkable || !tileB.isWalkable) && (x + FACE) / TILE < ti) {
					
					// if there is a walkable tile then make bomberman slide in front of the entrance
					if (tileA.isWalkable) fy = Mathf.Max(y - speed, ((y - WIDTH) / TILE    ) * TILE + WIDTH);
					if (tileB.isWalkable) fy = Mathf.Min(y + speed, ((y + WIDTH) / TILE + 1) * TILE - WIDTH);

					// snap player to the border of the tile
					fx = ti * TILE - WIDTH;
				}
			} else if (sx < 0) {
				int ti = (tx - FACE) / TILE;
				tileA = stage.GetTile(ti, (ty - SIDE) / TILE);
				tileB = stage.GetTile(ti, (ty + SIDE) / TILE);
				if ((!tileA.isWalkable || !tileB.isWalkable) && (x - FACE) / TILE > ti) {

					// if there is a walkable tile then make bomberman slide in front of the entrance
					if (tileA.isWalkable) fy = Mathf.Max(y - speed, ((y - WIDTH) / TILE    ) * TILE + WIDTH);
					if (tileB.isWalkable) fy = Mathf.Min(y + speed, ((y + WIDTH) / TILE + 1) * TILE - WIDTH);

					// snap player to the border of the tile
					fx = (ti + 1) * TILE + FACE;
				}
			}

			//-----------------------------------------------
			// vertical movement
			if (sy > 0) {
				int tj = (ty + FACE) / TILE;
				tileA = stage.GetTile((fx - SIDE) / TILE, tj);
				tileB = stage.GetTile((fx + SIDE) / TILE, tj);
				if ((!tileA.isWalkable || !tileB.isWalkable) && (y + FACE) / TILE < tj) {

					// if there is a walkable tile then make bomberman slide in front of the entrance
					if (tileA.isWalkable) fx = Mathf.Max(x - speed, ((x - WIDTH) / TILE    ) * TILE + WIDTH);
					if (tileB.isWalkable) fx = Mathf.Min(x + speed, ((x + WIDTH) / TILE + 1) * TILE - WIDTH);

					// snap player to the border of the tile
					fy = tj * TILE - WIDTH;
				}
			} else if (sy < 0) {
				int tj = (ty - FACE) / TILE;
				tileA = stage.GetTile((fx - SIDE) / TILE, tj);
				tileB = stage.GetTile((fx + SIDE) / TILE, tj);
				if ((!tileA.isWalkable || !tileB.isWalkable) && (y - FACE) / TILE > tj) {

					// if there is a walkable tile then make bomberman slide in front of the entrance
					if (tileA.isWalkable) fx = Mathf.Max(x - speed, ((x - WIDTH) / TILE) * TILE + WIDTH);
					if (tileB.isWalkable) fx = Mathf.Min(x + speed, ((x + WIDTH) / TILE + 1) * TILE - WIDTH);

					// snap player to the border of the tile
					fy = (tj + 1) * TILE + FACE;
				}
			}

			// fetch position
			x = fx;
			y = fy;
			transform.position = new Vector3(x / PIXEL, -y / PIXEL + 22, 0);

			i = x / TILE;
			j = y / TILE;

			tile = stage.GetTile(i, j);

			// drop a bomb on stage
			if (Input.GetButtonDown("joy" + joystick + "_A") && droppedBomb < bomb) {
				if (tile.isEmpty) ((Bomb)stage.AddTile(i, j, bombPrefab)).Init(i, j, flame, 120, this);
				droppedBomb += 1;
			}

			// stands on flame behaviour
			if (!isInvicible && tile.GetType() == typeof(Flame)) {
				StartCoroutine(DeathAnimCoroutine());
			}

			// stands on collectable
			if (tile.GetType() == typeof(Powerup)) {
				CollectItem((Powerup)tile);
			}
		}

		//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
		private void CollectItem(Powerup powerup) {
			collectedPowerups.Add(powerup);
			switch (powerup.code) {
				case PowerupCode.BOMB:  if (bomb  < Game.instance.MAX_BOMB)  bomb  += 1; break;
				case PowerupCode.FLAME: if (flame < Game.instance.MAX_FLAME) flame += 1; break;
				case PowerupCode.SPEED: if (speed < Game.instance.MAX_SPEED) speed += Game.instance.SPEED_INC; break;
				case PowerupCode.KICK:  canKick  = true; break;
				case PowerupCode.PUNCH: canPunch = true; break;
				case PowerupCode.THROW: canThrow = true; break;
				case PowerupCode.SUPER_FLAME: flame = Game.instance.MAX_FLAME; break;
				default: break;
			}
			powerup.Remove();
		}

		//▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄
		protected IEnumerator DeathAnimCoroutine() {
			alive = false;
			Game.instance.PlayerDeath(this);

			animator.Start("death");
			int duration = animator.GetDuration();

			for (int c = 0; c < duration; c++) {
				animator.Play();
				yield return new WaitForEndOfFrame();
			}

			yield return new WaitForSeconds(0.7f);
			GetComponent<SpriteRenderer>().sprite = null;

			// TODO spawn collected items in stage
			for (int i = 0; i < collectedPowerups.Count; i++) {
				Powerup powerup = (Powerup)collectedPowerups[i];
				Tile position = stage.GetTile(powerup.i, powerup.j);
				// TODO check that there is no entities
				if (position.isEmpty) {
					stage.AddTile(powerup.i, powerup.j, powerup.prefab);
				} else {
					// find a random valid position
					Vector2 newPosition = stage.GetEmptyPosition();
					stage.AddTile((int)newPosition.x, (int)newPosition.y, powerup.prefab);
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
