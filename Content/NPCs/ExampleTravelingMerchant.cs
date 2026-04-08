//行商NPCのサンプル
using CorpsMod.Content.Dusts;
using CorpsMod.Content.EmoteBubbles;
using CorpsMod.Content.Items;
using CorpsMod.Content.Items.Armor;
using CorpsMod.Content.Items.Placeable;
using CorpsMod.Content.Items.Placeable.Furniture;
using CorpsMod.Content.Items.Tools;
using CorpsMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CorpsMod.Content.NPCs
{
	[AutoloadHead]
	class ExampleTravelingMerchant : ModNPC
	{
		// 旅人が出発する時刻（午後6時）
		public const double despawnTime = 48600.0;

		// 旅人が出現する時刻（出現しない場合はdouble.MaxValue）。TravelingMerchantSystemに保存され、ワー​​ルドと共にロードされます。
		public static double spawnTime = double.MaxValue;

		// 旅人のショップにあるアイテムのリスト。ワールドに保存され、旅人がスポーンした際に設定されます。マルチプレイヤーではサーバーからクライアントに同期されます。
		public readonly static List<Item> shopItems = new();

		// 宣言型ショップの静的インスタンス。持ち込めるすべてのアイテムを定義します。NPCがスポーンしたときに新しいインベントリを作成するために使用されます。
		public static ExampleTravelingMerchantShop Shop;

		private static int ShimmerHeadIndex;
		private static Profiles.StackedNPCProfile NPCProfile;

		public override bool PreAI() {
			// もしデスポーン時間を過ぎていて（午後6時以降）、かつNPCが画面上にいない場合
			if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) {
				// ここでNPCをデスポーンさせ、NPCがデスポーンしたことを通知するメッセージを送信します。
				// LegacyMisc.35 は「{0} が出発しました！」です。
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), 50, 125, 255);
				}
				else {
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), new Color(50, 125, 255));
				}
				NPC.active = false;
				NPC.netSkip = -1;
				NPC.life = 0;
				return false;
			}

			return true;
		}

		public override void AddShops() {
			Shop = new ExampleTravelingMerchantShop(NPC.type);

			// 常に売るアイテム
			Shop.Add<ExampleItem>();

			// 2つのツールを持ってくる
			Shop.AddPool("Tools", slots: 2)
			.Add<ExampleDrill>()
			.Add<ExampleHamaxe>()
			.Add<ExampleFishingRod>()
			.Add<ExampleHookItem>()
			.Add<ExampleBugNet>()
			.Add<ExamplePickaxe>();

			// 4つの武器を持ってくる
			Shop.AddPool("Weapons", slots: 4)
			.Add<ExampleSword>()
			.Add<ExampleShortsword>()
			.Add<ExampleShootingSword>()
			.Add<ExampleJavelin>()
			.Add<ExampleSpear>()
			.Add<ExampleMagicWeapon>()
			.Add<ExampleGun>()
			.Add<ExampleShotgun>()
			.Add<ExampleMinigun>()
			.Add<ExampleFlail>()
			.Add<ExampleAdvancedFlail>(Condition.Hardmode) // ハードモードでのみ上位の例を持ってくる！
			.Add<ExampleWhip>()
			.Add<ExampleWhipAdvanced>(Condition.Hardmode)
			.Add<ExampleYoyo>();

			// 3つの家具を持ってくる
			Shop.AddPool("Furniture", slots: 3)
			.Add<ExampleLamp>()
			.Add<ExampleBed>()
			.Add<ExampleChair>()
			.Add<ExampleChest>()
			.Add<ExampleClock>()
			.Add<ExampleDoor>()
			.Add<ExampleSink>()
			.Add<ExampleTable>()
			.Add<ExampleToilet>()
			.Add<ExampleWorkbench>();

			Shop.Register();
		}

		// 行商NPCのスポーンロジックは、ModSystemの代わりにNPC自身で処理されます。
		public static void UpdateTravelingMerchant() {
			bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<ExampleTravelingMerchant>()) != -1); // ワールドにスポーンしている行商人を見つける

			// Main.timeは毎朝0に設定され、1回のアップデートでのみ実行されます。サンダイアルを使用してもtime 0をスキップすることはないため、ここは「新しい日になった」ときのコードを記述するのに適した場所です。
			if (Main.dayTime && Main.time == 0) {
				// ここに、他の条件（到着したNPCやプレイヤーが達成したマイルストーンなど）に基づいてスポーン確率を変更するコードを挿入できます。
				// また、ここで日数カウンターを追加して、商人が連続した日にスポーンするのを防ぐこともできます。

				// 一晩中滞在した場合、NPCはその日はスポーンしません
				if (!travelerIsThere && Main.rand.NextBool(4)) { // 4 = 25%の確率
																 // ここで、NPCがスポーンする時間を毎回完全に同じにしないように設定できます
					spawnTime = GetRandomSpawnTime(5400, 8100); // 最小時刻 = 午前6:00、最大時刻 = 午前7:30
				}
				else {
					spawnTime = double.MaxValue; // この日はスポーンしない
				}
			}

			// スポーン条件（時間帯、イベントなし、サンダイアルなし）が満たされている場合、旅人をスポーンさせます
			if (!travelerIsThere && CanSpawnNow()) {
				int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<ExampleTravelingMerchant>(), 1); // ワールドのスポーン地点にスポーンさせる
				NPC traveler = Main.npc[newTraveler];
				traveler.homeless = true;
				// スポーン地点がワールドの右側にあるか左側にあるかに応じて、旅人の向きを設定します
				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
				traveler.netUpdate = true;

				// 旅人が同じ日に再びスポーンするのを防ぎます
				spawnTime = double.MaxValue;

				// 旅人がスポーンしたことを告知します！
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(Language.GetTextValue("Announcement.HasArrived", traveler.FullName), 50, 125, 255);
				}
				else {
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", traveler.GetFullNetName()), new Color(50, 125, 255));
				}
			}
		}

		private static bool CanSpawnNow() {
			// いずれかのイベントが実行中の場合はスポーンできません
			if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
				return false;

			// サンダイアルがアクティブな場合はスポーンできません
			if (Main.IsFastForwardingTime())
				return false;

			// 昼間で、スポーン時間とデスポーン時間の間にいる場合はスポーンできます
			return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
		}

		private static bool IsNpcOnscreen(Vector2 center) {
			int w = NPC.sWidth + NPC.safeRangeX * 2;
			int h = NPC.sHeight + NPC.safeRangeY * 2;
			Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
			foreach (Player player in Main.ActivePlayers) {
				// いずれかのプレイヤーが旅人に十分近い場合、NPCがデスポーンするのを防ぎます
				if (player.getRect().Intersects(npcScreenRect)) {
					return true;
				}
			}
			return false;
		}

		public static double GetRandomSpawnTime(double minTime, double maxTime) {
			// 選択された2つの時間の間のランダムな時間を取得するための単純な式
			return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
		}

		public override void Load() {
			// NPCHeadLoaderにシマーヘッドを追加します。
			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25;
			NPCID.Sets.ExtraFramesCount[Type] = 9;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 60;
			NPCID.Sets.AttackType[Type] = 3; // 武器を振ります。このNPCはスタイリストとほぼ同じ方法で攻撃します
			NPCID.Sets.AttackTime[Type] = 12;
			NPCID.Sets.AttackAverageChance[Type] = 1;
			NPCID.Sets.HatOffsetY[Type] = 4;
			NPCID.Sets.ShimmerTownTransform[Type] = true;
			NPCID.Sets.NoTownNPCHappiness[Type] = true; // 幸福度ボタンを無効にします
			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleTravellingMerchantEmote>();

			// BestiaryでのNPCの見た目に影響を与えます
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 2f, // NPCがx方向に+2タイル歩いているかのようにBestiaryに描画します
				Direction = -1 // -1は左、1は右です。
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
			new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
			new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex)
			);
		}

		public override void SetDefaults() {
			NPC.townNPC = true; // Town NPCとしてマークしますが、手動でスポーンさせるためCanTownNPCSpawnはfalseです
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.Stylist;
			TownNPCStayingHomeless = true; // このNPCが家に定住しないようにします
		}

		public override void OnSpawn(IEntitySource source) {
			shopItems.Clear();
			shopItems.AddRange(Shop.GenerateNewInventoryList()); // ショップクラスからランダムな在庫リストを生成します

			// マルチプレイヤーでは、ショップアイテムがクライアントと同期されることを確認します (TravelingMerchantSystem.csを参照)
			if (Main.netMode == NetmodeID.Server) {
				// Modderには、WorldDataを頻繁に送信したり、過度に多くのデータで埋めたりしないことを推奨します。そうしないと、冗長なデータを繰り返し送信するために帯域幅を消費しすぎる可能性があります。
				// 同期する必要があるデータ量が多すぎる場合は、WorldDataの代わりにカスタムパケットを送信することを検討してください。
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange([
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface // 表層バイオームでスポーンすることを示す
			]);
		}

		public override void HitEffect(NPC.HitInfo hit) {
			int num = NPC.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
			}

			// NPCが倒されたときにゴアを作成します。
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// ゴアのタイプを取得します。このNPCには頭、腕、脚のゴアのシマーバリアントがあります。また、カスタムハットゴアもあります。(合計7つのゴア)
				// このNPCは、シマー化されていない場合、割り当てられたパーティーハットまたはカスタムハットゴアのいずれかをスポーンします。シマー化されている場合、トップハットは頭の一部であるため、ハットゴアはスポーンしません。
				int hatGore = NPC.GetPartyHatGore();
				// パーティーハットを着用しておらず、シマー化されていない場合は、カスタムハットゴアを取得します
				if (hatGore == 0 && !NPC.IsShimmerVariant) {
					hatGore = Mod.Find<ModGore>($"{Name}_Gore_Hat").Type;
				}
				string variant = "";
				if (NPC.IsShimmerVariant)
					variant += "_Shimmer";
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

				// ゴアをスポーンさせます。腕と脚の位置は、より自然な見た目にするために下げられています。
				if (hatGore > 0) {
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
				}
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			}
		}

		public override bool UsesPartyHat() {
			// ExampleTravelingMerchantはシマー化されている間は帽子を着用したままにしたいと考えています。（コード内のコメントとロジックが矛盾しているように見えるため、ここではロジックに従い、シマー化されている場合はパーティーハットを使用しないようにします）
			if (NPC.IsShimmerVariant) {
				return false;
			}
			return true;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			return false; // これは常にfalseであるべきです。なぜなら、行商NPCは手動でスポーンさせるからです
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			return new List<string>() {
				"Someone Else",
				"Somebody Else",
				"Blockster",
				"Colorful",
				"キルト"
};
		}

		public override string GetChat() {
			WeightedRandom<string> chat = new WeightedRandom<string>();

			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0) {
				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
			}

			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue3"));

			string hivePackDialogue = Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.HiveBackpackDialogue");
			chat.Add(hivePackDialogue);

			string dialogueLine = chat; // chatは暗黙的に文字列にキャストされます。
			if (hivePackDialogue.Equals(dialogueLine)) {
				// Main.npcChatCornerItemは、釣り人のクエストチャットのように、隅に単一のアイテムを表示します。
				Main.npcChatCornerItem = ItemID.HiveBackpack;
			}

			return dialogueLine;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = Language.GetTextValue("LegacyInterface.28"); // これは「ショップ」という単語のキーです
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				shop = Shop.Name; // ショップを開きます
			}
		}

		public override void AI() {
			NPC.homeless = true; // ホームレス状態を維持するようにします
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>())); // 倒したときにカスタムコスチュームをドロップするように追加
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 15;
			randExtraCooldown = 8;
		}

		public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
			itemWidth = itemHeight = 40;
		}

		public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) {
			Main.GetItemDrawFrame(ModContent.ItemType<ExampleSword>(), out item, out itemFrame);
			itemSize = 40;
			// この調整により、町のNPCが通常行う方法でスイングが描画されます。
			if (NPC.ai[1] > NPCID.Sets.AttackTime[NPC.type] * 0.66f) {
				offset.Y = 12f;
			}
		}
	}

	// ショップのプール（Pool）コンセプト
	// ショップは、アイテムが等しい重みでプールからランダムに選択される「プール」コンセプトを使用します。
	// この例では、NPCShopとNPCShop.Entryの多くのコードをコピーし、他のModによるショップの調整を容易にしています。
	// 
	// これは、コードが過度に長くなるのを避けるために、かなり高度なC#を使用しています。大幅に改変しようとする前に、C#言語を学ぶようにしてください。
	public class ExampleTravelingMerchantShop : AbstractNPCShop
	{
		// ショップのエントリ（アイテムと条件）
		public new record Entry(Item Item, List<Condition> Conditions) : AbstractNPCShop.Entry
		{
			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;

			public bool Disabled { get; private set; }

			public Entry Disable() {
				Disabled = true;
				return this;
			}

			// 条件がすべて満たされているかどうかを確認
			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
		}

		// アイテムのプール
		public record Pool(string Name, int Slots, List<Entry> Entries)
		{
			public Pool Add(Item item, params Condition[] conditions) {
				Entries.Add(new Entry(item, conditions.ToList()));
				return this;
			}

			public Pool Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
			public Pool Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

			// 条件が満たされているエントリリストから、指定された数（Slotsまで）のアイテムを選択します。
			public IEnumerable<Item> PickItems() {
				// これは、重複なしでアイテムを選択する高速な方法ではありませんが、確かに簡単です。これを1フレームで何度も実行したり、巨大なアイテムリストで実行したりしないように注意してください。
				var list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();
				for (int i = 0; i < Slots; i++) {
					if (list.Count == 0)
						break;

					int k = Main.rand.Next(list.Count);
					yield return list[k].Item;

					// 今回の選択で再び選択されないように、リストからエントリを削除します
					list.RemoveAt(k);
				}
			}
		}

		public List<Pool> Pools { get; } = new();

		public ExampleTravelingMerchantShop(int npcType) : base(npcType) { }

		public override IEnumerable<Entry> ActiveEntries => Pools.SelectMany(p => p.Entries).Where(e => !e.Disabled);

		public Pool AddPool(string name, int slots) {
			var pool = new Pool(name, slots, new List<Entry>());
			Pools.Add(pool);
			return pool;
		}

		// 単一アイテムのプールを追加するいくつかのメソッド
		public void Add(Item item, params Condition[] conditions) => AddPool(item.ModItem?.FullName ?? $"Terraria/{item.type}", slots: 1).Add(item, conditions);
		public void Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
		public void Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

		// ここで実際にショップの内容を「ロール（抽選）」します
		public List<Item> GenerateNewInventoryList() {
			var items = new List<Item>();
			foreach (var pool in Pools) {
				items.AddRange(pool.PickItems());
			}
			return items;
		}

		public override void FillShop(ICollection<Item> items, NPC npc) {
			// NPCがスポーンしたときに選択されたアイテムを使用します。
			foreach (var item in ExampleTravelingMerchant.shopItems) {
				// ショップが開かれたときにModifyActiveShopフックがアイテムを調整する場合に備えて、アイテムのクローンを追加するようにします
				items.Add(item.Clone());
			}
		}

		public override void FillShop(Item[] items, NPC npc, out bool overflow) {
			overflow = false;
			int i = 0;
			// NPCがスポーンしたときに選択されたアイテムを使用します。
			foreach (var item in ExampleTravelingMerchant.shopItems) {

				if (i == items.Length - 1) {
					// 販売用に最後のスロットを空けておきます
					overflow = true;
					return;
				}

				// ショップが開かれたときにModifyActiveShopフックがアイテムを調整する場合に備えて、アイテムのクローンを追加するようにします
				items[i++] = item.Clone();
			}
		}
	}
}

