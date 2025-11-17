using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleMinigun : ModItem
	{
		public override void SetDefaults() {
			// ModderはItem.DefaultToRangedWeaponを使用して、useTime、useAnimation、useStyle、autoReuse、DamageType、shoot、shootSpeed、useAmmo、noMeleeなど、多くの一般的なプロパティを素早く設定できます。
			// これらのプロパティを説明するコメントについては、ExampleGun.SetDefaultsを参照してください。
			Item.DefaultToRangedWeapon(ProjectileID.PurificationPowder, AmmoID.Bullet, 5, 16f, true);

			// Item.SetWeaponValuesは、ダメージ、ノックバック、クリティカル率を素早く設定できます。
			Item.SetWeaponValues(11, 1f);

			Item.width = 54; // アイテムの当たり判定の幅。
			Item.height = 22; // アイテムの当たり判定の高さ。
			Item.rare = ItemRarityID.Green; // ゲーム内でのアイテム名の色。
			Item.UseSound = SoundID.Item11; // このアイテムが使用されたときに再生される音。
		}

		// レシピ作成の詳細については、Content/ExampleRecipes.csを参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		// 次のメソッドは、この銃に38%の確率で弾薬を消費しないようにします。
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextFloat() >= 0.38f;
		}

		// 次のメソッドは、プレイヤーのインベントリに最低10個のExampleItemがある限り、弾薬がなくてもこの銃が発射できるようにします。
		// その際、この銃のデフォルトの弾薬（この場合はマスケットボール）が使用されているかのように発射されます。
		public override bool NeedsAmmo(Player player) {
			return player.CountItem(ModContent.ItemType<ExampleItem>(), 10) < 10;
		}

		// 次のメソッドは、銃をわずかに不正確にします。
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
		}

		// このメソッドを使用すると、プレイヤーの手の中での銃の位置を調整できます。グラフィックと合うようにこれらの値を試してみてください。
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, -2f);
		}
	}
}