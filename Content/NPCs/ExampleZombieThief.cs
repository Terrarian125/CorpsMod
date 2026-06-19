using CorpsMod.Content.Biomes;
using CorpsMod.Content.EmoteBubbles;
using CorpsMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;

namespace CorpsMod.Content.NPCs
{
	// ExampleZombieThief（ゾンビ泥棒）は、基本的には通常のゾンビと同じですが、ExampleItemを盗み、倒されるまでそれを保持します。十分な量を持っている場合は、ワールドデータと一緒に保存されます。
	public class ExampleZombieThief : ModNPC
	{
		public int StolenItems = 0;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				// 生物図鑑でのNPCの見え方に影響します
				Velocity = 1f // 生物図鑑内で、X方向に+1マス歩いているかのように描画します
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = 14;
			NPC.defense = 6;
			NPC.lifeMax = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = 3; // ファイターAI。真似したいNPCIDと一致するaiStyleを選ぶことが重要です

			AIType = NPCID.Zombie; // AIコードを実行する際、バニラのゾンビのタイプを使用します（これは昼間にデスポーンしようとすることも意味します）
			AnimationType = NPCID.Zombie; // アニメーションコードを実行する際、バニラのゾンビのタイプを使用します。SetStaticDefaultsでMain.npcFrameCount[NPC.type]を合わせることも重要です。
			Banner = Item.NPCtoBanner(NPCID.Zombie); // このNPCが通常のゾンビバナー（旗）の効果を受けるようにします。
			BannerItem = Item.BannerToItem(Banner); // このNPCを倒した際に、関連付けられたバナーがドロップするようにします。
			SpawnModBiomes = [ModContent.GetInstance<ExampleSurfaceBiome>().Type]; // 生物図鑑でこのNPCをExampleSurfaceBiomeに関連付けます
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// AddRangeを使用することで、一度に複数の項目をまとめて追加できます
			bestiaryEntry.Info.AddRange([
				// 生物図鑑に表示される、このNPCの出現条件（時間帯）を設定します。
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// 生物図鑑に表示される、このNPCの説明文（フレーバーテキスト）を設定します。
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleZombieThief"),
			]);
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			Rectangle hitbox = NPC.Hitbox;
			foreach (Item item in Main.item) {
				// NPCがアイテムに触れており、かつそのアイテムがまだプレイヤーに引き寄せられていない（拾われていない）場合のみ、アイテムを拾います
				if (item.active && !item.beingGrabbed && item.type == ModContent.ItemType<ExampleItem>() && hitbox.Intersects(item.Hitbox)) {
					item.active = false;
					StolenItems += item.stack;

					NetMessage.SendData(MessageID.SyncItem, number: item.whoAmI);

					// ExampleItemを盗んだ時にエモート（感情アイコン）を表示します
					EmoteBubble.NewBubble(ModContent.EmoteBubbleType<ExampleItemEmote>(), new WorldUIAnchor(NPC), 90);
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(StolenItems);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			StolenItems = reader.ReadInt32();
		}

		public override void OnKill() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				return;
			}

			// NPCが死亡した際、盗まれたアイテムをすべてドロップします
			while (StolenItems > 0) {
				// 一度にmaxStack（最大スタック数）以上のアイテムをドロップするのを防ぐため、すべてのアイテムがドロップされるまでループします
				int droppedAmount = Math.Min(ModContent.GetInstance<ExampleItem>().Item.maxStack, StolenItems);
				StolenItems -= droppedAmount;
				Item.NewItem(NPC.GetSource_Death(), NPC.Center, ModContent.ItemType<ExampleItem>(), droppedAmount, true);
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// ExampleSurfaceBiomeにプレイヤーがいて、かつ他にExampleZombieThiefが存在しない場合のみスポーンできます
			if (spawnInfo.Player.InModBiome(ModContent.GetInstance<ExampleSurfaceBiome>()) && !NPC.AnyNPCs(Type)) {
				return SpawnCondition.OverworldNightMonster.Chance * 0.1f; // 通常のゾンビの1/10の確率でスポーンします。
			}

			return 0f;
		}

		public override bool NeedSaving() {
			return StolenItems >= 10; // アイテムを少数しか持っていないNPCをメモリに保持し続けるのを避けるため、10個以上盗んでいる場合のみ保存します
		}

		public override void SaveData(TagCompound tag) {
			if (StolenItems > 0) {
				// 他のModやシステムがこのNPCを保存すると決定した場合、この時点で盗んだアイテムが10個未満である可能性もあります
				tag["StolenItems"] = StolenItems;
			}
		}

		public override void LoadData(TagCompound tag) {
			StolenItems = tag.GetInt("StolenItems");
		}
	}
}