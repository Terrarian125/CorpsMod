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
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ItemID.Anvil, 1, 1, 1));
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
				// 既存のアイテムを使用する場合は、このアプローチを使用します
				int itemType = ModContent.ItemType<ExampleCustomAmmoGun>();
				Main.GetItemDrawFrame(itemType, out item, out itemFrame);
				horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, itemType).X - 12;
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

//using CorpsMod.Content.Dusts;
//using CorpsMod.Content.EmoteBubbles;
//using CorpsMod.Content.Items;
//using CorpsMod.Content.Items.Weapons;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using ReLogic.Content;
//using System.Collections.Generic;
//using System.Linq;
//using Terraria;
//using Terraria.GameContent;
//using Terraria.GameContent.Bestiary;
//using Terraria.ID;
//using Terraria.Localization;
//using Terraria.ModLoader;
//using Terraria.Utilities;

//namespace CorpsMod.Content.NPCs
//{
//	/// <summary>
//	/// The main focus of this NPC is to show how to make something similar to the vanilla bone merchant;
//	/// which means that the NPC will act like any other town NPC but won't have a happiness button, won't appear on the minimap,
//	/// and will spawn like an enemy NPC. If you want a traditional town NPC instead, see <see cref="ExamplePerson"/>.
//	/// </summary>
//	public class ExampleBoneMerchant : ModNPC
//	{
//		private static Profiles.StackedNPCProfile NPCProfile;
//		private static Asset<Texture2D> shimmerGun;

//		public override void Load() {
//			shimmerGun = ModContent.Request<Texture2D>(Texture + "_Shimmer_Gun");
//		}

//		public override void SetStaticDefaults() {
//			Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has

//			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
//			NPCID.Sets.AttackFrameCount[Type] = 4;
//			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
//			NPCID.Sets.PrettySafe[Type] = 300;
//			NPCID.Sets.AttackType[Type] = 1; // Shoots a weapon.
//			NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
//			NPCID.Sets.AttackAverageChance[Type] = 30;
//			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
//			NPCID.Sets.ShimmerTownTransform[NPC.type] = true; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

//			// This sets entry is the most important part of this NPC. Since it is true, it tells the game that we want this NPC to act like a town NPC without ACTUALLY being one.
//			// What that means is: the NPC will have the AI of a town NPC, will attack like a town NPC, and have a shop (or any other additional functionality if you wish) like a town NPC.
//			// However, the NPC will not have their head displayed on the map, will de-spawn when no players are nearby or the world is closed, and will spawn like any other NPC.
//			NPCID.Sets.ActsLikeTownNPC[Type] = true;

//			// This prevents the happiness button
//			NPCID.Sets.NoTownNPCHappiness[Type] = true;

//			// To reiterate, since this NPC isn't technically a town NPC, we need to tell the game that we still want this NPC to have a custom/randomized name when they spawn.
//			// In order to do this, we simply make this hook return true, which will make the game call the TownNPCName method when spawning the NPC to determine the NPC's name.
//			NPCID.Sets.SpawnsWithCustomName[Type] = true;

//			// Connects this NPC with a custom emote.
//			// This makes it when the NPC is in the world, other NPCs will "talk about him".
//			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();

//			// The vanilla Bone Merchant cannot interact with doors (open or close them, specifically), but if you want your NPC to be able to interact with them despite this,
//			// uncomment this line below.
//			//NPCID.Sets.AllowDoorInteraction[Type] = true;

//			// Influences how the NPC looks in the Bestiary
//			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
//				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
//				Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
//			};

//			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

//			NPCProfile = new Profiles.StackedNPCProfile(
//				new Profiles.DefaultNPCProfile(Texture, -1),
//				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", -1)
//			);
//		}

//		public override void SetDefaults() {
//			NPC.friendly = true; // NPC Will not attack player
//			NPC.width = 18;
//			NPC.height = 40;
//			NPC.aiStyle = 7;
//			NPC.damage = 10;
//			NPC.defense = 15;
//			NPC.lifeMax = 250;
//			NPC.HitSound = SoundID.NPCHit1;
//			NPC.DeathSound = SoundID.NPCDeath1;
//			NPC.knockBackResist = 0.5f;

