using CorpsMod.Content.Items.Placeable.Banners;
using CorpsMod.NPCs;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.NPCs
{
	// これら3つのクラスは、Worm.csのWormHead、WormBody、WormTailクラスの使用例を示しています。
	internal class ExampleWormHead : WormHead
	{
		public override int BodyType => ModContent.NPCType<ExampleWormBody>();

		public override int TailType => ModContent.NPCType<ExampleWormTail>();

		public override void SetStaticDefaults() {
			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() { // 生物図鑑でのNPCの見え方に影響します
				CustomTexturePath = "CorpsMod/Content/NPCs/ExampleWorm_Bestiary", // ワームのように複数のパーツで構成されるNPCの場合、図鑑用にカスタムテクスチャを用意することをお勧めします。
				Position = new Vector2(40f, 24f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 12f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
		}

		public override void SetDefaults() {
			// 頭部の防御力は10、胴体は20、尻尾は30。
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.aiStyle = -1;

			Banner = Type;
			// これらの行は、メインとなる頭部パーツにのみ記述する必要があります。
			BannerItem = ModContent.ItemType<ExampleWormHeadBanner>();
			ItemID.Sets.KillsToBanner[BannerItem] = 25; // バナーのドロップおよび生物図鑑の解放に必要なカスタム撃破数。デフォルトの50体にする場合はこの行を省略してください。
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// AddRangeを使用することで、一度に複数の項目をまとめて追加できます
			bestiaryEntry.Info.AddRange([
				// 生物図鑑に表示される、このNPCの出現条件（バイオーム）を設定します。
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,

				// 生物図鑑に表示される、このNPCの説明文（フレーバーテキスト）を設定します。
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleWormHead")
			]);
		}

		public override void Init() {
			// 関節数（セグメント数）の範囲を設定します
			// もし長さを常に一定にしたい場合は、これら2つのプロパティに同じ数値を設定してください
			MinSegmentLength = 6;
			MaxSegmentLength = 12;

			CommonWormInit(this);
		}

		// このメソッドは、ExampleWormHead、ExampleWormBody、ExampleWormTailのすべてから呼び出されます
		internal static void CommonWormInit(Worm worm) {
			// これら2つのプロパティでワームの移動を制御します
			worm.MoveSpeed = 5.5f;
			worm.Acceleration = 0.045f;
		}

		private int attackCounter;
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(attackCounter);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			attackCounter = reader.ReadInt32();
		}

		public override void AI() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (attackCounter > 0) {
					attackCounter--; // 攻撃カウンターを減算（タイマー処理）
				}

				Player target = Main.player[NPC.target];
				// もし攻撃カウンターが0以下、かつターゲットとの距離が12.5ブロック（200ピクセル）未満、かつターゲットとの間にブロックの遮蔽物がない場合、弾を発射します。
				if (attackCounter <= 0 && Vector2.Distance(NPC.Center, target.Center) < 200 && Collision.CanHit(NPC.Center, 1, 1, target.Center, 1, 1)) {
					Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
					direction = direction.RotatedByRandom(MathHelper.ToRadians(10));

					int projectile = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, direction * 1, ProjectileID.ShadowBeamHostile, 5, 0, Main.myPlayer);
					Main.projectile[projectile].timeLeft = 300;
					attackCounter = 500;
					NPC.netUpdate = true;
				}
			}
		}
	}

	internal class ExampleWormBody : WormBody
	{
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Hide = true // このNPCを生物図鑑から非表示にします。登録枠を1つにまとめたいマルチパーツNPCに有効です。
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
			NPCID.Sets.RespawnEnemyID[NPC.type] = ModContent.NPCType<ExampleWormHead>();
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
			NPC.aiStyle = -1;

			// 追加の胴体パーツは、メインとなるModNPC（頭部）と同じバナー（旗）の値を共有させる必要があります。
			Banner = ModContent.NPCType<ExampleWormHead>();
		}

		public override void Init() {
			ExampleWormHead.CommonWormInit(this);
		}
	}

	internal class ExampleWormTail : WormTail
	{
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Hide = true // このNPCを生物図鑑から非表示にします。登録枠を1つにまとめたいマルチパーツNPCに有効です。
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
			NPCID.Sets.RespawnEnemyID[NPC.type] = ModContent.NPCType<ExampleWormHead>();
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
			NPC.aiStyle = -1;

			// 追加の胴体パーツは、メインとなるModNPC（頭部）と同じバナー（旗）の値を共有させる必要があります。
			Banner = ModContent.NPCType<ExampleWormHead>();
		}

		public override void Init() {
			ExampleWormHead.CommonWormInit(this);
		}
	}
}