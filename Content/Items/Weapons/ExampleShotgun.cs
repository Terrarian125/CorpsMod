using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleShotgun : ModItem
	{
		public override void SetDefaults() {
			// Mod開発者は、Item.DefaultToRangedWeapon を使用して、useTime、useAnimation、useStyle、autoReuse、DamageType、shoot、shootSpeed、useAmmo、noMeleeなど、多くの共通プロパティを素早く設定できます。ここでは、学習目的のためにこれらをすべて個別に示しています。

			// 共通プロパティ
			Item.width = 44; // アイテムの当たり判定の幅。
			Item.height = 18; // アイテムの当たり判定の高さ。
			Item.rare = ItemRarityID.Green; // ゲーム内でのアイテム名の色。

			// 使用プロパティ
			Item.useTime = 55; // アイテムの使用時間（ティック単位、60ティック＝1秒）。
			Item.useAnimation = 55; // アイテムの使用アニメーションの長さ（ティック単位、60ティック＝1秒）。
			Item.useStyle = ItemUseStyleID.Shoot; // アイテムの使用方法（スイング、構えなど）。
			Item.autoReuse = true; // クリックを押し続けたときに自動的に再使用するかどうか。
			Item.UseSound = SoundID.Item36; // 使用時に再生される音（ショットガンの音）。

			// 武器プロパティ
			Item.DamageType = DamageClass.Ranged; // ダメージタイプを遠距離に設定。
			Item.damage = 10; // アイテムのダメージ。この武器が発射する投射物は、これと使用された弾薬のダメージが合計されます。
			Item.knockBack = 6f; // アイテムのノックバック。この武器が発射する投射物は、これと使用された弾薬のノックバックが合計されます。
			Item.noMelee = true; // アイテムのアニメーションがダメージを与えないようにする。（銃自体での近接ダメージを無効化）

			// 銃プロパティ
			Item.shoot = ProjectileID.PurificationPowder; // なぜか、バニラのソース内のすべての銃はこの投射物IDになっています。（※実際には下のShootフックで上書きされるため、ここでは何でも良いことが多い）
			Item.shootSpeed = 10f; // 投射物の速度（フレームあたりのピクセル数で測定）。
			Item.useAmmo = AmmoID.Bullet; // この武器が使用する弾薬アイテムの「弾薬ID」。弾薬IDは通常、その弾薬タイプを最も一般的に表すアイテムIDに対応するマジックナンバーです。
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			const int NumProjectiles = 8; // この銃が発射する投射物の数。

			for (int i = 0; i < NumProjectiles; i++) {
				// 速度を最大30度（コードでは15度）ランダムに回転させます。（分散表現）
				Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

				// より見栄えを良くするために、速度をランダムに減少させます。（散らばり）
				newVelocity *= 1f - Main.rand.NextFloat(0.3f);

				// 投射物を作成します。
				Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
			}

			return false; // tModLoader に投射物（Item.shootで設定したPurificationPowder）を発射してほしくないので、false を返します。
		}

		// レシピ作成の詳細については、Content/ExampleRecipes.cs を参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>() // ExampleItem (カスタムアイテム) を必要とする
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // ExampleWorkbench (カスタム作業台) で作成する
				.Register();
		}

		// このメソッドを使用すると、プレイヤーの手の中での銃の位置を調整できます。グラフィックと見栄えが良くなるまで、これらの値を調整してください。
		public override Vector2? HoldoutOffset() {
			return new Vector2(-2f, -2f);
		}
	}
}

/*---

### 🚀 主な動作とショットガンとしての特徴

この Mod アイテムは、特に **`Shoot` メソッド**によってショットガンとして機能しています。

1.  **基本設定**:
    ***遠距離武器 * *(`DamageClass.Ranged`) として設定され、**弾薬** (`AmmoID.Bullet`) を使用します。
    * `useTime` と `useAnimation` が **55** と長く設定されており、発射速度が遅い重い武器であることを示しています。

2.  **散弾（ペレット）の発射**:
    * `Shoot` メソッド内で、**8個の投射物** (`NumProjectiles = 8`) を発射するループが実行されます。
    * 各投射物の速度 (`newVelocity`) は、**ランダムに角度が回転**し、**ランダムに速度が減少**させられます。これにより、ショットガン特有の**広範囲に散らばる散弾**が再現されます。
    * `return false;` を使用することで、`Item.shoot` で設定されたデフォルトの投射物（PurificationPowder）が発射されるのをキャンセルし、カスタムの散弾のみを発射させています。

3.  **構え位置の調整**:
    * `HoldoutOffset()` メソッドは、プレイヤーが武器を構えたときに、スプライトが手に持たれる位置を微調整するために使われます。
	*/