using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleJavelin : ModItem
	{
		public override void SetDefaults() {
			// これらの値は自由に変更できますが、useStyleは1、noUseGraphicとnoMeleeのbool値は保持すべきでしょう。

			// 共通プロパティ (Common Properties)
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(silver: 5);
			Item.maxStack = 999;

			// 使用プロパティ (Use Properties)
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 25;
			Item.useTime = 25;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.consumable = true;

			// 武器プロパティ (Weapon Properties)			
			Item.damage = 33;
			Item.knockBack = 5f;
			Item.noUseGraphic = true; // 使用時にアイテムを非表示にする
			Item.noMelee = true; // アイテムではなく、発射される弾丸がダメージを与えるようにする
			Item.DamageType = DamageClass.Ranged;

			// 弾丸プロパティ (Projectile Properties)
			Item.shootSpeed = 12f;
			Item.shoot = ModContent.ProjectileType<Projectiles.ExampleJavelinProjectile>(); // 投げられる弾丸

		}

		// レシピ作成の詳細については、Content/ExampleRecipes.csを参照してください。
		public override void AddRecipes() {
			CreateRecipe(20)
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}