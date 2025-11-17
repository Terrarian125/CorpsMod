using CorpsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleShortsword : ModItem
	{
		public override void SetDefaults() {
			Item.damage = 18; // ダメージを増加
			Item.knockBack = 3f; // ノックバックを減少
			Item.useStyle = ItemUseStyleID.Rapier; // プレイヤーに適切な腕の動きをさせる (変更なし)
			Item.useAnimation = 6; // 攻撃アニメーションを高速化 (12 -> 8)
			Item.useTime = 6; // 使用時間を高速化 (12 -> 8)
			Item.width = 40;
			Item.height = 32;
			Item.UseSound = SoundID.Item1;
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.autoReuse = true; // 自動連打を有効化 (false -> true)
			Item.noUseGraphic = true;
			Item.noMelee = true;

			// 💡 防御貫通を追加 (デフォルトの4を10に設定)
			Item.ArmorPenetration = 10; 

			Item.rare = ItemRarityID.Green; // レアリティをGreenに変更
			Item.value = Item.sellPrice(0, 0, 5, 0); // 売却価格を少し上昇

			Item.shoot = ModContent.ProjectileType<ExampleShortswordProjectile>();
			Item.shootSpeed = 2.5f; // 弾丸の速度をわずかに増加 (2.1f -> 2.5f)
		}

		// 🔨 金床でのレシピ変更
		public override void AddRecipes() {
			CreateRecipe()
													   .AddRecipeGroup(RecipeGroupID.IronBar, 2) // 鉄または鉛インゴット 2個
													   .AddRecipeGroup(RecipeGroupID.SilverBar, 2) // 銀またはタングステンインゴット 2個
				.AddTile(TileID.Anvils) // 金床で作る
				.Register();
		}

		// 💡 補足: Defense Penetrationを有効にするには、
		//Item.ArmorPenetration = 10; のコメントアウトを解除してください。
	}
}