////行商npcのサンプル
//using CorpsMod.Content.Dusts;
//using CorpsMod.Content.EmoteBubbles;
//using CorpsMod.Content.Items;
//using CorpsMod.Content.Items.Armor;
//using CorpsMod.Content.Items.Placeable;
//using CorpsMod.Content.Items.Placeable.Furniture;
//using CorpsMod.Content.Items.Tools;
//using CorpsMod.Content.Items.Weapons;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System.Collections.Generic;
//using System.Linq;
//using Terraria;
//using Terraria.Chat;
//using Terraria.DataStructures;
//using Terraria.GameContent;
//using Terraria.GameContent.Bestiary;
//using Terraria.GameContent.ItemDropRules;
//using Terraria.ID;
//using Terraria.Localization;
//using Terraria.ModLoader;
//using Terraria.Utilities;

//namespace CorpsMod.Content.NPCs
//{
//	[AutoloadHead]
//	class ExampleTravelingMerchant : ModNPC
//	{
//		//旅行者が出発する時刻（午後6時）
//		public const double despawnTime = 48600.0;

//		// the time of day the traveler will spawn (double.MaxValue for no spawn). Saved and loaded with the world in TravelingMerchantSystem
//		// 旅行者が出現する時刻（出現しない場合はdouble.MaxValue）。TravelingMerchantSystemに保存され、ワー​​ルドと共にロードされます。
//		public static double spawnTime = double.MaxValue;

