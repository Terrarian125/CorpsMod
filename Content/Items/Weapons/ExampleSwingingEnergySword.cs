using CorpsMod.Content.Items.Placeable;
using CorpsMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	// これはエクスカリバー（Excalibur）のコピーです
	public class ExampleSwingingEnergySword : ModItem
	{
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing; // 使用スタイルは振り下ろし
			Item.useAnimation = 20;               // 振り終わるまでの時間 (ティック)
			Item.useTime = 20;                    // 攻撃速度 (ティック)
			Item.damage = 72;
			Item.knockBack = 4.5f;
			Item.width = 40;
			Item.height = 40;
			Item.scale = 1f;
			Item.UseSound = SoundID.Item1;        // 使用時の音 (バニラの剣の音)
			Item.rare = ItemRarityID.Pink;        // レアリティはピンク (ハードモード初期のレアリティ)
			Item.value = Item.buyPrice(gold: 23); // 売却価格を設定 (購入価格の5分の1が実際の売却額になる)
			Item.DamageType = DamageClass.Melee;  // ダメージタイプは近接
			Item.shoot = ModContent.ProjectileType<ExampleSwingingEnergySwordProjectile>(); // 発射する投射物
			Item.noMelee = true;                   // これを設定すると、剣自体はダメージを与えません（投射物のみがダメージを与えます）。
			Item.shootsEveryUse = true;            // これにより、振るたびにPlayer.ItemAnimationJustStartedが確実に設定されます。
			Item.autoReuse = true;                 // オートスイング（長押しで連続使用可能）
		}

		// 投射物を発射する際のカスタムロジック
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float adjustedItemScale = player.GetAdjustedItemScale(Item); // プレイヤーとアイテムの近接スケールを取得します。

			// 新しい投射物を生成し、プレイヤーの中心から、プレイヤーの向きに応じた速度で発射します。
			Projectile.NewProjectile(
				source,
				player.MountedCenter,
				new Vector2(player.direction, 0f), // 投射物の速度。プレイヤーの向き (direction) のベクトルを使用
				type,
				damage,
				knockback,
				player.whoAmI,
				player.direction * player.gravDir,
				player.itemAnimationMax,
				adjustedItemScale
			);

			NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI); // マルチプレイヤーで変更を同期します。

			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		// レシピ作成の詳細については、Content/ExampleRecipes.cs を参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleBar>(12) // Example Bar (カスタムインゴット) を12個必要とする
				.AddTile(TileID.MythrilAnvil) // 作成にはミスリルまたはオリハルコンの金床が必要 (両方を含む)
				.Register();
		}
	}
}