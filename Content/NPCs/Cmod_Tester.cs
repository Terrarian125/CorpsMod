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
using Humanizer;
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
using CorpsMod.Content.NPCs;

namespace CorpsMod.Content.NPCs
{
    [AutoloadHead]
    public class Cmod_Tester : ModNPC
    {
        public const string ShopName = "Shop";

        private enum TalkState
        {
            Normal,      // 通常会話
            Challenge    // 挑戦モード
        }

        private TalkState state = TalkState.Normal;

        public override void SetStaticDefaults() {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];
        }

        public override void SetDefaults() {
            NPC.townNPC = true;
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
            AnimationType = NPCID.Guide;
        }

        public override List<string> SetNPCNameList() =>
            new() { "Cmodガイド", "テスター", "CCテスター" };

        public override string GetChat() {
            return state switch {
                TalkState.Challenge => "挑戦したい相手を選べ！",
                _ => GetGuideLikeHint()
            };
        }

        public override void SetChatButtons(ref string button, ref string button2) {
            switch (state) {
                case TalkState.Normal:
                    button = "ショップ";
                    button2 = "挑戦";
                    break;
                case TalkState.Challenge:
                    button = "TestBoss"; // ボタン名を分かりやすく変更
                    button2 = "戻る";
                    break;
            }
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop) {
            Player player = Main.LocalPlayer;

            switch (state) {
                case TalkState.Normal:
                    if (firstButton) {
                        shop = ShopName;
                    }
                    else {
                        state = TalkState.Challenge;
                        Main.npcChatText = "挑戦するのか？";
                    }
                    break;

				case TalkState.Challenge:
					if (firstButton) {
						//SummonBoss(player, ModContent.NPCType<TestBoss>(), "TestBoss");

						state = TalkState.Normal;
					}
					else {
						state = TalkState.Normal;
						Main.npcChatText = "また挑戦したくなったら声をかけてくれ。";
					}
					break;
			}
        }

        private string GetGuideLikeHint() {
            if (Main.bloodMoon)
                return "血だまりで釣りをしたら何が釣れるんだろうね";
            if (!Main.dayTime)
                return "夜だ、君は寝ないのかい？";
            if (Main.dayTime && Main.eclipse)
                return "日食の日は強敵が現れる…稼ぎ時でもある";
            return "やあ、何か買うかい？それとも売る？";
        }

        public override void AddShops() {
            NPCShop shop = new(Type);
            shop.Add(ItemID.Wood)
                .Add(ItemID.Torch)
                .Add(ItemID.HealingPotion)
                .Add(ItemID.Lens)
                .Add(ItemID.FallenStar)
                .Add(ItemID.Gel)
                // 標準的な価格指定方法
                .Add(new Item(ItemID.Gladius) { shopCustomPrice = Item.buyPrice(gold: 2) })
                .Add(new Item(ItemID.Javelin) { shopCustomPrice = Item.buyPrice(copper: 2) })
				   //.Add(Condition.Hardmode, ModContent.ItemType<AntlionRelic>())
				   .Add<AntlionRelic>(Condition.Hardmode)
			.Register();
        }

        public override bool CanTownNPCSpawn(int numTownNPCs) {
            return !NPC.AnyNPCs(NPCID.Guide);
        }

        private void SummonBoss(Player player, int npcType, string bossName) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                Main.npcChatText = "ボス召喚はホストのみ可能だ。";
                return;
            }

            if (NPC.AnyNPCs(npcType)) {
                Main.npcChatText = $"{bossName}はすでに出現している。";
                return;
            }

            // 演出：雷と光
            for (int i = 0; i < 40; i++) {
                Vector2 pos = player.Center + new Vector2(Main.rand.Next(-300, 300), -600);
                Dust.NewDustPerfect(pos, DustID.Electric, Vector2.Zero, 150, Color.Cyan, 2f).noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Thunder, player.Center);
            Main.NewText($"闘士 {bossName} が現れた！", 100, 200, 255);

            // ボスを生成し、そのインスタンスを変数「spawnedBoss」に格納
            int npcIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)player.Center.X, (int)player.Center.Y - 400, npcType);
            NPC spawnedBoss = Main.npc[npcIndex];

            // --- 強化処理 ---
            // 1. サイズを2倍にする
            float scaleMultiplier = 2.0f;
            spawnedBoss.scale = scaleMultiplier;
            spawnedBoss.width = (int)(spawnedBoss.width * scaleMultiplier);
            spawnedBoss.height = (int)(spawnedBoss.height * scaleMultiplier);

            // 2. HPを2倍にする
            float hpMultiplier = 2.0f;
            spawnedBoss.lifeMax = (int)(spawnedBoss.lifeMax * hpMultiplier);
            spawnedBoss.life = spawnedBoss.lifeMax;

            // 3. ダメージを半分にする
            spawnedBoss.damage = (int)(spawnedBoss.damage * 0.5f);

            // 4. 防御力を+10する
            spawnedBoss.defense += 10;
        }
    }
}