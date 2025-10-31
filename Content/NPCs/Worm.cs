using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.NPCs
{
	public enum WormSegmentType
	{
		/// <summary>
		/// ワームの頭部セグメント。どのワームにも「頭」は1つだけ存在します。
		/// </summary>
		Head,
		/// <summary>
		/// 胴体セグメント。前のセグメントに追従します。
		/// </summary>
		Body,
		/// <summary>
		/// 尻尾セグメント。胴体セグメントと同じAIを持ちます。どのワームにも「尻尾」は1つだけ存在します。
		/// </summary>
		Tail
	}

	/// <summary>
	/// The base class for non-separating Worm enemies.
	/// 分割しないワームの敵のベースクラス。
	/// </summary>
	public abstract class Worm : ModNPC
	{
		/*  ai[] usage:
		 *  ai[] の使用法:
		 *  ai[0] = "follower" segment, the segment that's following this segment
		 *  ai[0] = 「フォロワー」セグメント。このセグメントに追従しているセグメント
		 *  ai[1] = "following" segment, the segment that this segment is following
		 *  ai[1] = 「追従対象」セグメント。このセグメントが追従しているセグメント
		 *  localAI[0] = used when syncing changes to collision detection
		 *  localAI[0] = 衝突検出への変更を同期するときに使用されます
		 *  localAI[1] = checking if Init() was called
		 *  localAI[1] = Init() が呼び出されたかどうかを確認します
		 */

		/// <summary>
		/// Which type of segment this NPC is considered to be
		/// このNPCがどのタイプのセグメントと見なされるか
		/// </summary>
		public abstract WormSegmentType SegmentType { get; }

		/// <summary>
		/// The maximum velocity for the NPC
		/// NPCの最大速度
		/// </summary>
		public float MoveSpeed { get; set; }

		/// <summary>
		/// The rate at which the NPC gains velocity
		/// NPCが速度を得る割合（加速度）
		/// </summary>
		public float Acceleration { get; set; }

		/// <summary>
		/// The NPC instance of the head segment for this worm.
		/// このワームの頭部セグメントのNPCインスタンス。
		/// </summary>
		public NPC HeadSegment => Main.npc[NPC.realLife];

		/// <summary>
		/// The NPC instance of the segment that this segment is following (ai[1]).  For head segments, this property always returns <see langword="null"/>.
		/// このセグメントが追従しているセグメント（ai[1]）のNPCインスタンス。頭部セグメントの場合、このプロパティは常に <see langword="null"/> を返します。
		/// </summary>
		public NPC FollowingNPC => SegmentType == WormSegmentType.Head ? null : Main.npc[(int)NPC.ai[1]];

		/// <summary>
		/// The NPC instance of the segment that is following this segment (ai[0]).  For tail segment, this property always returns <see langword="null"/>.
		/// このセグメントに追従しているセグメント（ai[0]）のNPCインスタンス。尻尾セグメントの場合、このプロパティは常に <see langword="null"/> を返します。
		/// </summary>
		public NPC FollowerNPC => SegmentType == WormSegmentType.Tail ? null : Main.npc[(int)NPC.ai[0]];

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return SegmentType == WormSegmentType.Head ? null : false;
		}

		private bool startDespawning;

		public sealed override bool PreAI() {
			if (NPC.localAI[1] == 0) {
				NPC.localAI[1] = 1f;
				Init();
			}

			if (SegmentType == WormSegmentType.Head) {
				HeadAI();

				if (!NPC.HasValidTarget) {
					NPC.TargetClosest(true);

					// If the NPC is a boss and it has no target, force it to fall to the underworld quickly
					// NPCがボスでターゲットがいない場合、それを強制的に素早くアンダーワールドへ落とします
					if (!NPC.HasValidTarget && NPC.boss) {
						NPC.velocity.Y += 8f;

						MoveSpeed = 1000f;

						if (!startDespawning) {
							startDespawning = true;

							// Despawn after 90 ticks (1.5 seconds) if the NPC gets far enough away
							// NPCが十分に遠ざかった場合、90ティック（1.5秒）後にデスポーンします
							NPC.timeLeft = 90;
						}
					}
				}
			}
			else {
				BodyTailAI();
			}

			return true;
		}

		// Not visible to public API, but is used to indicate what AI to run
		// パブリックAPIからは見えませんが、実行するAIを示すために使用されます
		internal virtual void HeadAI() { }

		internal virtual void BodyTailAI() { }

		public abstract void Init();
	}

	/// <summary>
	/// The base class for head segment NPCs of Worm enemies
	/// ワームの敵の頭部セグメントNPCのベースクラス
	/// </summary>
	public abstract class WormHead : Worm
	{
		public sealed override WormSegmentType SegmentType => WormSegmentType.Head;

		/// <summary>
		/// The NPCID or ModContent.NPCType for the body segment NPCs.<br/>
		/// 胴体セグメントNPCの NPCID または ModContent.NPCType。<br/>
		/// This property is only used if <see cref="HasCustomBodySegments"/> returns <see langword="false"/>.
		/// このプロパティは、<see cref="HasCustomBodySegments"/> が <see langword="false"/> を返す場合にのみ使用されます。
		/// </summary>
		public abstract int BodyType { get; }

		/// <summary>
		/// The NPCID or ModContent.NPCType for the tail segment NPC.<br/>
		/// 尻尾セグメントNPCの NPCID または ModContent.NPCType。<br/>
		/// This property is only used if <see cref="HasCustomBodySegments"/> returns <see langword="false"/>.
		/// このプロパティは、<see cref="HasCustomBodySegments"/> が <see langword="false"/> を返す場合にのみ使用されます。
		/// </summary>
		public abstract int TailType { get; }

		/// <summary>
		/// The minimum amount of segments expected, including the head and tail segments
		/// 頭部と尻尾のセグメントを含め、想定される最小セグメント数
		/// </summary>
		public int MinSegmentLength { get; set; }

		/// <summary>
		/// The maximum amount of segments expected, including the head and tail segments
		/// 頭部と尻尾のセグメントを含め、想定される最大セグメント数
		/// </summary>
		public int MaxSegmentLength { get; set; }

		/// <summary>
		/// Whether the NPC ignores tile collision when attempting to "dig" through tiles, like how Wyverns work.
		/// ワイバーンが動作するように、タイルを「掘り進む」ときにNPCがタイル衝突を無視するかどうか。
		/// </summary>
		public bool CanFly { get; set; }

		/// <summary>
		/// The maximum distance in <b>pixels</b> within which the NPC will use tile collision, if <see cref="CanFly"/> returns <see langword="false"/>.<br/>
		/// NPCがタイル衝突を使用する最大距離（<b>ピクセル</b>単位）。ただし、<see cref="CanFly"/> が <see langword="false"/> を返す場合。<br/>
		/// Defaults to 1000 pixels, which is equivalent to 62.5 tiles.
		/// デフォルトは1000ピクセルで、これは62.5タイルに相当します。
		/// </summary>
		public virtual int MaxDistanceForUsingTileCollision => 1000;

		/// <summary>
		/// Whether the NPC uses 
		/// NPCが使用するかどうか
		/// </summary>
		public virtual bool HasCustomBodySegments => false;

		/// <summary>
		/// If not <see langword="null"/>, this NPC will target the given world position instead of its player target
		/// <see langword="null"/> でない場合、このNPCはプレイヤーのターゲットではなく、指定されたワールド座標をターゲットにします
		/// </summary>
		public Vector2? ForcedTargetPosition { get; set; }

		/// <summary>
		/// Override this method to use custom body-spawning code.<br/>
		/// カスタムの胴体スポーンコードを使用するには、このメソッドをオーバーライドします。<br/>
		/// This method only runs if <see cref="HasCustomBodySegments"/> returns <see langword="true"/>.
		/// このメソッドは、<see cref="HasCustomBodySegments"/> が <see langword="true"/> を返す場合にのみ実行されます。
		/// </summary>
		/// <param name="segmentCount">How many body segments are expected to be spawned</param>
		/// <returns>The whoAmI of the most-recently spawned NPC, which is the result of calling <see cref="NPC.NewNPC(Terraria.DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/></returns>
		/// <param name="segmentCount">スポーンが想定される胴体セグメントの数</param>
		/// <returns>最も新しくスポーンされたNPCの whoAmI。これは <see cref="NPC.NewNPC(Terraria.DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/> の呼び出し結果です</returns>
		public virtual int SpawnBodySegments(int segmentCount) {
			// Defaults to just returning this NPC's whoAmI, since the tail segment uses the return value as its "following" NPC index
			// 尻尾セグメントがこの戻り値を「追従対象」NPCのインデックスとして使用するため、デフォルトではこのNPCの whoAmI を返します
			return NPC.whoAmI;
		}

		/// <summary>
		/// Spawns a body or tail segment of the worm.
		/// ワームの胴体または尻尾セグメントをスポーンします。
		/// </summary>
		/// <param name="source">The spawn source</param>
		/// <param name="type">The ID of the segment NPC to spawn</param>
		/// <param name="latestNPC">The whoAmI of the most-recently spawned segment NPC in the worm, including the head</param>
		/// <returns></returns>
		/// <param name="source">スポーン元</param>
		/// <param name="type">スポーンするセグメントNPCのID</param>
		/// <param name="latestNPC">頭部を含む、ワーム内で最も新しくスポーンされたセグメントNPCの whoAmI</param>
		/// <returns></returns>
		protected int SpawnSegment(IEntitySource source, int type, int latestNPC) {
			// We spawn a new NPC, setting latestNPC to the newer NPC, whilst also using that same variable
			// to set the parent of this new NPC. The parent of the new NPC (may it be a tail or body part)
			// will determine the movement of this new NPC.
			// 新しいNPCをスポーンし、latestNPC をより新しいNPCに設定すると同時に、その同じ変数を使用して
			// この新しいNPCの親を設定します。新しいNPCの親（それが尻尾または胴体パーツである可能性があります）
			// が、この新しいNPCの動きを決定します。
			// Under there, we also set the realLife value of the new NPC, because of what is explained above.
			// その下で、上記で説明した理由により、新しいNPCの realLife 値も設定します。
			int oldLatest = latestNPC;
			latestNPC = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latestNPC);

			Main.npc[oldLatest].ai[0] = latestNPC;

			NPC latest = Main.npc[latestNPC];
			// NPC.realLife is the whoAmI of the NPC that the spawned NPC will share its health with
			// NPC.realLife は、スポーンされたNPCがヘルスを共有するNPCの whoAmI です
			latest.realLife = NPC.whoAmI;

			return latestNPC;
		}

		internal sealed override void HeadAI() {
			HeadAI_SpawnSegments();

			bool collision = HeadAI_CheckCollisionForDustSpawns();

			HeadAI_CheckTargetDistance(ref collision);

			HeadAI_Movement(collision);
		}

		private void HeadAI_SpawnSegments() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				// So, we start the AI off by checking if NPC.ai[0] (the following NPC's whoAmI) is 0.
				// そこで、まず NPC.ai[0]（追従するNPCの whoAmI）が 0 かどうかを確認してAIを開始します。
				// This is practically ALWAYS the case with a freshly spawned NPC, so this means this is the first update.
				// これは新しくスポーンされたNPCでは実質的に常にそうであるため、これが最初の更新であることを意味します。
				// Since this is the first update, we can safely assume we need to spawn the rest of the worm (bodies + tail).
				// これが最初の更新であるため、ワームの残りの部分（胴体 + 尻尾）をスポーンする必要があると安全に推測できます。
				bool hasFollower = NPC.ai[0] > 0;
				if (!hasFollower) {
					// So, here we assign the NPC.realLife value.
					// そこで、ここで NPC.realLife の値を割り当てます。
					// The NPC.realLife value is mainly used to determine which NPC loses life when we hit this NPC.
					// NPC.realLife の値は、このNPCを攻撃したときにどのNPCがライフを失うかを決定するために主に使用されます。
					// We don't want every single piece of the worm to have its own HP pool, so this is a neat way to fix that.
					// ワームのすべてのピースが独自のHPプールを持つことを望まないため、これはそれを解決するための巧妙な方法です。
					NPC.realLife = NPC.whoAmI;
					// latestNPC is going to be used in SpawnSegment() and I'll explain it there.
					// latestNPC は SpawnSegment() で使用され、そこで説明します。
					int latestNPC = NPC.whoAmI;

					// Here we determine the length of the worm.
					// ここでワームの長さを決定します。
					int randomWormLength = Main.rand.Next(MinSegmentLength, MaxSegmentLength + 1);

					int distance = randomWormLength - 2;

					IEntitySource source = NPC.GetSource_FromAI();

					if (HasCustomBodySegments) {
						// Call the method that'll handle spawning the body segments
						// 胴体セグメントのスポーンを処理するメソッドを呼び出します
						latestNPC = SpawnBodySegments(distance);
					}
					else {
						// Spawn the body segments like usual
						// いつも通り胴体セグメントをスポーンします
						while (distance > 0) {
							latestNPC = SpawnSegment(source, BodyType, latestNPC);
							distance--;
						}
					}

					// Spawn the tail segment
					// 尻尾セグメントをスポーンします
					SpawnSegment(source, TailType, latestNPC);

					NPC.netUpdate = true;

					// Ensure that all of the segments could spawn.  If they could not, despawn the worm entirely
					// すべてのセグメントがスポーンできたことを確認します。できなかった場合、ワーム全体をデスポーンします
					int count = 0;
					foreach (var n in Main.ActiveNPCs) {
						if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
							count++;
					}

					if (count != randomWormLength) {
						// Unable to spawn all of the segments... kill the worm
						// すべてのセグメントをスポーンできませんでした... ワームをキルします
						foreach (var n in Main.ActiveNPCs) {
							if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI) {
								n.active = false;
								n.netUpdate = true;
							}
						}
					}

					// Set the player target for good measure
					// 念のためプレイヤーのターゲットを設定します
					NPC.TargetClosest(true);
				}
			}
		}

		private bool HeadAI_CheckCollisionForDustSpawns() {
			int minTilePosX = (int)(NPC.Left.X / 16) - 1;
			int maxTilePosX = (int)(NPC.Right.X / 16) + 2;
			int minTilePosY = (int)(NPC.Top.Y / 16) - 1;
			int maxTilePosY = (int)(NPC.Bottom.Y / 16) + 2;

			// Ensure that the tile range is within the world bounds
			// タイルの範囲がワールド境界内にあることを確認します

			bool collision = false;

			// This is the initial check for collision with tiles.
			// これはタイルとの衝突の最初のチェックです。
			for (int i = minTilePosX; i < maxTilePosX; ++i) {
				for (int j = minTilePosY; j < maxTilePosY; ++j) {
					Tile tile = Main.tile[i, j];

					// If the tile is solid or is considered a platform, then there's valid collision
					// タイルがソリッドであるか、プラットフォームと見なされる場合、有効な衝突があります
					if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0) || tile.LiquidAmount > 64) {
						Vector2 tileWorld = new Point16(i, j).ToWorldCoordinates(0, 0);

						if (NPC.Right.X > tileWorld.X && NPC.Left.X < tileWorld.X + 16 && NPC.Bottom.Y > tileWorld.Y && NPC.Top.Y < tileWorld.Y + 16) {
							// Collision found
							// 衝突が見つかりました

							if (Main.rand.NextBool(100))
								WorldGen.KillTile(i, j, fail: true, effectOnly: true, noItem: false);
						}
					}
				}
			}

			return collision;
		}

		private void HeadAI_CheckTargetDistance(ref bool collision) {
			// If there is no collision with tiles, we check if the distance between this NPC and its target is too large, so that we can still trigger "collision".
			// タイルとの衝突がない場合、このNPCとターゲット間の距離が大きすぎるかどうかを確認し、それでも「衝突」をトリガーできるようにします。
			if (!collision) {
				Rectangle hitbox = NPC.Hitbox;

				int maxDistance = MaxDistanceForUsingTileCollision;

				bool tooFar = true;

				foreach (var player in Main.ActivePlayers) {
					Rectangle areaCheck;

					if (ForcedTargetPosition is Vector2 target)
						areaCheck = new Rectangle((int)target.X - maxDistance, (int)target.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					else if (!player.dead && !player.ghost)
						areaCheck = new Rectangle((int)player.position.X - maxDistance, (int)player.position.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					else
						continue;  // Not a valid player
								   // 有効なプレイヤーではありません

					if (hitbox.Intersects(areaCheck)) {
						tooFar = false;
						break;
					}
				}

				if (tooFar)
					collision = true;
			}
		}

		private void HeadAI_Movement(bool collision) {
			// MoveSpeed determines the max speed at which this NPC can move.
			// MoveSpeed は、このNPCが移動できる最大速度を決定します。
			// Higher value = faster speed.
			// 値が大きいほど、速度が速くなります。
			float speed = MoveSpeed;
			// acceleration is exactly what it sounds like. The speed at which this NPC accelerates.
			// acceleration は文字通りその通りです。このNPCが加速する速度です。
			float acceleration = Acceleration;

			float targetXPos, targetYPos;

			Player playerTarget = Main.player[NPC.target];

			Vector2 forcedTarget = ForcedTargetPosition ?? playerTarget.Center;
			// Using a ValueTuple like this allows for easy assignment of multiple values
			// このように ValueTuple を使用すると、複数の値を簡単に割り当てることができます
			(targetXPos, targetYPos) = (forcedTarget.X, forcedTarget.Y);

			// Copy the value, since it will be clobbered later
			// 後で上書きされるため、値をコピーします
			Vector2 npcCenter = NPC.Center;

			float targetRoundedPosX = (float)((int)(targetXPos / 16f) * 16);
			float targetRoundedPosY = (float)((int)(targetYPos / 16f) * 16);
			npcCenter.X = (float)((int)(npcCenter.X / 16f) * 16);
			npcCenter.Y = (float)((int)(npcCenter.Y / 16f) * 16);
			float dirX = targetRoundedPosX - npcCenter.X;
			float dirY = targetRoundedPosY - npcCenter.Y;

			float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

			// If we do not have any type of collision, we want the NPC to fall down and de-accelerate along the X axis.
			// いかなるタイプの衝突もない場合、NPCを落下させ、X軸に沿って減速させたいと考えます。
			if (!collision && !CanFly)
				HeadAI_Movement_HandleFallingFromNoCollision(dirX, speed, acceleration);
			else {
				// Else we want to play some audio (soundDelay) and move towards our target.
				// そうでない場合、オーディオ（soundDelay）を再生し、ターゲットに向かって移動したいと考えます。
				HeadAI_Movement_PlayDigSounds(length);

				HeadAI_Movement_HandleMovement(dirX, dirY, length, speed, acceleration);
			}

			HeadAI_Movement_SetRotation(collision);
		}

		private void HeadAI_Movement_HandleFallingFromNoCollision(float dirX, float speed, float acceleration) {
			// Keep searching for a new target
			// 新しいターゲットを探し続けます
			NPC.TargetClosest(true);

			// Constant gravity of 0.11 pixels/tick
			// 0.11 ピクセル/ティックの一定の重力
			NPC.velocity.Y += 0.11f;

			// Ensure that the NPC does not fall too quickly
			// NPCが速く落ちすぎないようにします
			if (NPC.velocity.Y > speed)
				NPC.velocity.Y = speed;

			// The following behavior mimics vanilla worm movement
			// 以下の動作はバニラのワームの動きを模倣しています
			if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.4f) {
				// Velocity is sufficiently fast, but not too fast
				// 速度は十分に速いが、速すぎない
				if (NPC.velocity.X < 0.0f)
					NPC.velocity.X -= acceleration * 1.1f;
				else
					NPC.velocity.X += acceleration * 1.1f;
			}
			else if (NPC.velocity.Y == speed) {
				// NPC has reached terminal velocity
				// NPCは終端速度に達しました
				if (NPC.velocity.X < dirX)
					NPC.velocity.X += acceleration;
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= acceleration;
			}
			else if (NPC.velocity.Y > 4) {
				if (NPC.velocity.X < 0)
					NPC.velocity.X += acceleration * 0.9f;
				else
					NPC.velocity.X -= acceleration * 0.9f;
			}
		}

		private void HeadAI_Movement_PlayDigSounds(float length) {
			if (NPC.soundDelay == 0) {
				// Play sounds quicker the closer the NPC is to the target location
				// ターゲット位置にNPCが近いほど、より速く音を再生します
				float num1 = length / 40f;

				if (num1 < 10)
					num1 = 10f;

				if (num1 > 20)
					num1 = 20f;

				NPC.soundDelay = (int)num1;

				SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
			}
		}

		private void HeadAI_Movement_HandleMovement(float dirX, float dirY, float length, float speed, float acceleration) {
			float absDirX = Math.Abs(dirX);
			float absDirY = Math.Abs(dirY);
			float newSpeed = speed / length;
			dirX *= newSpeed;
			dirY *= newSpeed;

			if ((NPC.velocity.X > 0 && dirX > 0) || (NPC.velocity.X < 0 && dirX < 0) || (NPC.velocity.Y > 0 && dirY > 0) || (NPC.velocity.Y < 0 && dirY < 0)) {
				// The NPC is moving towards the target location
				// NPCはターゲット位置に向かって移動しています
				if (NPC.velocity.X < dirX)
					NPC.velocity.X += acceleration;
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= acceleration;

				if (NPC.velocity.Y < dirY)
					NPC.velocity.Y += acceleration;
				else if (NPC.velocity.Y > dirY)
					NPC.velocity.Y -= acceleration;

				// The intended Y-velocity is small AND the NPC is moving to the left and the target is to the right of the NPC or vice versa
				// 意図されたY速度が小さい AND NPCが左に移動していてターゲットがNPCの右にある、またはその逆の場合
				if (Math.Abs(dirY) < speed * 0.2 && ((NPC.velocity.X > 0 && dirX < 0) || (NPC.velocity.X < 0 && dirX > 0))) {
					if (NPC.velocity.Y > 0)
						NPC.velocity.Y += acceleration * 2f;
					else
						NPC.velocity.Y -= acceleration * 2f;
				}

				// The intended X-velocity is small AND the NPC is moving up/down and the target is below/above the NPC
				// 意図されたX速度が小さい AND NPCが上下に移動していてターゲットがNPCの下/上にある場合
				if (Math.Abs(dirX) < speed * 0.2 && ((NPC.velocity.Y > 0 && dirY < 0) || (NPC.velocity.Y < 0 && dirY > 0))) {
					if (NPC.velocity.X > 0)
						NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
					else
						NPC.velocity.X = NPC.velocity.X - acceleration * 2f;
				}
			}
			else if (absDirX > absDirY) {
				// The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
				// X距離がY距離よりも大きい。X軸に沿った動きをより強くします
				if (NPC.velocity.X < dirX)
					NPC.velocity.X += acceleration * 1.1f;
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= acceleration * 1.1f;

				if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5) {
					if (NPC.velocity.Y > 0)
						NPC.velocity.Y += acceleration;
					else
						NPC.velocity.Y -= acceleration;
				}
			}
			else {
				// The X distance is larger than the Y distance.  Force movement along the X-axis to be stronger
				// X距離がY距離よりも大きい。X軸に沿った動きをより強くします
				if (NPC.velocity.Y < dirY)
					NPC.velocity.Y += acceleration * 1.1f;
				else if (NPC.velocity.Y > dirY)
					NPC.velocity.Y -= acceleration * 1.1f;

				if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5) {
					if (NPC.velocity.X > 0)
						NPC.velocity.X += acceleration;
					else
						NPC.velocity.X -= acceleration;
				}
			}
		}

		private void HeadAI_Movement_SetRotation(bool collision) {
			// Set the correct rotation for this NPC.
			// このNPCの正しい回転を設定します。
			// Assumes the sprite for the NPC points upward.  You might have to modify this line to properly account for your NPC's orientation
			// NPCのスプライトが上を向いていると仮定しています。NPCの向きを適切に考慮するために、この行を修正する必要があるかもしれません
			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

			// Some netupdate stuff (multiplayer compatibility).
			// いくつかのネットアップデート処理（マルチプレイヤー互換性）。
			if (collision) {
				if (NPC.localAI[0] != 1)
					NPC.netUpdate = true;

				NPC.localAI[0] = 1f;
			}
			else {
				if (NPC.localAI[0] != 0)
					NPC.netUpdate = true;

				NPC.localAI[0] = 0f;
			}

			// Force a netupdate if the NPC's velocity changed sign and it was not "just hit" by a player
			// NPCの速度が符号を変え、プレイヤーによって「ちょうどヒットされた」わけではない場合、ネットアップデートを強制します
			if (((NPC.velocity.X > 0 && NPC.oldVelocity.X < 0) || (NPC.velocity.X < 0 && NPC.oldVelocity.X > 0) || (NPC.velocity.Y > 0 && NPC.oldVelocity.Y < 0) || (NPC.velocity.Y < 0 && NPC.oldVelocity.Y > 0)) && !NPC.justHit)
				NPC.netUpdate = true;
		}
	}

	public abstract class WormBody : Worm
	{
		public sealed override WormSegmentType SegmentType => WormSegmentType.Body;

		internal override void BodyTailAI() {
			CommonAI_BodyTail(this);
		}

		internal static void CommonAI_BodyTail(Worm worm) {
			if (!worm.NPC.HasValidTarget)
				worm.NPC.TargetClosest(true);

			if (Main.player[worm.NPC.target].dead && worm.NPC.timeLeft > 30000)
				worm.NPC.timeLeft = 10;

			NPC following = worm.NPC.ai[1] >= Main.maxNPCs ? null : worm.FollowingNPC;
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				// Some of these conditions are possible if the body/tail segment was spawned individually
				// これらの条件の一部は、胴体/尻尾セグメントが個別にスポーンされた場合に発生する可能性があります
				// Kill the segment if the segment NPC it's following is no longer valid
				// 追従しているセグメントNPCが有効でなくなった場合、そのセグメントをキルします
				if (following is null || !following.active || following.friendly || following.townNPC || following.lifeMax <= 5) {
					worm.NPC.life = 0;
					worm.NPC.HitEffect(0, 10);
					worm.NPC.active = false;
				}
			}

			if (following is not null) {
				// Follow behind the segment "in front" of this NPC
				// このNPCの「前」にあるセグメントの後ろに追従します
				// Use the current NPC.Center to calculate the direction towards the "parent NPC" of this NPC.
				// 現在の NPC.Center を使用して、このNPCの「親NPC」への方向を計算します。
				float dirX = following.Center.X - worm.NPC.Center.X;
				float dirY = following.Center.Y - worm.NPC.Center.Y;
				// We then use Atan2 to get a correct rotation towards that parent NPC.
				// 次に Atan2 を使用して、その親NPCへの正しい回転を取得します。
				// Assumes the sprite for the NPC points upward.  You might have to modify this line to properly account for your NPC's orientation
				// NPCのスプライトが上を向いていると仮定しています。NPCの向きを適切に考慮するために、この行を修正する必要があるかもしれません
				worm.NPC.rotation = (float)Math.Atan2(dirY, dirX) + MathHelper.PiOver2;
				// We also get the length of the direction vector.
				// また、方向ベクトルの長さを取得します。
				float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
				// We calculate a new, correct distance.
				// 新しい、正しい距離を計算します。
				float dist = (length - worm.NPC.width) / length;
				float posX = dirX * dist;
				float posY = dirY * dist;

				// Reset the velocity of this NPC, because we don't want it to move on its own
				// このNPCが単独で動くことを望まないため、このNPCの速度をリセットします
				worm.NPC.velocity = Vector2.Zero;
				// And set this NPCs position accordingly to that of this NPCs parent NPC.
				// そして、このNPCの親NPCの位置に合わせて、このNPCの位置を設定します。
				worm.NPC.position.X += posX;
				worm.NPC.position.Y += posY;
			}
		}
	}

	// Since the body and tail segments share the same AI
	// 胴体セグメントと尻尾セグメントは同じAIを共有するため
	public abstract class WormTail : Worm
	{
		public sealed override WormSegmentType SegmentType => WormSegmentType.Tail;

		internal override void BodyTailAI() {
			WormBody.CommonAI_BodyTail(this);
		}
	}
}