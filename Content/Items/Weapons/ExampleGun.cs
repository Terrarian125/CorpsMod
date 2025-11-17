using CorpsMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleGun : ModItem
	{
		public override void SetDefaults() {
			// ModderはItem.DefaultToRangedWeaponを使用して、useTime、useAnimation、useStyle、autoReuse、DamageType、shoot、shootSpeed、useAmmo、noMeleeなど、多くの一般的なプロパティを素早く設定できます。これらはすべて、教育目的のためにここで個別に示されています。

			// 共通プロパティ (Common Properties)
			Item.width = 62; // アイテムの当たり判定の幅。
			Item.height = 32; // アイテムの当たり判定の高さ。
			Item.scale = 0.75f;
			Item.rare = ItemRarityID.Green; // ゲーム内でのアイテム名の色。

			// 使用プロパティ (Use Properties)
			Item.useTime = 8; // アイテムの使用時間（ティック単位、60ティック == 1秒）。
			Item.useAnimation = 8; // アイテムの使用アニメーションの長さ（ティック単位、60ティック == 1秒）。
			Item.useStyle = ItemUseStyleID.Shoot; // アイテムの使用方法（振る、構えるなど）。
			Item.autoReuse = true; // クリックを押しっぱなしで自動的に再度使用できるかどうか。

			// このアイテムが使用されたときに再生される音。
			Item.UseSound = new SoundStyle($"{nameof(CorpsMod)}/Assets/Sounds/Items/Guns/ExampleGun") {
				Volume = 0.9f,
				PitchVariance = 0.2f,
				MaxInstances = 3,
			};

			// 武器プロパティ (Weapon Properties)
			Item.DamageType = DamageClass.Ranged; // ダメージタイプを遠隔に設定します。
			Item.damage = 20; // アイテムのダメージを設定します。この武器から発射される弾丸は、このダメージと使用された弾薬のダメージを合計して使用することに注意してください。
			Item.knockBack = 5f; // アイテムのノックバックを設定します。この武器から発射される弾丸は、このノックバックと使用された弾薬のノックバックを合計して使用することに注意してください。
			Item.noMelee = true; // アイテムのアニメーションがダメージを与えないようにします。

			// 銃プロパティ (Gun Properties)
			Item.shoot = ProjectileID.PurificationPowder; // どういうわけか、バニラのソースにあるすべての銃にはこれが設定されています。
			Item.shootSpeed = 10f; // 弾丸の速度（フレームあたりのピクセル数で測定）。この値はハンドガンと同等です。
			Item.useAmmo = AmmoID.Bullet; // この武器が使用する弾薬アイテムの「弾薬ID」。弾薬IDは、通常、その弾薬タイプを最も一般的に表す一つのアイテムのアイテムIDに対応するマジックナンバーです。
		}

		// レシピ作成の詳細については、Content/ExampleRecipes.csを参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		// このメソッドを使用すると、プレイヤーの手の中での銃の位置を調整できます。グラフィックと合うようにこれらの値を試してみてください。
		public override Vector2? HoldoutOffset() {
			return new Vector2(2f, -2f);
		}

		// TODO: これをより具体的な名前の例（例えば、ペイントガンなど）に移動してください。
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// この銃から発射されるすべての弾丸は、1/3の確率でExampleInstancedProjectileになります。
			if (Main.rand.NextBool(3)) {
				type = ModContent.ProjectileType<ExampleInstancedProjectile>();
			}
		}

		/*
		* 以下の例のいずれかを自由にアンコメントして、それらが何をするかを確認してください。
		*/

		// 通常の弾丸を高速弾に置き換えるUziのように機能させたい場合はどうすればよいですか？
		// Uzi/Molten Furyスタイル：通常の弾丸を高速弾に置き換える
		/*public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) { // または ProjectileID.WoodenArrowFriendly
				type = ProjectileID.BulletHighVelocity; // または ProjectileID.FireArrow;
			}
		}*/

		// 均等に拡散する複数の弾丸を発射したい場合はどうすればよいですか？ (ヴァンパイアナイフ)
		// 均等な円弧スタイル：複数の弾丸、均等な拡散
		/*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float numberProjectiles = 3 + Main.rand.Next(3); // 3発、4発、または5発
			float rotation = MathHelper.ToRadians(45);

			position += Vector2.Normalize(velocity) * 45f;
			velocity *= 0.2f; // 弾丸の速度を1/5に遅くして、見やすくします。これは、この例が他の例とModItem.SetDefaultsコードを共有しているためだけにあります。独自の武器を作成する場合は、通常どおりItem.shootSpeedを変更してください。

			for (int i = 0; i < numberProjectiles; i++) {
				Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))); // 弾丸が1つしかない場合は、0で割らないように注意してください。
				Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
			}

			return false; // バニラがProjectile.NewProjectileを呼び出すのを止めるためにfalseを返します。
		}*/

		// 銃口から正確にショットが出現するようにするにはどうすればよいですか？
		// また、これを行う場合、タイルを貫通して発射されるのを防ぐにはどうすればよいですか？
		/*public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 muzzleOffset = Vector2.Normalize(velocity) * 25f;

			if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0)) {
				position += muzzleOffset;
			}
		}*/

		// 「クロックワーク・アサルトライフル」のような効果を得るにはどうすればよいですか？
		// 3点バースト、バーストで消費する弾薬は1つだけ。バースト間の遅延にはreuseDelayを使用します。
		// SetDefaults()に以下の変更を加えます。
		/*
			item.useAnimation = 12;
			item.useTime = 4; // useAnimationの3分の1
			item.reuseDelay = 14;
			item.consumeAmmoOnLastShotOnly = true;
		*/

		// 2種類の異なる弾丸を同時に発射するにはどうすればよいですか？
		/*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// ここでは、発射したい弾丸のタイプを手動で指定して、2番目の弾丸を手動でスポーンさせます。
			Projectile.NewProjectile(source, position, velocity, ProjectileID.GrenadeI, damage, knockback, player.whoAmI);

			// trueを返すことで、バニラの動作が実行され、1番目の弾丸（弾薬によって決定されたもの）が発射されます。
			return true;
		}*/

		// いくつかの弾丸の中からランダムに選ぶにはどうすればよいですか？
		/*public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// ここでは、typeをランダムに、元のもの（弾薬によって定義されたもの）、バニラの弾丸、またはmodの弾丸のいずれかに設定します。
			type = Main.rand.Next([type, ProjectileID.GoldenBullet, ModContent.ProjectileType<Projectiles.ExampleBullet>()]);
		}*/
	}
}