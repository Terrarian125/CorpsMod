using CorpsMod.Content.Projectiles;
using CorpsMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	// ExampleStaff は典型的な杖です。杖や他の射撃武器は非常によく似ていますが、この例は主に杖を他のアイテムから際立たせるものを示すために役立ちます。
	// 杖のスプライトは、慣例により右斜め上を向くように傾けられています。「Item.staff[Type] = true;」は、杖を正しく描画するために不可欠です。
	// 杖はマナを使用し、弾薬の代わりに特定の投射物を発射します。Item.DefaultToStaff がこれらを処理します。
	public class ExampleStaff : ModItem
	{
		public override void SetStaticDefaults() {
			Item.staff[Type] = true; // これにより、使用スタイルが銃としてではなく、杖としてアニメーションするようになります。
		}

		public override void SetDefaults() {
			// DefaultToStaff は、魔法の杖武器が使用する様々な Item の値を設定します。
			// Visual Studio で DefaultToStaff にマウスカーソルを合わせると、ドキュメントを読むことができます！
			// (投射物ID, 攻撃速度/使用時間, マナコスト, 弾速) を設定
			Item.DefaultToStaff(ModContent.ProjectileType<SparklingBall>(), 16, 25, 12);

			// UseSound をカスタマイズします。DefaultToStaff は UseSound を SoundID.Item43 に設定しますが、ここでは SoundID.Item20 にしたい。
			Item.UseSound = SoundID.Item20;

			// ダメージとノックバックを設定
			Item.SetWeaponValues(20, 5);

			// レアリティと価値を設定
			Item.SetShopValues(ItemRarityColor.Green2, 10000); // レアリティ: グリーン2 (緑色)、価値: 1ゴールド
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>() // ExampleItem (カスタムアイテム) を1個必要とする
				.AddTile<ExampleWorkbench>() // ExampleWorkbench (カスタム作業台) で作成する
				.Register();
		}
	}
}