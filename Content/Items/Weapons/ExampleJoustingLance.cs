using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleJoustingLance : ModItem
	{
		public override void SetDefaults() {
			// A special method that sets a variety of item parameters that make the item act like a spear weapon.
			// To see everything DefaultToSpear() does, right click the method in Visual Studios and choose "Go To Definition" (or press F12). You can also hover over DefaultToSpear to see the documentation.
			// The shoot speed will affect how far away the projectile spawns from the player's hand.
			// If you are using the custom AI in your projectile (and not aiStyle 19 and AIType = ProjectileID.JoustingLance), the standard value is 1f.
			// If you are using aiStyle 19 and AIType = ProjectileID.JoustingLance, then multiply the value by about 3.5f.
			// アイテムを槍武器のように動作させるための様々なパラメータを設定する特別なメソッドです。
			// DefaultToSpear() の機能すべてを確認するには、Visual Studio でメソッドを右クリックし、「定義へジャンプ」を選択するか、F12 キーを押します。また、DefaultToSpear にマウスオーバーしてドキュメントを表示することもできます。
			// 発射速度は、プレイヤーの手から発射物がどのくらい離れた場所に出現するかに影響します。
			// 発射物にカスタム AI を使用している場合（aiStyle 19 かつ AIType = ProjectileID.JoustingLance ではない場合）、標準値は 1f です。
			// aiStyle 19 かつ AIType = ProjectileID.JoustingLance を使用している場合は、この値に約 3.5f を掛けます。
			Item.DefaultToSpear(ModContent.ProjectileType<Projectiles.ExampleJoustingLanceProjectile>(), 1f, 24);

			Item.DamageType = DamageClass.MeleeNoSpeed; // We need to use MeleeNoSpeed here so that attack speed doesn't effect our held projectile.// 攻撃速度が保持されている発射物に影響を与えないように、ここで MeleeNoSpeed を使用する必要があります。

			Item.SetWeaponValues(56, 12f, 0); // A special method that sets the damage, knockback, and bonus critical strike chance.// ダメージ、ノックバック、ボーナスクリティカルヒットの確率を設定する特別なメソッド。

			Item.SetShopValues(ItemRarityColor.LightRed4, Item.buyPrice(0, 6)); // A special method that sets the rarity and value.// 希少性と価値を設定する特別なメソッド。

			Item.channel = true; // Channel is important for our projectile.// 発射体にとってチャネルは重要です。

			// This will make sure our projectile completely disappears on hurt.
			// It's not enough just to stop the channel, as the lance can still deal damage while being stowed
			// If two players charge at each other, the first one to hit should cancel the other's lance
			// これにより、ダメージを受けた際に発射物が完全に消えるようになります。
			// ランスは収納中でもダメージを与えることができるため、チャネルを止めるだけでは不十分です。
			// 2人のプレイヤーが互いに突撃した場合、先に攻撃を命中させたプレイヤーが相手のランスをキャンセルする必要があります。
			Item.StopAnimationOnHurt = true;
		}

		// これにより、Jousting Lance に近接武器と同じ修飾子を適用できるようになります。
		public override bool MeleePrefix() {
			return true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		// レシピ作成の詳細な説明については、Content/ExampleRecipes.cs を参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(5)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}