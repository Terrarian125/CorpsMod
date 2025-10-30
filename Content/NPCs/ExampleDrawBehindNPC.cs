//サンプルアイテムをもって右クリックすると爆発するNPCの例
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.NPCs
{
	// このNPCは、単純に**DrawBehind**メソッドの動作を示すための展示品です。
	// このNPCは、ModNPCが描画できるすべての利用可能な「レイヤー」（層）を順番に切り替えます。
	// **Cheat Sheet**や**Hero's Mod**などのツールを使ってこのNPCをスポーンさせると、その効果を確認できます。
	// さらに、このNPCは**PreHoverInteract**フックを使用して、カスタムの右クリックおよびホバー操作を示します。この例は、DrawBehindの展示とは無関係です。
	public class ExampleDrawBehindNPC : ModNPC
	{
		public override void SetStaticDefaults() {
			// アニメーションフレームの総数
			Main.npcFrameCount[NPC.type] = 6;
		}

		public override void SetDefaults() {
			NPC.width = 30; // NPCのヒットボックスの幅
			NPC.height = 40; // NPCのヒットボックスの高さ
			NPC.aiStyle = -1; // カスタムAIを使用
			NPC.damage = 0; // このNPCが衝突時に与えるダメージ量
			NPC.defense = 2; // このNPCのダメージ耐性
			NPC.lifeMax = 100; // このNPCの最大ライフ
			NPC.HitSound = SoundID.NPCHit2; // このNPCがヒットしたときに再生される音
			NPC.DeathSound = SoundID.NPCDeath2; // このNPCが死亡したときに再生される音
			NPC.noGravity = true; // trueの場合、NPCは重力の影響を受けません
			NPC.noTileCollide = true; // trueの場合、NPCはタイルと衝突しません
			NPC.knockBackResist = 0f; // 受けるノックバックが実際に適用される割合。1f: フルノックバック; 0f: ノックバックなし
		}

		// 現在の描画レイヤーは40ティックごとに変化します
		private int CurrentLayer => (int)(NPC.ai[0] / 40);

		// これは、現在のレイヤーに応じて、このNPCのテクスチャから描画されるフレームを変更します
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = CurrentLayer * frameHeight;
		}

		public override void AI() {
			NPC.ai[0] = (NPC.ai[0] + 1) % 240;

			// これらは通常の描画（ケース3）のデフォルト設定です
			NPC.hide = false;
			NPC.behindTiles = false;

			switch (CurrentLayer) {
				case 0:
				case 1:
				case 4:
				case 5:
					NPC.hide = true;
					break;
				case 2:
					NPC.behindTiles = true;
					break;
				case 3:
					break;
			}
		}

		// このメソッドを使用すると、このNPCを特定の要素の**背後**に描画するように指定できます。
		public override void DrawBehind(int index) {
			// 利用可能な6つの位置は次のとおりです:
			switch (CurrentLayer) {
				case 0: // タイルと壁の背後
					Main.instance.DrawCacheNPCsMoonMoon.Add(index);
					break;
				case 1: // 非ソリッドなタイル（例：草）の背後だが、壁の前
					Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
					break;
				case 2: // タイルの背後だが、非ソリッドなタイルの前
				case 3: // 通常（タイルの前）
					break;
				case 4: // すべての通常のNPCの前（実質的には、NPCによって生成された投射物のレイヤー）
					Main.instance.DrawCacheNPCProjectiles.Add(index);
					break;
				case 5: // プレイヤーの前
					Main.instance.DrawCacheNPCsOverPlayers.Add(index);
					break;
			}
		}

		// **PreHoverInteract**は、このNPCにカスタムの右クリック操作とホバー動作を与えます。この例は、DrawBehindの展示とは無関係です。
		public override bool PreHoverInteract(bool mouseIntersects) {
			if (CurrentLayer < 3) {
				return true;
			}

			// このコードは、プレイヤーがOld Shaking Chest NPCを右クリックしてGolden Keyを消費し、Elder Slimeに変身させるコードと似ています。
			Player player = Main.LocalPlayer;
			int keyItem = ModContent.ItemType<Items.Placeable.ExampleBlock>();
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = keyItem;
			player.cursorItemIconText = "";
			player.noThrow = 2;

			// このコードは、このメソッドで通常のNPC相互作用範囲を強制したい場合に使用できます。
			/*
			Rectangle interactionRange = new Rectangle((int)(player.Center.X - Player.tileRangeX * 16), (int)(player.Center.Y - Player.tileRangeY * 16), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2);
			if (!interactionRange.Intersects(NPC.getRect())) {
				return false;
			}
			*/

			if (!player.dead) {
				PlayerInput.SetZoom_MouseInWorld();
				if (Main.mouseRight && Main.npcChatRelease) {
					Main.npcChatRelease = false;
					if (PlayerInput.UsingGamepad) {
						player.releaseInventory = false;
					}

					if (player.talkNPC != NPC.whoAmI && !player.tileInteractionHappened) {
						if (player.HasItem(keyItem) && player.ConsumeItem(keyItem)) {
							SoundEngine.PlaySound(SoundID.Item14); // 爆弾の爆発音
							NPC.SimpleStrikeNPC(1000, 0); // NPCを即座に倒します（爆発させます）
						}
						else {
							SoundEngine.PlaySound(SoundID.MenuClose);
						}
					}
				}
			}

			return false; // バニラの（通常の）処理をスキップします。
		}

		// PreHoverInteractがチャットコードをバイパスするため、実際にはチャットする意図がなくても、CanChatを使用してスマートインタラクトがこのNPCをターゲットにするように強制できます。
		public override bool CanChat() {
			if (CurrentLayer < 3) {
				return base.CanChat();
			}

			return true;
		}
	}
}

