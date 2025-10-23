using CorpsMod.Common;
using CorpsMod.Common.Configs;
using CorpsMod.Common.Systems;
using CorpsMod.Content.Biomes;
using CorpsMod.Content.Dusts;
using CorpsMod.Content.EmoteBubbles;
using CorpsMod.Content.Items;
using CorpsMod.Content.Items.Accessories;
using CorpsMod.Content.Items.Armor;
using CorpsMod.Content.Projectiles;
using CorpsMod.Content.Tiles;
using CorpsMod.Content.Tiles.Furniture;
using CorpsMod.Content.Walls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace CorpsMod.Content.NPCs
{
	// [AutoloadHead] and NPC.townNPC are extremely important and absolutely both necessary for any Town NPC to work at all.
	// [AutoloadHead] と NPC.townNPC は非常に重要であり、Town NPC が機能するには両方とも絶対に必要です。
	[AutoloadHead]
	public class ExamplePerson : ModNPC
	{
		public const string ShopName = "Shop";
		public int NumberOfTimesTalkedTo = 0;

		private static int ShimmerHeadIndex;
		private static Profiles.StackedNPCProfile NPCProfile;

		public static LocalizedText UpgradedText { get; private set; }

		// Sets a unique message when the NPC dies.
		// See also NPCID.Sets.IsTownChild if you just want the message used by Angler and Princess.
		// See ModifyDeathMessage() way below for more details
		// NPCが死亡したときに固有のメッセージを設定します。
		// AnglerとPrincessが使用するメッセージだけが必要な場合は、NPCID.Sets.IsTownChildも参照してください。
		// 詳細については、以下のModifyDeathMessage()メソッドを参照してください。
		public override LocalizedText DeathMessage => this.GetLocalization("DeathMessage");

		public override void Load() {
			// Adds our Shimmer Head to the NPCHeadLoader.
			// Shimmer Head を NPCHeadLoader に追加します。
			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25; // NPCが持つフレームの総数

			NPCID.Sets.ExtraFramesCount[Type] = 9; // 一般的には町のNPC向けですが、椅子に座ったり他のNPCと会話したりするなど、NPCが追加の行動をとる際にも使用されます。これは歩行フレームの後の残りのフレームです。
			NPCID.Sets.AttackFrameCount[Type] = 4; // 攻撃アニメーションのフレーム数。
			NPCID.Sets.DangerDetectRange[Type] = 700;// NPC が敵を攻撃しようとするときの中心から離れたピクセル数。
			NPCID.Sets.AttackType[Type] = 0; //町のNPCが行う攻撃の種類 performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
			NPCID.Sets.AttackTime[Type] = 90; //NPC の攻撃アニメーションが開始してから終了するまでにかかる時間。
			NPCID.Sets.AttackAverageChance[Type] = 30; //町のNPCが攻撃する確率の分母。数値が低いほど、町のNPCはより攻撃的に見えるようになります。
			NPCID.Sets.HatOffsetY[Type] = 4; //パーティーがアクティブな場合、パーティーハットは Y オフセットで生成されます。
			NPCID.Sets.ShimmerTownTransform[NPC.type] = true; // このセットでは、町のNPCがシマー状態になっていると記載されています。そうでない場合、町のNPCは他の敵と同様にシマーに触れると透明になります。

			NPCID.Sets.ShimmerTownTransform[Type] = true; //この NPC がシマー液体に触れた後に異なるテクスチャを持つようになります。

			// Connects this NPC with a custom emote.
			// This makes it when the NPC is in the world, other NPCs will "talk about him".
			// By setting this you don't have to override the PickEmote method for the emote to appear.
			// この NPC をカスタムエモートに接続します。
			// これにより、NPC がワールドに存在するときに、他の NPC が「彼について話す」ようになります。
			// これを設定すると、エモートを表示するために PickEmote メソッドをオーバーライドする必要がなくなります。
			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExamplePersonEmote>();

			// Influences how the NPC looks in the Bestiary
			// ベストイアリにおけるNPCの見た目に影響します
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				Direction = 1 // -1 は左、1 は右です。NPC はデフォルトで左向きに描画されますが、ExamplePerson は右向きに描画されます。
							  // Rotation = MathHelper.ToRadians(180) // You can also change the rotation of an NPC. Rotation is measured in radians
							  // If you want to see an example of manually modifying these when the NPC is drawn, see PreDraw
							  // Rotation = MathHelper.ToRadians(180) // NPCの回転角度も変更できます。回転角度はラジアン単位で測定されます。
							  // NPCの描画時に手動で回転角度を変更する例については、PreDrawを参照してください。
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			// Set Example Person's biome and neighbor preferences with the NPCHappiness hook. You can add happiness text and remarks with localization (See an example in CorpsMod/Localization/en-US.lang).
			// NOTE: The following code uses chaining - a style that works due to the fact that the SetXAffection methods return the same NPCHappiness instance they're called on.
			// NPCHappinessフックを使って、サンプル人物のバイオームと隣人設定を設定します。ローカライズによって幸福度テキストとコメントを追加できます（CorpsMod/Localization/en-US.langの例を参照）。
			// 注: 以下のコードではチェーンを使用しています。これは、SetXAffectionメソッドが、呼び出されたのと同じNPCHappinessインスタンスを返すため、うまく機能するスタイルです。
			NPC.Happiness
				.SetBiomeAffection<ForestBiome>(AffectionLevel.Like) // Example Person prefers the forest.// 例人は森を好みます。
				.SetBiomeAffection<SnowBiome>(AffectionLevel.Dislike) // Example Person dislikes the snow.// 例 人は雪が嫌いです。
				.SetBiomeAffection<ExampleSurfaceBiome>(AffectionLevel.Love) // Example Person likes the Example Surface Biome// 例の人物は例の表面バイオームを気に入っています
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Love) // Loves living near the dryad.ドライアドの近くに住むのが大好きです。
				.SetNPCAffection(NPCID.Guide, AffectionLevel.Like) // Likes living near the guide.ガイドの近くに住むのが好きです。
				.SetNPCAffection(NPCID.Merchant, AffectionLevel.Dislike) // Dislikes living near the merchant.// 商人の近くに住むのが嫌いです。
				.SetNPCAffection(NPCID.Demolitionist, AffectionLevel.Hate) // Hates living near the demolitionist.解体業者の近くに住むのが大嫌い。
			; // < Mind the semicolon!// < セミコロンに注意してください!

			// This creates a "profile" for ExamplePerson, which allows for different textures during a party and/or while the NPC is shimmered.
			// これにより、ExamplePerson の「プロファイル」が作成され、パーティ中や NPC がきらめいている間にさまざまなテクスチャを使用できるようになります。
			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex, Texture + "_Shimmer_Party")
			);

			ContentSamples.NpcBestiaryRarityStars[Type] = 3;// これを設定することで、デフォルトのベストイアリの星の数の計算を上書きできます。

			UpgradedText = this.GetLocalization("Upgraded");
		}

		public override void SetDefaults() {
			NPC.townNPC = true; // Sets NPC to be a Town NPC  NPCを町のNPCに設定する
			NPC.friendly = true; // NPC Will not attack player // NPCはプレイヤーを攻撃しません
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

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			// 複数のアイテムを一度に追加するには、Addを複数回呼び出す代わりにAddRangeを使用します。
			bestiaryEntry.Info.AddRange([
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				// 町の NPC の、ベストイアリに記載されている好みのバイオームを設定します。
				// 町の NPC の場合、通常は NPC の幸福度に関して最も好まれるバイオームを設定します。
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

				// Sets your NPC's flavor text in the bestiary. (use localization keys)
				// ベストイアリのNPCのフレーバーテキストを設定します。(ローカライズキーを使用)
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExamplePerson_1"),

				// You can add multiple elements if you really wanted to
				// 複数の要素を追加することもできます
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExamplePerson_2")
			]);
		}

		// The PreDraw hook is useful for drawing things before our sprite is drawn or running code before the sprite is drawn
		// Returning false will allow you to manually draw your NPC
		// PreDrawフックは、スプライトが描画される前に何かを描画したり、スプライトが描画される前にコードを実行したりするのに便利です。
		// falseを返すと、NPCを手動で描画できます。
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// This code slowly rotates the NPC in the bestiary
			// (simply checking NPC.IsABestiaryIconDummy and incrementing NPC.Rotation won't work here as it gets overridden by drawModifiers.Rotation each tick)
			// このコードは、ベストイアリ内のNPCをゆっくりと回転させます。
			// (NPC.IsABestiaryIconDummyをチェックしてNPC.Rotationをインクリメントするだけでは、ここでは機能しません。これは、ティックごとにdrawModifiers.Rotationによって上書きされるためです。)
			if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(Type, out NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers)) {
				drawModifiers.Rotation += 0.001f;

				// Replace the existing NPCBestiaryDrawModifiers with our new one with an adjusted rotation
				// 既存の NPC BestiaryDrawModifiers を、回転を調整した新しいものに置き換えます。
				NPCID.Sets.NPCBestiaryDrawOffset.Remove(Type);
				NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
			}

			return true;
		}

		public override void HitEffect(NPC.HitInfo hit) {
			int num = NPC.life > 0 ? 1 : 5;

			for (int k = 0; k < num; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
			}

			// Create gore when the NPC is killed.
			// NPC が殺されたときにゴアを作成します。
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// Retrieve the gore types. This NPC has shimmer and party variants for head, arm, and leg gore. (12 total gores)
				// ゴアの種類を取得します。このNPCには、頭、腕、脚のゴアにシマーとパーティのバリエーションがあります。(合計12種類のゴア)
				string variant = "";
				if (NPC.IsShimmerVariant)
					variant += "_Shimmer";
				if (NPC.altTexture == 1)
					variant += "_Party";
				int hatGore = NPC.GetPartyHatGore();
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
				// ゴアを生成します。より自然な見た目にするために、腕と脚の位置を下げます。
				if (hatGore > 0) {
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
				}
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			}
		}

		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_SpawnNPC) {
				// A TownNPC is "unlocked" once it successfully spawns into the world.
				// TownNPC は、ワールドに正常に出現すると「ロック解除」されます。
				TownNPCRespawnSystem.unlockedExamplePersonSpawn = true;
			}
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) { //町の NPC が出現するための要件。
			if (TownNPCRespawnSystem.unlockedExamplePersonSpawn) {
				// If Example Person has spawned in this world before, we don't require the user satisfying the ExampleItem/ExampleBlock inventory conditions for a respawn.
				// Example Person が以前にこのワールドに出現したことがある場合、再出現のためにユーザーが ExampleItem/ExampleBlock インベントリ条件を満たす必要はありません。
				return true;
			}

			foreach (var player in Main.ActivePlayers) {
				// Player has to have either an ExampleItem or an ExampleBlock in order for the NPC to spawn
				// NPC が出現するには、プレイヤーは ExampleItem または ExampleBlock のいずれかを持っている必要があります
				if (player.inventory.Any(item => item.type == ModContent.ItemType<ExampleItem>() || item.type == ModContent.ItemType<Items.Placeable.ExampleBlock>())) {
					return true;
				}
			}

			return false;
		}

		// Example Person needs a house built out of CorpsMod tiles. You can delete this whole method in your townNPC for the regular house conditions.
		// 例：CorpsModタイルで家を建てたい人。townNPCのこのメソッド全体を削除すれば、通常の家の状態になります。
		//public override bool CheckConditions(int left, int right, int top, int bottom) {
		//	int score = 0;
		//	for (int x = left; x <= right; x++) {
		//		for (int y = top; y <= bottom; y++) {
		//			int type = Main.tile[x, y].TileType;
		//			if (type == ModContent.TileType<ExampleBlock>() || type == ModContent.TileType<ExampleChair>() || type == ModContent.TileType<ExampleWorkbench>() || type == ModContent.TileType<ExampleBed>() || type == ModContent.TileType<ExampleDoorOpen>() || type == ModContent.TileType<ExampleDoorClosed>()) {
		//				score++;
		//			}

		//			if (Main.tile[x, y].WallType == ModContent.WallType<ExampleWall>()) {
		//				score++;
		//			}
		//		}
		//	}

		//	return score >= ((right - left) * (bottom - top)) / 2;
		//}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			return new List<string>() {
				"Someone",
				"Somebody",
				"Blocky",
				"Colorless"
			};
		}

		public override void FindFrame(int frameHeight) {
			/*npc.frame.Width = 40;
			if (((int)Main.time / 10) % 2 == 0)
			{
				npc.frame.X = 40;
			}
			else
			{
				npc.frame.X = 0;
			}*/
		}

		public override string GetChat() {
			WeightedRandom<string> chat = new WeightedRandom<string>();

			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0 && Main.rand.NextBool(4)) {
				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
			}
			// These are things that the NPC has a chance of telling you when you talk to it.
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.StandardDialogue3"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.StandardDialogue4"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.CommonDialogue"), 5.0);
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.RareDialogue"), 0.1);

			NumberOfTimesTalkedTo++;
			if (NumberOfTimesTalkedTo >= 10) {
				// This counter is linked to a single instance of the NPC, so if ExamplePerson is killed, the counter will reset.
				// このカウンターは NPC の単一インスタンスにリンクされているため、ExamplePerson が殺されるとカウンターはリセットされます。
				//プレイヤーがそのNPCと10回以上会話すると、NPCが特別な反応をするように設定している部分
				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.TalkALot"));
			}

			string chosenChat = chat; // チャットは暗黙的に文字列に変換されます。ここでランダムな選択が行われます。

			// Here is some additional logic based on the chosen chat line. In this case, we want to display an item in the corner for StandardDialogue4.
			// 選択されたチャットラインに基づいた追加ロジックをここに示します。この場合、StandardDialogue4 のコーナーにアイテムを表示します。
			if (chosenChat == Language.GetTextValue("Mods.CorpsMod.Dialogue.ExamplePerson.StandardDialogue4")) {
				// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
				// Main.npcChatCornerItem は、Angler Quest チャットのように、コーナーに単一のアイテムを表示します。
				Main.npcChatCornerItem = ItemID.HiveBackpack;
			}

			return chosenChat;
		}

		public override void SetChatButtons(ref string button, ref string button2) { // チャットUIを開いたときに表示されるチャットボタン
			button = Language.GetTextValue("LegacyInterface.28"); // これは「Shop」という単語のキーです
			button2 = "Awesomeify";
			if (Main.LocalPlayer.HasItem(ItemID.HiveBackpack)) {
				button = "Upgrade " + Lang.GetItemNameValue(ItemID.HiveBackpack);
			}
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				// We want 3 different functionalities for chat buttons, so we use HasItem to change button 1 between a shop and upgrade action.
				// チャット ボタンには 3 つの異なる機能が必要なため、HasItem を使用してボタン 1 をショップ アクションとアップグレード アクションの間で変更します。

				if (Main.LocalPlayer.HasItem(ItemID.HiveBackpack)) {
					SoundEngine.PlaySound(SoundID.Item37); // Reforge/Anvil sound// 再鍛造/金床の音

					Main.npcChatText = UpgradedText.Value;

					int hiveBackpackItemIndex = Main.LocalPlayer.FindItem(ItemID.HiveBackpack);
					var entitySource = NPC.GetSource_GiftOrReward();

					Main.LocalPlayer.inventory[hiveBackpackItemIndex].TurnToAir();
					Main.LocalPlayer.QuickSpawnItem(entitySource, ModContent.ItemType<WaspNest>());

					return;
				}

				shop = ShopName; // Name of the shop tab we want to open.// 開きたいショップタブの名前。
			}
		}

		// Not completely finished, but below is what the NPC will sell
		// 完全には完成していませんが、以下はNPCが販売するものになります
		public override void AddShops() {
			var npcShop = new NPCShop(Type, ShopName)
				.Add<ExampleItem>()
				//.Add<EquipMaterial>()
				//.Add<BossItem>()
				.Add(new Item(ModContent.ItemType<Items.Placeable.Furniture.ExampleWorkbench>()) { shopCustomPrice = Item.buyPrice(copper: 15) }) // この例ではカスタム価格を設定します。ExampleNPCShop.cs にはカスタム価格と通貨に関する詳細情報が記載されています。
				.Add<Items.Placeable.Furniture.ExampleChair>()
				.Add<Items.Placeable.Furniture.ExampleDoor>()
				.Add<Items.Placeable.Furniture.ExampleBed>()
				.Add<Items.Placeable.Furniture.ExampleChest>()
				.Add<Items.Tools.ExamplePickaxe>()
				.Add<Items.Tools.ExampleHamaxe>()
				.Add<Items.Consumables.ExampleHealingPotion>(new Condition("Mods.CorpsMod.Conditions.PlayerHasLifeforceBuff", () => Main.LocalPlayer.HasBuff(BuffID.Lifeforce)))
				.Add<Items.Weapons.ExampleSword>(Condition.MoonPhasesQuarter0)
				//.Add<ExampleGun>(Condition.MoonPhasesQuarter1)
				.Add<Items.Ammo.ExampleBullet>(Condition.MoonPhasesQuarter1)
				.Add<Items.Weapons.ExampleStaff>(ExampleConditions.DownedMinionBoss)
				.Add<ExampleOnBuyItem>()
				.Add(ItemID.AcornAxe) //既存のバニラアイテムを販売する方法の例を次に示します。
				.Add<Items.Weapons.ExampleYoyo>(Condition.IsNpcShimmered); // Let's sell an yoyo if this NPC is shimmered!// このNPCがキラキラしていたらヨーヨーを売りましょう！

			if (ModContent.GetInstance<CorpsModConfig>().ExampleWingsToggle) {
				npcShop.Add<ExampleWings>(ExampleConditions.InExampleBiome);
			}

			if (ModContent.TryFind("SummonersAssociation/BloodTalisman", out ModItem bloodTalisman)) {
				npcShop.Add(bloodTalisman.Type);
			}
			npcShop.Register(); // このショップタブの名前
		}

		public override void ModifyActiveShop(string shopName, Item[] items) {
			foreach (Item item in items) {
				// Skip 'air' items and null items.
				// 'air' 項目と null 項目をスキップします。
				if (item == null || item.type == ItemID.None) {
					continue;
				}

				// If NPC is shimmered then reduce all prices by 50%.
				// NPC がシマー状態の場合、すべての価格を 50% 引き下げます。
				if (NPC.IsShimmerVariant) {
					int value = item.shopCustomPrice ?? item.value;
					item.shopCustomPrice = value / 2;
				}
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>()));
		}

		// Make this Town NPC teleport to the King and/or Queen statue when triggered. Return toKingStatue for only King Statues. Return !toKingStatue for only Queen Statues. Return true for both.
		// この町のNPCがトリガーされた時に王または女王の像へテレポートするようにします。王の像の場合のみtoKingStatueを返します。女王の像の場合のみ!toKingStatueを返します。どちらもtrueを返します。
		public override bool CanGoToStatue(bool toKingStatue) => true;

		// Make something happen when the npc teleports to a statue. Since this method only runs server side, any visual effects like dusts or gores have to be synced across all clients manually.
		// NPCが像にテレポートした際に何かが起こるようにします。このメソッドはサーバー側でのみ実行されるため、ダストやゴアなどの視覚効果はすべてのクライアント間で手動で同期する必要があります。
		public override void OnGoToStatue(bool toKingStatue) {
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)CorpsMod.MessageType.ExampleTeleportToStatue);
				packet.Write((byte)NPC.whoAmI);
				packet.Send();
			}
			else {
				StatueTeleport();
			}
		}

		// Create a square of pixels around the NPC on teleport.
		// テレポート時に NPC の周囲にピクセルの正方形を作成します。
		public void StatueTeleport() {
			for (int i = 0; i < 30; i++) {
				Vector2 position = Main.rand.NextVector2Square(-20, 21);
				if (Math.Abs(position.X) > Math.Abs(position.Y)) {
					position.X = Math.Sign(position.X) * 20;
				}
				else {
					position.Y = Math.Sign(position.Y) * 20;
				}

				Dust.NewDustPerfect(NPC.Center + position, ModContent.DustType<Sparkle>(), Vector2.Zero).noGravity = true;
			}
		}

		public override bool ModifyDeathMessage(ref NetworkText customText, ref Color color) {
			// This example shows how you would further customize the message, in this case just for the shimmer variant.
			// この例では、シマー バリアントのみを対象に、メッセージをさらにカスタマイズする方法を示します。
			if (NPC.IsShimmerVariant) {
				customText = NetworkText.FromKey(this.GetLocalizationKey("DeathMessageAlt"), NPC.GetFullNetName());
				color = Color.Yellow;
			}
			return true;
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<SparklingBall>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 2f;
			// SparklingBall is not affected by gravity, so gravityCorrection is left alone.
			// SparklingBall は重力の影響を受けないので、gravityCorrection はそのまま残します。
		}

		public override void LoadData(TagCompound tag) {
			NumberOfTimesTalkedTo = tag.GetInt("numberOfTimesTalkedTo");
		}

		public override void SaveData(TagCompound tag) {
			tag["numberOfTimesTalkedTo"] = NumberOfTimesTalkedTo;
		}

		// Let the NPC "talk about" minion boss
		// NPCにミニオンのボスについて「話させる」
		public override int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) {
			// By default this NPC will have a chance to use the Minion Boss Emote even if Minion Boss is not downed yet
			int type = ModContent.EmoteBubbleType<MinionBossEmote>();
			// If the NPC is talking to the Demolitionist, it will be more likely to react with angry emote
			// NPCがデモリショニスト(爆破技師？)と話しているときは、怒りのエモートで反応する可能性が高くなります
			if (otherAnchor.entity is NPC { type: NPCID.Demolitionist }) {
				type = EmoteID.EmotionAnger;
			}

			// Make the selection more likely by adding it to the list multiple times
			// リストに複数回追加することで、選択の可能性を高めます
			for (int i = 0; i < 4; i++) {
				emoteList.Add(type);
			}

			// Use this or return null if you don't want to override the emote selection totally
			// エモート選択を完全に上書きしたくない場合は、これを使用するか、null を返します
			return base.PickEmote(closestPlayer, emoteList, otherAnchor);
		}
	}
}