//		// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns. Synced by the server to clients in multi player
//		// 旅人のショップにあるアイテムのリスト。ワールドに保存され、旅人がスポーンした際に設定されます。マルチプレイヤーではサーバーからクライアントに同期されます。
//		public readonly static List<Item> shopItems = new();

//		// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
//		// 宣言型ショップの静的インスタンス。持ち込めるすべてのアイテムを定義します。NPCがスポーンしたときに新しいインベントリを作成するために使用されます。
//		public static ExampleTravelingMerchantShop Shop;

//		private static int ShimmerHeadIndex;
//		private static Profiles.StackedNPCProfile NPCProfile;

//		public override bool PreAI() {
//			if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
//			{
//				// Here we despawn the NPC and send a message stating that the NPC has despawned
//				// LegacyMisc.35 is {0) has departed!
//				// ここでNPCをデスポーンし、NPCがデスポーンしたことを通知するメッセージを送信します。
//				// LegacyMisc.35 is {0) が出発しました！
//				if (Main.netMode == NetmodeID.SinglePlayer) {
//					Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), 50, 125, 255);
//				}
//				else {
//					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), new Color(50, 125, 255));
//				}
//				NPC.active = false;
//				NPC.netSkip = -1;
//				NPC.life = 0;
//				return false;
//			}