////サンプルアイテムをもって右クリックすると爆発するNPCの例
//using Terraria;
//using Terraria.Audio;
//using Terraria.GameInput;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace CorpsMod.Content.NPCs
//{
//	// This NPC is simply an exhibition of the DrawBehind method.
//	// The npc cycles between all the available "layers" that a ModNPC can be drawn at.
//	// Spawn this NPC with something like Cheat Sheet or Hero's Mod to view the effect.
//	// In addition, this NPC showcases the PreHoverInteract hook demonstrating a custom right click and hover interaction. This example is unrelated to DrawBehind exhibition.
//	public class ExampleDrawBehindNPC : ModNPC
//	{
//		public override void SetStaticDefaults() {
//			// Total count animation frames
//			Main.npcFrameCount[NPC.type] = 6;
//		}

//		public override void SetDefaults() {
//			NPC.width = 30; // The width of the npc hitbox
//			NPC.height = 40; // The height of the npc hitbox
//			NPC.aiStyle = -1; // Using custom AI
//			NPC.damage = 0; // The amount of damage this NPC will deal on collision
//			NPC.defense = 2; // How resistant to damage this NPC is
//			NPC.lifeMax = 100; // The maximum life of this NPC
//			NPC.HitSound = SoundID.NPCHit2; // The sound that plays when this npc is hit
//			NPC.DeathSound = SoundID.NPCDeath2; // The sound that plays when this npc dies
//			NPC.noGravity = true; // If true, the npc will not be affected by gravity
//			NPC.noTileCollide = true; // If true, the npc does not collide with tiles
//			NPC.knockBackResist = 0f; // How much of the knockback it receives will actually apply. 1f: full knockback; 0f: no knockback
//		}

//		// The current drawing layer will change every 40 ticks
//		private int CurrentLayer => (int)(NPC.ai[0] / 40);

//		// This changes the frame from the this NPC's texture that is drawn, depending on the current layer
//		public override void FindFrame(int frameHeight) {
//			NPC.frame.Y = CurrentLayer * frameHeight;
//		}

//		public override void AI() {
//			NPC.ai[0] = (NPC.ai[0] + 1) % 240;

//			// These are the defaults for normal drawing(case 3)
//			NPC.hide = false;
//			NPC.behindTiles = false;

//			switch (CurrentLayer) {
//				case 0:
//				case 1:
//				case 4:
//				case 5:
//					NPC.hide = true;
//					break;
//				case 2:
//					NPC.behindTiles = true;
//					break;
//				case 3:
//					break;
//			}
//		}

//		// This method allows you to specify that this npc should be drawn behind certain elements
//		public override void DrawBehind(int index) {
//			// The 6 available positions are as follows:
//			switch (CurrentLayer) {
//				case 0: // Behind tiles and walls
//					Main.instance.DrawCacheNPCsMoonMoon.Add(index);
//					break;
//				case 1: // Behind non solid tiles, but in front of walls
//					Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
//					break;
//				case 2: // Behind tiles, but in front of non solid tiles
//				case 3: // Normal (in front of tiles)
//					break;
//				case 4: // In front of all normal NPC
//					Main.instance.DrawCacheNPCProjectiles.Add(index);
//					break;
//				case 5: // In front of Players
//					Main.instance.DrawCacheNPCsOverPlayers.Add(index);
//					break;
//			}
//		}

//		// PreHoverInteract gives this NPC a custom right click interaction and hover behavior. This example is unrelated to the DrawBehind exhibition.
//		public override bool PreHoverInteract(bool mouseIntersects) {
//			if (CurrentLayer < 3) {
//				return true;
//			}

//			// This code is similar to the code that lets the player right click on the Old Shaking Chest NPC to consume a GoldenKey from the player and transform into Elder Slime.
//			Player player = Main.LocalPlayer;
//			int keyItem = ModContent.ItemType<Items.Placeable.ExampleBlock>();
//			player.cursorItemIconEnabled = true;
//			player.cursorItemIconID = keyItem;
//			player.cursorItemIconText = "";
//			player.noThrow = 2;

//			// This code can be used if you want to enforce the normal NPC interaction range in this method.
//			/*
//			Rectangle interactionRange = new Rectangle((int)(player.Center.X - Player.tileRangeX * 16), (int)(player.Center.Y - Player.tileRangeY * 16), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2);
//			if (!interactionRange.Intersects(NPC.getRect())) {
//				return false;
//			}
//			*/

//			if (!player.dead) {
//				PlayerInput.SetZoom_MouseInWorld();
//				if (Main.mouseRight && Main.npcChatRelease) {
//					Main.npcChatRelease = false;
//					if (PlayerInput.UsingGamepad) {
//						player.releaseInventory = false;
//					}

//					if (player.talkNPC != NPC.whoAmI && !player.tileInteractionHappened ) {
//						if (player.HasItem(keyItem) && player.ConsumeItem(keyItem)) {
//							SoundEngine.PlaySound(SoundID.Item14); // The bomb explosion sound
//							NPC.SimpleStrikeNPC(1000, 0);
//						}
//						else {
//							SoundEngine.PlaySound(SoundID.MenuClose);
//						}
//					}
//				}
//			}

//			return false; // skip vanilla.
//		}

//		// We can use CanChat to force smart interact to target this NPC, even though we don't intend to actually chat with it due to PreHoverInteract bypassing the chat code.
//		public override bool CanChat() {
//			if (CurrentLayer < 3) {
//				return base.CanChat();
//			}

//			return true;
//		}
//	}
//}
