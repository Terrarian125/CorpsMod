using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CorpsMod.Content.Items.Weapons
{
	public class SawCleaver : ModItem
	{
		private bool transformed = false; // false=ノコギリ, true=ナタ

		public override void SetDefaults() {
			SetForm(false); // 初期形態：ノコギリ
		}

		private void SetForm(bool nata) {
			transformed = nata;

			if (nata) {
				// --- ナタ形態 ---
				Item.damage = 70;
				Item.DamageType = DamageClass.Melee;
				Item.width = 70;
				Item.height = 70;
				Item.useTime = 28;
				Item.useAnimation = 28;
				Item.knockBack = 6f;
				Item.useStyle = ItemUseStyleID.Swing;
				Item.value = Item.buyPrice(gold: 1);
				Item.rare = ItemRarityID.Green;
				Item.UseSound = SoundID.Item1;
				Item.autoReuse = true;
				Item.crit = 4;

				// 攻撃範囲（リーチ）拡大
				Item.scale = 2.0f;
			}
			else {
				// --- ノコギリ形態 ---
				Item.damage = 50;
				Item.DamageType = DamageClass.Melee;
				Item.width = 40;
				Item.height = 40;
				Item.useTime = 12;
				Item.useAnimation = 12;
				Item.knockBack = 3f;
				Item.useStyle = ItemUseStyleID.Swing;
				Item.value = Item.buyPrice(silver: 50);
				Item.rare = ItemRarityID.Blue;
				Item.UseSound = SoundID.Item71;
				Item.autoReuse = true;
				Item.crit = 8;

				// 攻撃範囲（リーチ）縮小
				Item.scale = 1.0f;
			}
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				transformed = !transformed;
				SetForm(transformed);

				// --- 変形エフェクト ---
				SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.3f }, player.position);
				//SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.7f }, player.position);

				for (int i = 0; i < 25; i++) {
					Dust.NewDust(player.position, player.width, player.height, DustID.Electric,
						Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 150, default, 1.5f);
					Dust.NewDust(player.position, player.width, player.height, DustID.Smoke,
						Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
				}

				CombatText.NewText(player.getRect(), Color.Crimson,
					transformed ? "変形：ナタ形態" : "変形：ノコギリ形態");

				return false;
			}
			return base.CanUseItem(player);
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!transformed)
				target.AddBuff(BuffID.Bleeding, 300); // ノコギリ形態のみ出血
		}

		// === 見た目を切り替える部分 ===
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {

			string path = transformed
				? "CorpsMod/Content/Items/Weapons/SawCleaver_Nata"
				: "CorpsMod/Content/Items/Weapons/SawCleaver_Saw";

			Texture2D tex = ModContent.Request<Texture2D>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			if (tex == null)
				return true;

			spriteBatch.Draw(tex, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
			ref float rotation, ref float scale, int whoAmI) {

			string path = transformed
				? "CorpsMod/Content/Items/Weapons/SawCleaver_Nata"
				: "CorpsMod/Content/Items/Weapons/SawCleaver_Saw";

			Texture2D tex = ModContent.Request<Texture2D>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			if (tex == null)
				return true;

			Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height / 2);
			spriteBatch.Draw(tex, drawPos, null, lightColor, rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
