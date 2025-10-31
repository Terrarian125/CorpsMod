using CorpsMod.Content.Dusts;
using CorpsMod.Content.EmoteBubbles;
using CorpsMod.Content.Items;
using CorpsMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CorpsMod.Content.NPCs
{
	/// <summary>
	/// このNPCの主な目的は、バニラの**骨商人（Bone Merchant）**に似たものを作成する方法を示すことです。
	/// つまり、このNPCは他の町のNPCのように振る舞いますが、**幸福度（happiness）ボタン**がなく、**ミニマップ**に表示されず、**敵NPCのようにスポーン**します。代わりに従来の町のNPCが必要な場合は、<see cref="ExamplePerson"/>を参照してください。
	/// </summary>
	public class ExampleBoneMerchant : ModNPC
	{
		private static Profiles.StackedNPCProfile NPCProfile;
		private static Asset<Texture2D> shimmerGun;

		public override void Load() {
			shimmerGun = ModContent.Request<Texture2D>(Texture + "_Shimmer_Gun");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25; // NPCが持つフレーム数

			NPCID.Sets.ExtraFramesCount[Type] = 9; // 通常は町のNPC用ですが、これはNPCが椅子に座ったり、他のNPCと話したりするなどの追加の動作を行う方法です。
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700; // NPCが敵を攻撃しようとする、NPCの中心からのピクセル距離。
			NPCID.Sets.PrettySafe[Type] = 300;
			NPCID.Sets.AttackType[Type] = 1; // 武器を撃ちます。
			NPCID.Sets.AttackTime[Type] = 60; // NPCの攻撃アニメーションが開始されてから終了するまでの時間。
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4; // パーティーがアクティブな場合、パーティーハットがYオフセットでスポーンします。
			NPCID.Sets.ShimmerTownTransform[NPC.type] = true; // このセットは、町のNPCが**シマー化**した姿を持つことを示します。そうでない場合、他の敵のようにシマーに触れると透明になります。

			// このセットのエントリは、このNPCの最も重要な部分です。これがtrueであるため、ゲームに**「実際には」町のNPCでなくても、町のNPCのように振る舞わせたい**ことを伝えます。
			// これは、NPCが町のNPCのAIを持ち、町のNPCのように攻撃し、町のNPCのようにショップを持つ（または必要に応じてその他の追加機能を持つ）ことを意味します。
			// ただし、NPCはマップ上に頭が表示されず、近くにプレイヤーがいない場合やワールドが閉じられた場合に**デスポーン**し、**他のNPCと同じようにスポーン**します。
			NPCID.Sets.ActsLikeTownNPC[Type] = true;

			// これは**幸福度ボタン**を防ぎます。
			NPCID.Sets.NoTownNPCHappiness[Type] = true;

			// 繰り返しますが、このNPCは技術的には町のNPCではないため、スポーン時にカスタム/ランダムな名前を持たせたいことをゲームに伝える必要があります。
			// これを行うには、このフックを単純に**true**に戻すように設定します。これにより、NPCをスポーンするときに、NPCの名前を決定するために**TownNPCName**メソッドが呼び出されます。
			NPCID.Sets.SpawnsWithCustomName[Type] = true;

			// このNPCをカスタム**エモート**に接続します。
			// これにより、NPCがワールドにいるときに、他のNPCが「彼について話す」ようになります。
			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();

			// バニラの骨商人はドアと相互作用できません（特にドアを開閉できません）が、それにもかかわらずNPCにドアと相互作用させたい場合は、
			// 以下のこの行のコメントを解除してください。
			//NPCID.Sets.AllowDoorInteraction[Type] = true;

			// **Bestiary**でのNPCの見た目に影響を与えます。
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f, // NPCがx方向に+1タイル歩いているかのようにBestiaryに描画します
				Direction = 1 // -1は左、1は右です。NPCはデフォルトで左を向いて描画されますが、ExamplePersonは右を向いて描画されます
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, -1),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", -1)
			);
		}

		public override void SetDefaults() {
			NPC.friendly = true; // NPCはプレイヤーを攻撃しません
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;

			// ここにレアリティを設定（例としてTerrariaのピンクスライムと同じレアリティ2を設定）
			NPC.rarity = 2; // レアリティを設定することで、生体探知機で追跡される可能性があります

			AnimationType = NPCID.Guide;
		}

		// 「町のNPCのようだ」という設定だけでは自動的にチャットが許可されないため、NPCがチャットできるように必ず設定してください。
		public override bool CanChat() {
			return true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// Addを複数回呼び出す代わりにAddRangeを使用して、複数のアイテムを一度に追加できます。
			bestiaryEntry.Info.AddRange([
				// Bestiaryにリストされているこの町のNPCの推奨されるバイオームを設定します。
				// 町のNPCでは、通常、NPCの幸福度に関して最も好きなバイオームにこれを設定します。
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,

				// BestiaryでのNPCのフレーバーテキストを設定します。（ローカリゼーションキーを使用します）
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleBoneMerchant_1"),

				// 本当に必要であれば、複数の要素を追加できます
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleBoneMerchant_2")
			]);
		}

		public override void HitEffect(NPC.HitInfo hit) {
			// NPCがダメージを受けたときにダストがスポーンするようにします。
			int num = NPC.life > 0 ? 1 : 5;

			for (int k = 0; k < num; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
			}

			// NPCが倒されたときに**ゴア（Gore）**を作成します。
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// ゴアのタイプを取得します。このNPCにはシマーバリアントのみがあります。（合計6つのゴア）
				string variant = "";
				if (NPC.IsShimmerVariant)
					variant += "_Shimmer";
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

				// ゴアをスポーンさせます。腕と脚の位置は、より自然な見た目にするために下げられています。
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			}
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			return new List<string> {
				"拾い虫",
				"宝石好きなアントリオン",
				"鉱石喰らい"
			};
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// プレイヤーが地下または洞窟にいる場合、Example Bone Merchantがスポーンするわずかなチャンスがあります。
			// ZoneDirtLayerHeight（地下土層）またはZoneRockLayerHeight（岩盤層/洞窟）のどちらかにいるかを確認します。
			bool inUndergroundOrCave = spawnInfo.Player.ZoneDirtLayerHeight || spawnInfo.Player.ZoneRockLayerHeight;

			if (inUndergroundOrCave) {
				return 0.5f;
			}

			// それ以外の場合、上記の条件が満たされない場合、Example Bone Merchantはスポーンしません。
			return 0f;
		}

		//public override float SpawnChance(NPCSpawnInfo spawnInfo) {
		//	// プレイヤーが地下にいて、インベントリにExample Itemを持っている場合、Example Bone Merchantがスポーンするわずかなチャンスがあります。
		//	if (spawnInfo.Player.ZoneDirtLayerHeight && spawnInfo.Player.inventory.Any(item => item.type == ModContent.ItemType<ExampleItem>())) {
		//		return 0.34f;
		//	}

		//	// それ以外の場合、上記の条件が満たされない場合、Example Bone Merchantはスポーンしません。
		//	return 0f;
		//}

		public override string GetChat() {
			WeightedRandom<string> chat = new WeightedRandom<string>();

			// これらは、話しかけたときにNPCがあなたに伝える可能性があることです。
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleBoneMerchant.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleBoneMerchant.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleBoneMerchant.StandardDialogue3"));
			return chat; // chatは暗黙的に文字列にキャストされます。
		}

		public override void SetChatButtons(ref string button, ref string button2) { // チャットUIを開いたときのチャットボタンの内容
			button = Language.GetTextValue("LegacyInterface.28"); // これは「ショップ」という単語のキーです
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				shop = "Shop";
			}
		}

		public override void AddShops() {
			// カスタムのExampleTravelingMerchantShopを使用します
			Shop = new ExampleTravelingMerchantShop(NPC.type);

			// 常に売るアイテム
			Shop.Add(ItemID.Obsidian);

			//宝石全種からランダムに3つ持ってくる
			Shop.AddPool("Gems", slots: 3)
				.Add(ItemID.Amethyst)
				.Add(ItemID.Topaz)
				.Add(ItemID.Sapphire)
				.Add(ItemID.Emerald)
				.Add(ItemID.Ruby)
				.Add(ItemID.Diamond)
				.Add(ItemID.Amber); // アンバーも宝石として含めます

			//ノーマルモードの鉱石からランダムに3つ持ってくる
			Shop.AddPool("NormalModeOres", slots: 3)
				.Add(ItemID.CopperOre)
				.Add(ItemID.TinOre)
				.Add(ItemID.IronOre)
				.Add(ItemID.LeadOre)
				.Add(ItemID.SilverOre)
				.Add(ItemID.TungstenOre)
				.Add(ItemID.GoldOre)
				.Add(ItemID.PlatinumOre);

			//ハードモード限定でハードモードの鉱石からランダムに3つ持ってくる
			Shop.AddPool("HardmodeOres", slots: 3, Condition.Hardmode)
				.Add(ItemID.CobaltOre)
				.Add(ItemID.PalladiumOre)
				.Add(ItemID.MythrilOre)
				.Add(ItemID.OrichalcumOre)
				.Add(ItemID.AdamantiteOre)
				.Add(ItemID.TitaniumOre);

			// 元のサンプルにあったカテゴリを削除したため、ここにはShop.Register()のみ残します。
			Shop.Register();
		}

		// NPCが倒されたときにドロップするアイテムを変更または追加するために使用されます。
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			// 金床 (Anvil) を100%の確率でドロップするように追加します。
			// ItemDropRule.Common(アイテムID, 確率の分母, 最小スタック数, 最大スタック数)
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ItemID.IronAnvil, 1, 1, 1));
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 2f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 10;
			randExtraCooldown = 1;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ProjectileID.NanoBullet;
			attackDelay = 1;

			// このコードは、後続のショットを段階的に遅延させます。
			if (NPC.localAI[3] > attackDelay) {
				attackDelay = 12;
			}
			if (NPC.localAI[3] > attackDelay) {
				attackDelay = 24;
			}
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 10f;
			randomOffset = 0.2f;
		}

		public override void TownNPCAttackShoot(ref bool inBetweenShots) {
			if (NPC.localAI[3] > 1) {
				inBetweenShots = true;
			}
		}

		public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) {
			if (!NPC.IsShimmerVariant) {
				//// 既存のアイテムを使用する場合は、このアプローチを使用します
				//int itemType = ModContent.ItemType<ExampleCustomAmmoGun>();
				//Main.GetItemDrawFrame(itemType, out item, out itemFrame);
				//horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, itemType).X - 12;
			}
			else {
				// このテクスチャは実際には既存のアイテムではありませんが、使用できます。
				item = shimmerGun.Value;
				itemFrame = item.Frame();
				horizontalHoldoutOffset = -2;
			}
		}
	}
}
