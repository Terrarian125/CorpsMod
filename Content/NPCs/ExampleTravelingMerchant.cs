using CorpsMod.Content.Dusts;
using CorpsMod.Content.EmoteBubbles;
using CorpsMod.Content.Items;
using CorpsMod.Content.Items.Armor;
using CorpsMod.Content.Items.Placeable;
using CorpsMod.Content.Items.Placeable.Furniture;
using CorpsMod.Content.Items.Tools;
using CorpsMod.Content.Items.Weapons;
using CorpsMod.Common.Systems; // システムへの参照を追加
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
using System; // Math.Floor を使用するために追加

namespace CorpsMod.Content.NPCs
{
	[AutoloadHead]
	class ExampleTravelingMerchant : ModNPC
	{
		public const double despawnTime = 48600.0;
		public static double spawnTime = double.MaxValue;
		public readonly static List<Item> shopItems = new();
		public static ExampleTravelingMerchantShop Shop;

		private static int ShimmerHeadIndex;
		private static Profiles.StackedNPCProfile NPCProfile;

		// ★ クエスト機能のために要求アイテムを定義
		private const int QuestItemType = ItemID.BossMaskMoonlord; // 仮の要求アイテム

		public override bool PreAI() {
			// ★ 定住済みなら行商人のデスポーンロジックをスキップ
			if (TravelingMerchantSystem.HasSettled) {
				return true;
			}

			// もしデスポーン時間を過ぎていて（午後6時以降）、かつNPCが画面上にいない場合
			if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) {
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
			// ... (ショップの定義は変更なし) ...
			Shop = new ExampleTravelingMerchantShop(NPC.type);

			Shop.Add<ExampleItem>();

			Shop.AddPool("Tools", slots: 2)
				.Add<ExampleDrill>()
				.Add<ExampleHamaxe>()
				.Add<ExampleFishingRod>()
				.Add<ExampleHookItem>()
				.Add<ExampleBugNet>()
				.Add<ExamplePickaxe>();

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
				.Add<ExampleAdvancedFlail>(Condition.Hardmode)
				.Add<ExampleWhip>()
				.Add<ExampleWhipAdvanced>(Condition.Hardmode)
				.Add<ExampleYoyo>();

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

		// ★ 行商NPCのスポーンロジックをシステムから呼び出されるメソッドに分離
		public static void UpdateTravelingMerchantLogic() {
			bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<ExampleTravelingMerchant>()) != -1);

			if (Main.dayTime && Main.time == 0) {
				if (!travelerIsThere && Main.rand.NextBool(4)) {
					spawnTime = GetRandomSpawnTime(5400, 8100);
				}
				else {
					spawnTime = double.MaxValue;
				}
			}

			if (!travelerIsThere && CanSpawnNow()) {
				int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<ExampleTravelingMerchant>(), 1);
				NPC traveler = Main.npc[newTraveler];
				traveler.homeless = true;
				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
				traveler.netUpdate = true;