//			return true;
//		}

//		public override void AddShops() {
//			Shop = new ExampleTravelingMerchantShop(NPC.type);

//			// 常に売るアイテム
//			Shop.Add<ExampleItem>();

//			// Bring 2 Tools
//			//持ってくる2つのツール
//			Shop.AddPool("Tools", slots: 2)
//				.Add<ExampleDrill>()
//				.Add<ExampleHamaxe>()
//				.Add<ExampleFishingRod>()
//				.Add<ExampleHookItem>()
//				.Add<ExampleBugNet>()
//				.Add<ExamplePickaxe>();

//			// Bring 4 Weapons
//			// 4つの武器を持ってくる
//			Shop.AddPool("Weapons", slots: 4)
//				.Add<ExampleSword>()
//				.Add<ExampleShortsword>()
//				.Add<ExampleShootingSword>()
//				.Add<ExampleJavelin>()
//				.Add<ExampleSpear>()
//				.Add<ExampleMagicWeapon>()
//				.Add<ExampleGun>()
//				.Add<ExampleShotgun>()
//				.Add<ExampleMinigun>()
//				.Add<ExampleFlail>()
//				.Add<ExampleAdvancedFlail>(Condition.Hardmode) // Only bring advanced examples in hardmode!
//				.Add<ExampleWhip>()
//				.Add<ExampleWhipAdvanced>(Condition.Hardmode)
//				.Add<ExampleYoyo>();

