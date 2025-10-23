using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Common.GlobalNPCs
{
	public class GuideGlobalNPC : GlobalNPC
	{
		public override bool AppliesToEntity(NPC npc, bool lateInstantiation) {
			return npc.type == NPCID.Guide;
		}

		public override void AI(NPC npc) {
			// Make the guide giant and green.
			npc.scale = 1.5f;
			//npc.color = Color.ForestGreen;
		}

		public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) {
			// Flip and move his emote bubble to the front of him.
			spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			position.X += npc.width * npc.spriteDirection;
		}

		public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) {
			// Move where the party hat is on the Guide.
			position.Y -= npc.height / 2f;
			position.X += 4 * npc.spriteDirection;
		}
		public override void SetDefaults(NPC npc) {
			if (npc.type == NPCID.Guide) {
				string[] customNames = { "Big", "BIG", "ジャイアン"};
				npc.GivenName = Main.rand.Next(customNames);
			}
		}
		public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) {
			if (npc.type == NPCID.Guide) {
				projType = ProjectileID.FireArrow; // 通常は木の矢 → 火矢に変更
			}
		}

		public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) {
			if (npc.type == NPCID.Guide) {
				damage = 25; // 攻撃力を上げる
				knockback = 4f;
			}
		}
		public override void OnKill(NPC npc) {
			if (npc.type == NPCID.Guide) {
				Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ItemID.WoodenBow);
			}
		}
		public override void GetChat(NPC npc, ref string chat) {
			if (npc.type == NPCID.Guide) {
				string[] customLines = {
			"Big！！",
			"君も緑色になってみない？",
			"森と共に生きるんだ！"
		};
				chat = customLines[Main.rand.Next(customLines.Length)];
			}
		}

	}
}
