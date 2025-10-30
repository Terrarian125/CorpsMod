using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace CorpsMod.Content.Items.Weapons
{
	public class SawCleaver : ModItem
	{
		private bool transformed = false; // false=ノコギリ, true=ナタ

		//public override void SetStaticDefaults() {
		//	//DisplayName.SetDefault("ノコギリ鉈");
		//	//Tooltip.SetDefault("右クリックで変形する血塗られた武器");
		//}

		public override void SetDefaults() {
			SetForm(false); // 初期はノコギリ形態
		}

		private void SetForm(bool nata) {
			if (nata) {
				// --- ナタ形態 ---
				transformed = true;
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
			}
			else {
				// --- ノコギリ形態 ---
				transformed = false;
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
				Item.UseSound = SoundID.Item71; // シャキン！と鋭い音
				Item.autoReuse = true;
				Item.crit = 8;
			}
		}

		public override bool AltFunctionUse(Player player) {
			// 右クリックを許可
			return true;
		}

		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				// 右クリックで変形！
				transformed = !transformed;
				SetForm(transformed);

				// --- 変形エフェクト ---
				SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.3f }, player.position); // 金属音
				SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.7f }, player.position); // 雷音

				for (int i = 0; i < 25; i++) {
					Dust.NewDust(player.position, player.width, player.height, DustID.Electric, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 150, default, 1.5f);
					Dust.NewDust(player.position, player.width, player.height, DustID.Smoke, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
				}

				for (int i = 0; i < 3; i++) {
					Vector2 lightningPos = player.Center + new Vector2(Main.rand.Next(-20, 20), -50);
					Dust.NewDust(lightningPos, 10, 10, DustID.GoldFlame, 0, 5f);
				}

				CombatText.NewText(player.getRect(), Color.Crimson, transformed ? "変形：ナタ形態" : "変形：ノコギリ形態");

				return false; // 右クリック攻撃はしない
			}

			return base.CanUseItem(player);
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!transformed) {
				// ノコギリ形態のみ出血付与（5秒）
				target.AddBuff(BuffID.Bleeding, 300);
				CombatText.NewText(target.getRect(), Color.DarkRed, "出血！");
			}
		}

	public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