//			// Bring 3 Furniture
//			// 3つの家具を持ってくる
//			Shop.AddPool("Furniture", slots: 3)
//				.Add<ExampleLamp>()
//				.Add<ExampleBed>()
//				.Add<ExampleChair>()
//				.Add<ExampleChest>()
//				.Add<ExampleClock>()
//				.Add<ExampleDoor>()
//				.Add<ExampleSink>()
//				.Add<ExampleTable>()
//				.Add<ExampleToilet>()
//				.Add<ExampleWorkbench>();

//			Shop.Register();
//		}

//		public static void UpdateTravelingMerchant() {
//			bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<ExampleTravelingMerchant>()) != -1); // Find a Merchant if there's one spawned in the world

//			// Main.time is set to 0 each morning, and only for one update. Sundialling will never skip past time 0 so this is the place for 'on new day' code
//			if (Main.dayTime && Main.time == 0) {
//				// insert code here to change the spawn chance based on other conditions (say, NPCs which have arrived, or milestones the player has passed)
//				// You can also add a day counter here to prevent the merchant from possibly spawning multiple days in a row.

//				// NPC won't spawn today if it stayed all night
//				if (!travelerIsThere && Main.rand.NextBool(4)) { // 4 = 25% Chance
//					// Here we can make it so the NPC doesn't spawn at the EXACT same time every time it does spawn
//					spawnTime = GetRandomSpawnTime(5400, 8100); // minTime = 6:00am, maxTime = 7:30am
//				}
//				else {
//					spawnTime = double.MaxValue; // no spawn today
//				}
//			}

