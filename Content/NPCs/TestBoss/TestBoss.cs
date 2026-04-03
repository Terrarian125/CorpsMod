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

namespace CorpsMod.Content.NPCs //YourModNameを自分のModの名前空間に変えてください
{
	[AutoloadBossHead]
	public class TestBoss : ModNPC
	{
		//距離のしきい値（約37.5ブロック）
		public const float DashDistance = 600f;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4; //パラディンのアニメーション枚数に合わせて調整
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
			NPC.aiStyle = -1; //完全自作AI
		}

		public override void AI() {
			//ターゲットの確認
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest();
			}

			Player player = Main.player[NPC.target];
			float distance = Vector2.Distance(NPC.Center, player.Center);

			//距離による無敵化判定
			if (distance > DashDistance) {
				NPC.dontTakeDamage = true;
				NPC.localAI[0] = 1f; //無敵フラグON
			}
			else {
				NPC.dontTakeDamage = false;
				NPC.localAI[0] = 0f; //無敵フラグOFF
			}

			//基本的な動き
			NPC.velocity.X *= 0.93f; //摩擦
			if (NPC.Center.X < player.Center.X) {
				NPC.velocity.X += 0.2f;
			}
			else {
				NPC.velocity.X -= 0.2f;
			}

			//速度制限
			if (NPC.velocity.X > 3f)
				NPC.velocity.X = 3f;
			if (NPC.velocity.X < -3f)
				NPC.velocity.X = -3f;

			//向きの調整
			NPC.spriteDirection = NPC.direction;
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
				for (int i = 0; i < 6; i++) {
					Vector2 offset = new Vector2(Main.rand.NextFloat(4f), 0).RotatedBy(MathHelper.TwoPi / 6 * i + Main.GlobalTimeWrappedHourly * 3);
					Color auraColor = Color.Purple * 0.5f;
					spriteBatch.Draw(texture, drawPos + offset, NPC.frame, auraColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
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