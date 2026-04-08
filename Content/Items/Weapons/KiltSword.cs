using CorpsMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	// ExampleCustomSwingSword is an example of a sword with a custom swing using a held projectile
	// This is great if you want to make melee weapons with complex swing behavior
	// A separate Example, ExampleCustomUseStyleWeapon, showcases implementing a custom swing using custom use style code rather than a held projectile.
	// ExampleCustomSwingSword は、保持された発射物を使用してカスタムスイングを行う剣の例です。
	// 複雑なスイング動作を持つ近接武器を作成したい場合に最適です。
	// 別の例である ExampleCustomUseStyleWeapon では、保持された発射物ではなく、カスタム使用スタイルコードを使用してカスタムスイングを実装する方法を紹介しています。
	public class KiltSword : ModItem
	{
		public int attackType = 0; // どの攻撃であるかを追跡します
		public int comboExpireTimer = 0; // 武器が一定時間使用されなかった場合、攻撃パターンをリセットしたい

		public override void SetDefaults() {
			// 共通プロパティ
			Item.width = 46;
			Item.height = 48;
			Item.value = Item.sellPrice(gold: 2, silver: 50);
			Item.rare = ItemRarityID.Green;

			// Use Properties
			// Note that useTime and useAnimation for this item don't actually affect the behavior because the held projectile handles that. 
			// Each attack takes a different amount of time to execute
			// Conforming to the item useTime and useAnimation makes it much harder to design
			// It does, however, affect the item tooltip, so don't leave it out.
			// プロパティを使用
			// このアイテムの useTime と useAnimation は、保持されている発射物が処理するため、実際には動作に影響を与えないことに注意してください。
			// 攻撃ごとに実行時間は異なります。
			// アイテムの useTime と useAnimation に準拠すると、設計が非常に難しくなります。
			// ただし、アイテムのツールチップには影響するため、省略しないでください。
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;

			//// 武器のプロパティ
			Item.knockBack = 7;  // 剣のノックバック。これは発射物のコード内で動的に調整されます。
			Item.autoReuse = true; /// これは武器が自動スイングするかどうかを決定します
			Item.damage = 62; // 剣のダメージ。これは発射物のコード内で動的に調整されます。
			Item.DamageType = DamageClass.Melee; // 近接ダメージを与える
			Item.noMelee = true;  //これにより、アイテムがスイングアニメーションによってダメージを与えないようにします。
			Item.noUseGraphic = true; // これにより、プレイヤーが手を振ったときにアイテムが表示されなくなります

			//発射体のプロパティ
			Item.shoot = ModContent.ProjectileType<ExampleCustomSwingProjectile>(); // The sword as a projectile発射物としての剣
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Using the shoot function, we override the swing projectile to set ai[0] (which attack it is)
			// シュート関数を使用して、スイング発射体をオーバーライドし、ai[0]（どの攻撃か）を設定します。
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
			attackType = (attackType + 1) % 2; // Increment attackType to make sure next swing is different// 次の攻撃が異なることを保証するために attackType をインクリメントします
			comboExpireTimer = 0; // Every time the weapon is used, we reset this so the combo does not expire// 武器が使用されるたびにこれをリセットしてコンボが期限切れにならないようにします
			return false; // return false to prevent original projectile from being shot// 元の発射物が発射されないように false を返す
		}

		public override void UpdateInventory(Player player) {
			if (comboExpireTimer++ >= 120) // after 120 ticks (== 2 seconds) in inventory, reset the attack pattern// インベントリで120ティック（== 2秒）経過後、攻撃パターンをリセットします
				attackType = 0;
		}

		public override bool MeleePrefix() {
			return true; // return true to allow weapon to have melee prefixes (e.g. Legendary)// true を返すと、武器に近接接頭辞（例: Legendary）を付けることができます。
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}