//			// Spawn the traveler if the spawn conditions are met (time of day, no events, no sundial)
//			if (!travelerIsThere && CanSpawnNow()) {
//				int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<ExampleTravelingMerchant>(), 1); // Spawning at the world spawn
//				NPC traveler = Main.npc[newTraveler];
//				traveler.homeless = true;
//				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
//				traveler.netUpdate = true;

//				// Prevents the traveler from spawning again the same day
//				spawnTime = double.MaxValue;

//				// Announce that the traveler has spawned in!
//				if (Main.netMode == NetmodeID.SinglePlayer) {
//					Main.NewText(Language.GetTextValue("Announcement.HasArrived", traveler.FullName), 50, 125, 255);
//				}
//				else {
//					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", traveler.GetFullNetName()), new Color(50, 125, 255));
//				}
//			}
//		}

//		private static bool CanSpawnNow() {
//			// can't spawn if any events are running
//			if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
//				return false;

//			// can't spawn if the sundial is active
//			if (Main.IsFastForwardingTime())
//				return false;

//			// can spawn if daytime, and between the spawn and despawn times
//			return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
//		}

//		private static bool IsNpcOnscreen(Vector2 center) {
//			int w = NPC.sWidth + NPC.safeRangeX * 2;
//			int h = NPC.sHeight + NPC.safeRangeY * 2;
//			Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
//			foreach (Player player in Main.ActivePlayers) {
//				// If any player is close enough to the traveling merchant, it will prevent the npc from despawning
//				if (player.getRect().Intersects(npcScreenRect)) {
//					return true;
//				}
//			}
//			return false;
//		}

//		public static double GetRandomSpawnTime(double minTime, double maxTime) {
//			// A simple formula to get a random time between two chosen times
//			return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
//		}

//		public override void Load() {
//			// Adds our Shimmer Head to the NPCHeadLoader.
//			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
//		}

//		public override void SetStaticDefaults() {
//			Main.npcFrameCount[Type] = 25;
//			NPCID.Sets.ExtraFramesCount[Type] = 9;
//			NPCID.Sets.AttackFrameCount[Type] = 4;
//			NPCID.Sets.DangerDetectRange[Type] = 60;
//			NPCID.Sets.AttackType[Type] = 3; // Swings a weapon. This NPC attacks in roughly the same manner as Stylist
//			NPCID.Sets.AttackTime[Type] = 12;
//			NPCID.Sets.AttackAverageChance[Type] = 1;
//			NPCID.Sets.HatOffsetY[Type] = 4;
//			NPCID.Sets.ShimmerTownTransform[Type] = true;
//			NPCID.Sets.NoTownNPCHappiness[Type] = true; // Prevents the happiness button
//			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleTravellingMerchantEmote>();

//			// Influences how the NPC looks in the Bestiary
//			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
//				Velocity = 2f, // Draws the NPC in the bestiary as if its walking +2 tiles in the x direction
//				Direction = -1 // -1 is left and 1 is right.
//			};

