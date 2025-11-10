using CorpsMod.Content.Projectiles; // CorpsMod.Content.Projectiles を使用
using Microsoft.Xna.Framework; // Microsoft.Xna.Framework を使用
using Terraria; // Terraria を使用
using Terraria.DataStructures; // Terraria.DataStructures を使用
using Terraria.ID; // Terraria.ID を使用
using Terraria.ModLoader; // Terraria.ModLoader を使用

namespace CorpsMod.Content.Items.Tools
{
	// ExampleFishingRod は釣り竿アイテムの例です。
	// SetDefaults内のコードと、ModifyFishingLineでlineOriginOffsetを設定するコードは、一般的な動作する釣り竿アイテムに必要なすべてです。
	// それ以外のコードはすべて、複数の浮き（bobbers）、カスタムラインの色、溶岩での釣りといった追加の機能を示しています。
	public class ExampleFishingRod : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.CanFishInLava[Item.type] = true; // この釣り竿で溶岩での釣りを許可します
		}

		public override void SetDefaults() {
			// これらはCloneDefaultsメソッドによってコピーされます:
			// Item.width = 24;
			// Item.height = 28;
			// Item.useStyle = ItemUseStyleID.Swing;
			// Item.useAnimation = 8;
			// Item.useTime = 8;
			// Item.UseSound = SoundID.Item1;
			Item.CloneDefaults(ItemID.WoodFishingPole); // 木の釣り竿のデフォルト値をコピー

			Item.fishingPole = 30; // 釣り竿の釣りパワーを設定します
			Item.shootSpeed = 12f; // 浮きが射出される速度を設定します。木の釣り竿は9f、金の釣り竿は17fです。
			Item.shoot = ModContent.ProjectileType<Projectiles.ExampleBobber>(); // 浮きの投射物。注：釣り浮きアクセサリーが存在する場合、これによって上書きされるため、スポーンした浮きが指定された投射物であると想定しないでください。https://terraria.wiki.gg/wiki/Fishing_Bobbers
		}

		// アイテムを持っている場合、High Test Fishing Line（丈夫な釣り糸）のブール値を与えます。
		// 注：インベントリの外で手でアイテムを持っている場合ではなく、ホットバーにある場合のみトリガーされます。
		public override void HoldItem(Player player) {
			player.accFishingLine = true;
		}

		// 複数の浮きを発射するために、デフォルトの射出メソッドをオーバーライドします。
		// 注：これにより、インベントリに複数のトリュフワームがある場合、釣り竿が複数のデュークフィッシュロンを召喚できるようになります。
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int bobberAmount = Main.rand.Next(3, 6); // 3～5個の浮き
			float spreadAmount = 75f; // 異なる浮きがどれだけ拡散するか。

			for (int index = 0; index < bobberAmount; ++index) {
				Vector2 bobberSpeed = velocity + new Vector2(Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f, Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f);

				// 新しい浮きを生成
				Projectile.NewProjectile(source, position, bobberSpeed, type, 0, 0f, player.whoAmI);
			}
			return false; // デフォルトの射出を防ぎ、カスタムのマルチショットを使用します
		}

		public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor) {
			// 釣り糸が描画され始める原点（Origin）を変更するために、この2つの値を変更します。
			// これにより、プレイヤーが右を向いていて通常の重力下にある場合、プレイヤーの中心から右に43ピクセル、上に30ピクセル移動した位置から描画されます。
			lineOriginOffset = new Vector2(43, -30);

			// 釣り糸の色を設定します。注：これは色付きのストリングアクセサリーによって上書きされます。
			if (bobber.ModProjectile is ExampleBobber exampleBobber) {
				// ExampleBobberには、ラインの色を決定するためのカスタムコードがあります。
				lineColor = exampleBobber.FishingLineColor;
			}
			else {
				// 浮きがExampleBobberでない場合、釣り浮きアクセサリーが有効になっており、代わりにDiscoColor（ディスコカラー）を使用します。
				lineColor = Main.DiscoColor;
			}
		}

		// レシピ作成の詳細な説明については、Content/ExampleRecipes.csを参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}