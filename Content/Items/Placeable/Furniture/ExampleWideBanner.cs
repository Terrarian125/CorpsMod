using CorpsMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Placeable.Furniture
{
	public class ExampleWideBanner : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleWideBannerTile>());
			Item.value = Item.buyPrice(copper: 10);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}