//			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

//			NPCProfile = new Profiles.StackedNPCProfile(
//				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
//				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex)
//			);
//		}

//		public override void SetDefaults() {
//			NPC.townNPC = true;
//			NPC.friendly = true;
//			NPC.width = 18;
//			NPC.height = 40;
//			NPC.aiStyle = 7;
//			NPC.damage = 10;
//			NPC.defense = 15;
//			NPC.lifeMax = 250;
//			NPC.HitSound = SoundID.NPCHit1;
//			NPC.DeathSound = SoundID.NPCDeath1;
//			NPC.knockBackResist = 0.5f;
//			AnimationType = NPCID.Stylist;
//			TownNPCStayingHomeless = true;
//		}

//		public override void OnSpawn(IEntitySource source) {
//			shopItems.Clear();
//			shopItems.AddRange(Shop.GenerateNewInventoryList());

//			// In multi player, ensure the shop items are synced with clients (see TravelingMerchantSystem.cs)
//			if (Main.netMode == NetmodeID.Server) {
//				// We recommend modders avoid sending WorldData too often, or filling it with too much data, lest too much bandwidth be consumed sending redundant data repeatedly
//				// Consider sending a custom packet instead of WorldData if you have a significant amount of data to synchronize
//				NetMessage.SendData(MessageID.WorldData);
//			}
//		}

//		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
//			bestiaryEntry.Info.AddRange([
//				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface
//			]);
//		}

//		public override void HitEffect(NPC.HitInfo hit) {
//			int num = NPC.life > 0 ? 1 : 5;
//			for (int k = 0; k < num; k++) {
//				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
//			}

//			// Create gore when the NPC is killed.
//			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
//				// Retrieve the gore types. This NPC has shimmer variants for head, arm, and leg gore. It also has a custom hat gore. (7 gores)
//				// This NPC will spawn either the assigned party hat or a custom hat gore when not shimmered. When shimmered the top hat is part of the head and no hat gore is spawned.
//				int hatGore = NPC.GetPartyHatGore();
//				// If not wearing a party hat, and not shimmered, retrieve the custom hat gore 
//				if (hatGore == 0 && !NPC.IsShimmerVariant) {
//					hatGore = Mod.Find<ModGore>($"{Name}_Gore_Hat").Type;
//				}
//				string variant = "";
//				if (NPC.IsShimmerVariant)
//					variant += "_Shimmer";
//				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
//				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
//				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

//				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
//				if (hatGore > 0) {
//					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
//				}
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
//				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
//			}
//		}

//		public override bool UsesPartyHat() {
//			// ExampleTravelingMerchant likes to keep his hat on while shimmered.
//			if (NPC.IsShimmerVariant) {
//				return false;
//			}
//			return true;
//		}

//		public override bool CanTownNPCSpawn(int numTownNPCs) {
//			return false; // This should always be false, because we spawn in the Traveling Merchant manually
//		}

//		public override ITownNPCProfile TownNPCProfile() {
//			return NPCProfile;
//		}

//		public override List<string> SetNPCNameList() {
//			return new List<string>() {
//				"Someone Else",
//				"Somebody Else",
//				"Blockster",
//				"Colorful"
//			};
//		}

//		public override string GetChat() {
//			WeightedRandom<string> chat = new WeightedRandom<string>();

//			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
//			if (partyGirl >= 0) {
//				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
//			}

//			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue1"));
//			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue2"));
//			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue3"));

//			string hivePackDialogue = Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.HiveBackpackDialogue");
//			chat.Add(hivePackDialogue);

//			string dialogueLine = chat; // chat is implicitly cast to a string.
//			if (hivePackDialogue.Equals(dialogueLine)) {
//				// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
//				Main.npcChatCornerItem = ItemID.HiveBackpack;
//			}

//			return dialogueLine;
//		}

//		public override void SetChatButtons(ref string button, ref string button2) {
//			button = Language.GetTextValue("LegacyInterface.28"); // This is the key to the word "Shop"
//		}

//		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
//			if (firstButton) {
//				shop = Shop.Name; // Opens the shop
//			}
//		}

