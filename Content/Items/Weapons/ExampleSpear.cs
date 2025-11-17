using CorpsMod.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	public class ExampleSpear : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // これにより、使用アニメーションに紐づくサウンド再生がスキップされます。これにより、UseItem()フック内で使用時間に合わせてサウンドを再生できるようになります。
			ItemID.Sets.Spears[Item.type] = true; // これにより、ゲームが私たちの新しいアイテムを槍として認識できるようになります。
		}

		public override void SetDefaults() {
			// 共通プロパティ (Common Properties)
			Item.rare = ItemRarityID.Pink; // このアイテムにピンクのレアリティレベルを割り当てます
			Item.value = Item.sellPrice(silver: 10); // NPCに売却できるコインの種類と枚数

			// 使用プロパティ (Use Properties)
			Item.useStyle = ItemUseStyleID.Shoot; // アイテムの使用方法（振る、構えるなど）
			Item.useAnimation = 12; // アイテムの使用アニメーションの長さ（ティック単位、60ティック == 1秒）
			Item.useTime = 18; // アイテムの使用時間（ティック単位、60ティック == 1秒）
			Item.UseSound = SoundID.Item71; // このアイテムが使用されたときに再生される音。
			Item.autoReuse = true; // プレイヤーがクリックを押しっぱなしでアイテムを自動的に再使用できるようにします。ほとんどの槍はautoReuseしませんが、CanUseItem()と組み合わせて使用​​すると可能です。

			// 武器プロパティ (Weapon Properties)
			Item.damage = 25;
			Item.knockBack = 6.5f;
			Item.noUseGraphic = true; // trueの場合、アイテム使用中にアイテムの画像が表示されなくなります。槍の弾丸が表示されるため、槍の画像も表示したくないため、trueに設定します。
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true; // アイテムのアニメーションがダメージを与えないようにします。槍は実際にはアイテムではなく弾丸であるため、これは重要です。これにより、このアイテムの近接攻撃の当たり判定が防止されます。

			// 弾丸プロパティ (Projectile Properties)
			Item.shootSpeed = 3.7f; // 弾丸の速度（フレームあたりのピクセル数で測定）。
			Item.shoot = ModContent.ProjectileType<ExampleSpearProjectile>(); // この武器から発射される弾丸
		}

		public override bool CanUseItem(Player player) {
			// 自動連射を使用する場合、投げられる槍が1本以下であることを保証します。
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override bool? UseItem(Player player) {
			// 使用アニメーションの開始時のサウンド再生をスキップしているため、アイテムが実際に使用されるたびに、自分でサウンドを再生する必要があります。
			if (!Main.dedServ && Item.UseSound.HasValue) {
				SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
			}

			return null;
		}

		// レシピ作成の詳細については、Content/ExampleRecipes.csを参照してください。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}