//			AnimationType = NPCID.Guide;
//		}

//		// Make sure to allow your NPC to chat, since being "like a town NPC" doesn't automatically allow for chatting.
//		public override bool CanChat() {
//			return true;
//		}

//		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
//			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
//			bestiaryEntry.Info.AddRange([
//				// Sets the preferred biomes of this town NPC listed in the bestiary.
//				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
//				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,

//				// Sets your NPC's flavor text in the bestiary. (use localization keys)
//				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleBoneMerchant_1"),

//				// You can add multiple elements if you really wanted to
//				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleBoneMerchant_2")
//			]);
//		}

//		public override void HitEffect(NPC.HitInfo hit) {
//			// Causes dust to spawn when the NPC takes damage.
//			int num = NPC.life > 0 ? 1 : 5;

//			for (int k = 0; k < num; k++) {
//				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
//			}

//			// Create gore when the NPC is killed.
//			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
//				// Retrieve the gore types. This NPC only has shimmer variants. (6 total gores)
//				string variant = "";
//				if (NPC.IsShimmerVariant)
//					variant += "_Shimmer";
//				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
//				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
//				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

//				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
//			}
//		}

//		public override ITownNPCProfile TownNPCProfile() {
//			return NPCProfile;
//		}

//		public override List<string> SetNPCNameList() {
//			return new List<string> {
//				"Blocky Bones",
//				"Someone's Ribcage",
//				"Underground Blockster",
//				"ああああ"
//			};
//		}

//		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
//			// If any player is underground and has an example item in their inventory, the example bone merchant will have a slight chance to spawn.
//			if (spawnInfo.Player.ZoneDirtLayerHeight && spawnInfo.Player.inventory.Any(item => item.type == ModContent.ItemType<ExampleItem>())) {
//				return 0.34f;
//			}

//			// Else, the example bone merchant will not spawn if the above conditions are not met.
//			return 0f;
//		}

//		public override string GetChat() {
//			WeightedRandom<string> chat = new WeightedRandom<string>();

//			// These are things that the NPC has a chance of telling you when you talk to it.
//			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleBoneMerchant.StandardDialogue1"));
//			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleBoneMerchant.StandardDialogue2"));
//			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleBoneMerchant.StandardDialogue3"));
//			return chat; // chat is implicitly cast to a string.
//		}

//		public override void SetChatButtons(ref string button, ref string button2) { // What the chat buttons are when you open up the chat UI
//			button = Language.GetTextValue("LegacyInterface.28"); // This is the key to the word "Shop"
//		}

//		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
//			if (firstButton) {
//				shop = "Shop";
//			}
//		}

//		public override void AddShops() {
//			new NPCShop(Type)
//				.Add<ExampleItem>()
//				.Register();
//		}

//		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
//			damage = 20;
//			knockback = 2f;
//		}

//		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
//			cooldown = 10;
//			randExtraCooldown = 1;
//		}

//		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
//			projType = ProjectileID.NanoBullet;
//			attackDelay = 1;

//			// This code progressively delays subsequent shots.
//			if (NPC.localAI[3] > attackDelay) {
//				attackDelay = 12;
//			}
//			if (NPC.localAI[3] > attackDelay) {
//				attackDelay = 24;
//			}
//		}

//		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
//			multiplier = 10f;
//			randomOffset = 0.2f;
//		}

//		public override void TownNPCAttackShoot(ref bool inBetweenShots) {
//			if (NPC.localAI[3] > 1) {
//				inBetweenShots = true;
//			}
//		}

//		public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) {
//			if (!NPC.IsShimmerVariant) {
//				// If using an existing item, use this approach
//				int itemType = ModContent.ItemType<ExampleCustomAmmoGun>();
//				Main.GetItemDrawFrame(itemType, out item, out itemFrame);
//				horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, itemType).X - 12;
//			}
//			else {
//				// This texture isn't actually an existing item, but can still be used.
//				item = shimmerGun.Value;
//				itemFrame = item.Frame();
//				horizontalHoldoutOffset = -2;
//			}
//		}
//	}
//}