//		public override void AI() {
//			NPC.homeless = true; // Make sure it stays homeless
//		}

//		public override void ModifyNPCLoot(NPCLoot npcLoot) {
//			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>()));
//		}

//		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
//			damage = 20;
//			knockback = 4f;
//		}

//		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
//			cooldown = 15;
//			randExtraCooldown = 8;
//		}

//		public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight) {
//			itemWidth = itemHeight = 40;
//		}

//		public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) {
//			Main.GetItemDrawFrame(ModContent.ItemType<ExampleSword>(), out item, out itemFrame);
//			itemSize = 40;
//			// This adjustment draws the swing the way town npcs usually do.
//			if (NPC.ai[1] > NPCID.Sets.AttackTime[NPC.type] * 0.66f) {
//				offset.Y = 12f;
//			}
//		}
//	}

//	// You have the freedom to implement custom shops however you want
//	// This example uses a 'pool' concept where items will be randomly selected from a pool with equal weight
//	// We copy a bunch of code from NPCShop and NPCShop.Entry, allowing this shop to be easily adjusted by other mods.
//	// 
//	// This uses some fairly advanced C# to avoid being excessively long, so make sure you learn the language before trying to adapt it significantly
//	public class ExampleTravelingMerchantShop : AbstractNPCShop
//	{
//		public new record Entry(Item Item, List<Condition> Conditions) : AbstractNPCShop.Entry
//		{
//			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;

//			public bool Disabled { get; private set; }

//			public Entry Disable() {
//				Disabled = true;
//				return this;
//			}

//			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
//		}

//		public record Pool(string Name, int Slots, List<Entry> Entries)
//		{
//			public Pool Add(Item item, params Condition[] conditions) {
//				Entries.Add(new Entry(item, conditions.ToList()));
//				return this;
//			}

//			public Pool Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
//			public Pool Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

//			// Picks a number of items (up to Slots) from the entries list, provided conditions are met.
//			public IEnumerable<Item> PickItems() {
//				// This is not a fast way to pick items without replacement, but it's certainly easy. Be careful not to do this many many times per frame, or on huge lists of items.
//				var list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();
//				for (int i = 0; i < Slots; i++) {
//					if (list.Count == 0)
//						break;

//					int k = Main.rand.Next(list.Count);
//					yield return list[k].Item;

//					// remove the entry from the list so it can't be selected again this pick
//					list.RemoveAt(k);
//				}
//			}
//		}

//		public List<Pool> Pools { get; } = new();

//		public ExampleTravelingMerchantShop(int npcType) : base(npcType) { }

//		public override IEnumerable<Entry> ActiveEntries => Pools.SelectMany(p => p.Entries).Where(e => !e.Disabled);

//		public Pool AddPool(string name, int slots) {
//			var pool = new Pool(name, slots, new List<Entry>());
//			Pools.Add(pool);
//			return pool;
//		}

//		// Some methods to add a pool with a single item
//		public void Add(Item item, params Condition[] conditions) => AddPool(item.ModItem?.FullName ?? $"Terraria/{item.type}", slots: 1).Add(item, conditions);
//		public void Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
//		public void Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

//		// Here is where we actually 'roll' the contents of the shop
//		public List<Item> GenerateNewInventoryList() {
//			var items = new List<Item>();
//			foreach (var pool in Pools) {
//				items.AddRange(pool.PickItems());
//			}
//			return items;
//		}

//		public override void FillShop(ICollection<Item> items, NPC npc) {
//			// use the items which were selected when the NPC spawned.
//			foreach (var item in ExampleTravelingMerchant.shopItems) {
//				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
//				items.Add(item.Clone());
//			}
//		}

//		public override void FillShop(Item[] items, NPC npc, out bool overflow) {
//			overflow = false;
//			int i = 0;
//			// use the items which were selected when the NPC spawned.
//			foreach (var item in ExampleTravelingMerchant.shopItems) {

//				if (i == items.Length - 1) {
//					// leave the last slot empty for selling
//					overflow = true;
//					return;
//				}

//				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
//				items[i++] = item.Clone();
//			}
//		}
//	}
//}