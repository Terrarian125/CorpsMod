using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace CorpsMod.Content.NPCs.TestBoss
{
	[AutoloadBossHead]
	public class TestBoss : ModNPC
	{
		//距離のしきい値
		public const float DashDistance = 300f;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4; //フレーム数
			NPCID.Sets.BossBestiaryPriority.Add(Type);
		}

		public override void SetDefaults() {
			NPC.width = 100;
			NPC.height = 100;
			NPC.damage = 80;
			NPC.defense = 50;
			NPC.lifeMax = 10000;
			NPC.HitSound = SoundID.NPCHit4; //金属音
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.knockBackResist = 0f; //ノックバックしない
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.value = Item.buyPrice(gold: 20);
			NPC.SpawnWithHigherTime(30);
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1; //AI
		}

		public override void AI() {
			//ターゲット取得
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest();
			}

			Player player = Main.player[NPC.target];
			float distance = Vector2.Distance(NPC.Center, player.Center);


			//離れすぎでワープ

			if (distance > 1000f || NPC.position.Y > player.position.Y + 800f) {
				TeleportAbovePlayer(player);
			}


			//🔴 無敵判定
			if (distance > DashDistance) {
				NPC.dontTakeDamage = true;
				NPC.localAI[0] = 1f;
			}
			else {
				NPC.dontTakeDamage = false;
				NPC.localAI[0] = 0f;
			}

			//横移動
			NPC.velocity.X *= 0.93f;

			if (NPC.Center.X < player.Center.X) {
				NPC.velocity.X += 0.2f;
			}
			else {
				NPC.velocity.X -= 0.2f;
			}

			if (NPC.velocity.X > 3f)
				NPC.velocity.X = 3f;
			if (NPC.velocity.X < -3f)
				NPC.velocity.X = -3f;

			//ジャンプタイマー
			NPC.ai[0]++;

			//詰まり判定
			if (Math.Abs(NPC.velocity.X) < 0.1f && NPC.collideY) {
				NPC.localAI[1]++;
			}
			else {
				NPC.localAI[1] = 0;
			}

			//定期ジャンプ（2秒ごと）
			if (NPC.ai[0] > 120 && NPC.collideY) {
				float heightDiff = player.Center.Y - NPC.Center.Y;

				if (heightDiff < -50f) {
					NPC.velocity.Y = -12f;
				}
				else {
					NPC.velocity.Y = -9f;
				}

				NPC.ai[0] = 0;
			}

			//詰まりすぎ → ワープ（5秒）
			if (NPC.localAI[1] > 300) {
				//5秒詰まり → ワープ
				TeleportAbovePlayer(player);
				NPC.localAI[1] = 0;
			}
			else if (NPC.localAI[1] > 30) {
				//0.5秒詰まり → 大ジャンプ
				NPC.velocity.Y = -14f;
			}

			//段差ジャンプ（補助
			int direction = NPC.direction;

			bool blocked = Collision.SolidCollision(
				NPC.position + new Vector2(direction * 4, 0),
				NPC.width,
				NPC.height
			);

			bool blockedUpper = Collision.SolidCollision(
				NPC.position + new Vector2(direction * 4, -16),
				NPC.width,
				NPC.height
			);

			if (NPC.collideY && (blocked || blockedUpper)) {
				NPC.velocity.Y = -7f;
			}

			//向き
			NPC.spriteDirection = NPC.direction;
		}

		//テレポート用
		private void TeleportAbovePlayer(Player player) {
			Vector2 spawnPos = player.Center + new Vector2(Main.rand.Next(-200, 200), -400);

			NPC.position = spawnPos;
			NPC.velocity = new Vector2(0, 5f);

			for (int i = 0; i < 20; i++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame);
			}

			SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
		}

		//無敵オーラ中の弾丸反射
		public override bool? CanBeHitByProjectile(Projectile projectile) {
			//無敵状態の時だけ反射
			if (NPC.localAI[0] == 1f) {
				if (projectile.friendly && !projectile.hostile) {
					//反射音
					SoundEngine.PlaySound(SoundID.NPCHit4, NPC.Center);

					//弾の向きを反転させて加速
					projectile.velocity *= -1.2f;
					projectile.hostile = true;
					projectile.friendly = false;

					//ダメージを本体に通さない
					return false;
				}
			}
			return null;
		}

		//ディアクロップス風の紫オーラ
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width / 2, (texture.Height / Main.npcFrameCount[Type]) / 2);
			Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
			SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			//無敵状態なら紫色の残像オーラを描画
			if (NPC.localAI[0] == 1f) {
				for (int i = 0; i < 16; i++) {
					float radius = 8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 4f; //波打つ
					Vector2 offset = new Vector2(radius, 0)
						.RotatedBy(MathHelper.TwoPi / 16 * i + Main.GlobalTimeWrappedHourly * 4);

					Color auraColor = Color.Purple * 0.7f;

					spriteBatch.Draw(texture, drawPos + offset, NPC.frame, auraColor,
						NPC.rotation, drawOrigin, NPC.scale * 1.05f, effects, 0f);
				}
			}

			//本体を描画
			spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);

			return false; //デフォルトの描画をスキップ
		}

		public override void FindFrame(int frameHeight) {
			//歩行アニメーション（簡易版）
			NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.2f;
			NPC.frameCounter %= Main.npcFrameCount[Type];
			int frame = (int)NPC.frameCounter;
			NPC.frame.Y = frame * frameHeight;
		}
	}
}