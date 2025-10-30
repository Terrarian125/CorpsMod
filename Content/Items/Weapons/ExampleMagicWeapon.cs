using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleMagicWeapon : ModItem
	{
		public override void SetDefaults() {
			// DefaultToStaff は、魔法の杖武器が使用する様々な Item の値を設定します。
			// Visual Studio で DefaultToStaff にマウスカーソルを合わせると、ドキュメントを読むことができます！
			// オニキスブラスターが発射する投射物としても知られる、黒いボルトを発射します。
			// (投射物ID: BlackBolt, 攻撃速度/使用時間: 7, マナコスト: 20, 弾速: 11)
			Item.DefaultToStaff(ProjectileID.BlackBolt, 7, 20, 11);
			Item.width = 34;
			Item.height = 40;
			Item.UseSound = SoundID.Item71; // 魔法武器らしい、高周波の音

			// ダメージ、ノックバック、ボーナスとなるクリティカルストライク率を設定する特殊なメソッドです。
			// この武器はクリティカル率が 32% であり、プレイヤーのデフォルトのクリティカル率 4% に追加されます。
			Item.SetWeaponValues(25, 6, 32);

			Item.SetShopValues(ItemRarityColor.LightRed4, 10000); // レアリティ: 薄い赤色（ハードモード序盤）、価値: 1ゴールド
		}

		// レシピ作成の詳細については、Content/ExampleRecipes.cs を参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>() // ExampleItem (カスタムアイテム) を必要とする
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // ExampleWorkbench (カスタム作業台) で作成する
				.Register();
		}

		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			// ModifyManaCost を使用して、メテオアーマーセットでのスペースガンと同様に、このアイテムのマナコストを動的に調整できます。
			// 例のフード（ExampleHood）で、アクセサリーがどのようにマナコスト削減効果を与えるかを確認してください。
			if (player.statLife < player.statLifeMax2 / 2) {
				mult *= 0.5f; // 体力が半分以下のとき、マナコストを半分にします。必ず mult パラメータに対して乗算を使用してください。
			}
		}
	}
}