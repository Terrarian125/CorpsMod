using CorpsMod.Content.Projectiles;
using CorpsMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleLastPrism : ModItem
	{
		// バニラのテクスチャをアイテムに使用するには、次の形式を使用します: "Terraria/Item_<アイテムID>"。
		public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;
		public static Color OverrideColor = new(122, 173, 255);

		public override void SetDefaults() {
			// まず、CloneDefaultsを使用して、バニラのラストプリズム（Last Prism）の基本的なアイテムプロパティをすべてクローンします。
			// たとえば、これによりスプライトサイズ、使用スタイル、売却価格、そしてアイテムが魔法武器であることなどがコピーされます。
			Item.CloneDefaults(ItemID.LastPrism);
			Item.mana = 4;
			Item.damage = 42;
			Item.shoot = ModContent.ProjectileType<ExampleLastPrismHoldout>();
			Item.shootSpeed = 30f;

			// アイテムの描画色を変更し、バニラのラストプリズムと視覚的に区別できるようにします。
			Item.color = OverrideColor;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}

		// この武器はホールドアウト（持続的な）弾丸を発射するため、その弾丸がすでに存在する場合は使用をブロックする必要があります。
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[ModContent.ProjectileType<ExampleLastPrismHoldout>()] <= 0;
		}
	}
}