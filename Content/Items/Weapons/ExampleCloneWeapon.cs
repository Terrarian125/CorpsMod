using CorpsMod.Content.Projectiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	/// <summary>
	/// This weapon and its corresponding projectile showcase the CloneDefaults() method, which allows for cloning of other items.
	/// この武器とそれに対応するプロジェクタイルは、他のアイテムのクローン作成を可能にする CloneDefaults() メソッドを紹介しています。
	/// For this example, we shall copy the Meowmere and its projectiles with the CloneDefaults() method, while also changing them slightly.
	/// この例では、CloneDefaults() メソッドを使用して Meowmere とそのプロジェクタイルをコピーし、同時にそれらをわずかに変更します。
	/// For a more detailed description of each item field used here, check out <see cref="ExampleSword" />.
	/// ここで使用されている各アイテムフィールドの詳細については、<see cref="ExampleSword" /> を確認してください。
	/// </summary>
	public class ExampleCloneWeapon : ModItem
	{
		public override void SetDefaults() {
			// This method right here is the backbone of what we're doing here; by using this method, we copy all of
			// このメソッドこそがここで行っていることの根幹です。このメソッドを使用することで、
			// the meowmere's SetDefault stats (such as Item.melee and Item.shoot) on to our item, so we don't have to
			// Meowmere の SetDefault ステータス（Item.melee や Item.shoot など）をすべて私たちのアイテムにコピーするため、
			// go into the source and copy the stats ourselves. It saves a lot of time and looks much cleaner; if you're
			// ソースに入って自分でステータスをコピーする必要がなくなります。これにより、多くの時間が節約され、コードがはるかにきれいになります。
			// going to copy the stats of an item, use CloneDefaults().
			// アイテムのステータスをコピーする場合は、CloneDefaults() を使用してください。

			Item.CloneDefaults(ItemID.Meowmere);

			// After CloneDefaults has been called, we can now modify the stats to our wishes, or keep them as they are.
			// CloneDefaults が呼び出された後、私たちはステータスを希望通りに変更したり、そのまま維持したりできます。
			// For the sake of example, let's swap the vanilla Meowmere projectile shot from our item for our own projectile by changing Item.shoot:
			// 例として、Item.shoot を変更して、私たちのアイテムから発射されるバニラの Meowmere プロジェクタイルを独自のプロジェクタイルに交換してみましょう。

			Item.shoot = ModContent.ProjectileType<ExampleCloneProjectile>(); // Remember that we must use ProjectileType<>() since it is a modded projectile!
																			  // Mod のプロジェクタイルであるため、ProjectileType<>() を使用する必要があることを忘れないでください！
																			  // Check out ExampleCloneProjectile to see how this projectile is different from the Vanilla Meowmere projectile.
																			  // このプロジェクタイルがバニラの Meowmere プロジェクタイルとどのように異なるかを確認するには、ExampleCloneProjectile をチェックしてください。

			// While we're at it, let's make our weapon's stats a bit stronger than the Meowmere, which can be done
			// ついでに、この武器のステータスを Meowmere よりも少し強くしてみましょう。これは、
			// by using math on each given stat.
			// 各ステータスに計算を使用することで可能です。

			Item.damage *= 2; // Makes this weapon's damage double the Meowmere's damage.
							  // この武器のダメージを Meowmere のダメージの2倍にします。
			Item.shootSpeed *= 1.25f; // Makes this weapon's projectiles shoot 25% faster than the Meowmere's projectiles.
									  // この武器のプロジェクタイルを Meowmere のプロジェクタイルよりも25%速く発射するようにします。
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		// レシピ作成の詳細については、Content/ExampleRecipes.cs を参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}