				spawnTime = double.MaxValue;

				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText(Language.GetTextValue("Announcement.HasArrived", traveler.FullName), 50, 125, 255);
				}
				else {
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", traveler.GetFullNetName()), new Color(50, 125, 255));
				}
			}
		}

		private static bool CanSpawnNow() {
			if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
				return false;
			if (Main.IsFastForwardingTime())
				return false;

			return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
		}

		private static bool IsNpcOnscreen(Vector2 center) {
			int w = NPC.sWidth + NPC.safeRangeX * 2;
			int h = NPC.sHeight + NPC.safeRangeY * 2;
			Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
			foreach (Player player in Main.ActivePlayers) {
				if (player.getRect().Intersects(npcScreenRect)) {
					return true;
				}
			}
			return false;
		}

		public static double GetRandomSpawnTime(double minTime, double maxTime) {
			return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
		}

		public override void Load() {
			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 25;
			NPCID.Sets.ExtraFramesCount[Type] = 9;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 60;
			NPCID.Sets.AttackType[Type] = 3;
			NPCID.Sets.AttackTime[Type] = 12;
			NPCID.Sets.AttackAverageChance[Type] = 1;
			NPCID.Sets.HatOffsetY[Type] = 4;
			NPCID.Sets.ShimmerTownTransform[Type] = true;
			NPCID.Sets.NoTownNPCHappiness[Type] = true;
			NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleTravellingMerchantEmote>();

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 2f,
				Direction = -1
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex)
			);
		}

		public override void SetDefaults() {
			NPC.townNPC = TravelingMerchantSystem.HasSettled; // ★ 定住フラグに応じて設定
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
			// ★ 定住している場合はホームレス状態を維持しない
			TownNPCStayingHomeless = !TravelingMerchantSystem.HasSettled;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			// ★ 定住フラグが立っている場合のみ、通常のTown NPCとしてスポーン可能
			return TravelingMerchantSystem.HasSettled;
		}

		public override void OnSpawn(IEntitySource source) {
			// ★ 定住していない場合のみ在庫を再生成 (定住後は Town NPC の通常の在庫を使用)
			if (!TravelingMerchantSystem.HasSettled) {
				shopItems.Clear();
				shopItems.AddRange(Shop.GenerateNewInventoryList());

				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}

		public override List<string> SetNPCNameList() {
			return new List<string>() {
				"貿易商"
			};
		}

		public override string GetChat() {
			Player player = Main.LocalPlayer;

			// ★ 定住済みの場合の会話
			if (TravelingMerchantSystem.HasSettled) {
				// 通常のTown NPCの会話ロジック
				WeightedRandom<string> settledChat = new WeightedRandom<string>();
				settledChat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.SettledDialogue1"));
				settledChat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.SettledDialogue2"));
				return settledChat;
			}

			// ★ クエスト未完了の場合（行商人状態）
			// ... 既存の行商人の会話ロジック ...
			WeightedRandom<string> chat = new WeightedRandom<string>();
			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0) {
				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
			}

			// クエスト依頼の会話
			if (!player.HasItem(QuestItemType)) {
				// アイテムを持っていない場合
				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.QuestRequest", ItemID.Sets.ItemIcon[QuestItemType]));
			}
			else {
				// アイテムを持っている場合
				chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.QuestReady"));
			}

			// ... 他の既存の会話 ...
			chat.Add(Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.StandardDialogue1"));

			// クエストアイテムのアイコンを角に表示
			Main.npcChatCornerItem = QuestItemType;

			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			// ★ 定住済みか行商人かでボタンを切り替え
			if (TravelingMerchantSystem.HasSettled) {
				button = Language.GetTextValue("LegacyInterface.28"); // ショップ
				button2 = "";
			}
			else {
				Player player = Main.LocalPlayer;
				if (player.HasItem(QuestItemType)) {
					button = Language.GetTextValue("Mods.CorpsMod.Interface.CompleteQuest"); // 依頼を完了
				}
				else {
					button = Language.GetTextValue("Mods.CorpsMod.Interface.AcceptQuest"); // 依頼を受ける
				}
				button2 = Language.GetTextValue("LegacyInterface.28"); // ショップ
			}
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (TravelingMerchantSystem.HasSettled) {
				// 定住済み: ボタン1はショップ
				shop = Shop.Name;
				return;
			}

			if (firstButton) {
				Player player = Main.LocalPlayer;

				if (player.HasItem(QuestItemType)) {
					// ★ クエスト完了ロジック
					player.ConsumeItem(QuestItemType); // アイテムを消費
					TravelingMerchantSystem.HasSettled = true; // 定住フラグを立てる

					// NPCをデスポーンさせ、Town NPCとして再スポーンさせる
					if (Main.netMode == NetmodeID.SinglePlayer) {
						WorldGen.spawnNPC = NPC.type;
					}
					else {
						// マルチプレイでは、Town NPCとしてスポーンするようサーバーに要求し、既存のNPCを強制デスポーンさせます
						NetMessage.SendData(MessageID.RequestImmediateSpawn, -1, -1, null, NPC.type);
					}

					Main.npcChatText = Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.QuestComplete");

					// 既存の行商NPCをデスポーンさせる
					NPC.active = false;
					NPC.netSkip = -1;
					NPC.life = 0;

					// メッセージウィンドウを閉じる
					Main.npcChatText = "";
					Main.playerInventory = false;
					return;
				}
				else {
					// 依頼を受ける（アイテムを持っていない）
					Main.npcChatText = Language.GetTextValue("Mods.CorpsMod.Dialogue.ExampleTravelingMerchant.QuestStatus");
				}
			}
			else {
				// ボタン2はショップ
				shop = Shop.Name;
			}
		}

		public override void AI() {
			// ★ 定住していない場合のみホームレス状態を維持
			if (!TravelingMerchantSystem.HasSettled) {
				NPC.homeless = true;
			}
		}

		// ... (TownNPCAttack, Loot, Draw, etc. は元のコードと同じ) ...

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>()));
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
			if (NPC.ai[1] > NPCID.Sets.AttackTime[NPC.type] * 0.66f) {
				offset.Y = 12f;
			}
		}
	}

	// ... (ExampleTravelingMerchantShop クラスは変更なしでここに含める) ...
	public class ExampleTravelingMerchantShop : AbstractNPCShop
	{
		// ショップのプール（Pool）コンセプト
		public new record Entry(Item Item, List<Condition> Conditions) : AbstractNPCShop.Entry
		{
			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;
			public bool Disabled { get; private set; }
			public Entry Disable() { Disabled = true; return this; }
			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
		}

		public record Pool(string Name, int Slots, List<Entry> Entries)
		{
			public Pool Add(Item item, params Condition[] conditions) { Entries.Add(new Entry(item, conditions.ToList())); return this; }
			public Pool Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
			public Pool Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

			public IEnumerable<Item> PickItems() {
				var list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();
				for (int i = 0; i < Slots; i++) {
					if (list.Count == 0)
						break;
					int k = Main.rand.Next(list.Count);
					yield return list[k].Item;
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

		public void Add(Item item, params Condition[] conditions) => AddPool(item.ModItem?.FullName ?? $"Terraria/{item.type}", slots: 1).Add(item, conditions);
		public void Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
		public void Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

		public List<Item> GenerateNewInventoryList() {
			var items = new List<Item>();
			foreach (var pool in Pools) {
				items.AddRange(pool.PickItems());
			}
			return items;
		}

		public override void FillShop(ICollection<Item> items, NPC npc) {
			// ★ 定住後は通常のショップロジックに任せるか、毎日在庫を再抽選するロジックをここに書くことができます。
			// 今回は、行商人のロジックを再利用し、定住後も日替わり在庫を維持します。
			foreach (var item in ExampleTravelingMerchant.shopItems) {
				items.Add(item.Clone());
			}
		}

		public override void FillShop(Item[] items, NPC npc, out bool overflow) {
			overflow = false;
			int i = 0;
			foreach (var item in ExampleTravelingMerchant.shopItems) {

				if (i == items.Length - 1) {
					overflow = true;
					return;
				}

				items[i++] = item.Clone();
			}
		}
	}
}