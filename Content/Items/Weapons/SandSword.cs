using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CorpsMod.Content.Items.Weapons
{
	/// <summary>
	/// 友好NPCのみに大ダメージを与える、即死級の剣の例。
	/// </summary>
	public class SandSword : ModItem
	{
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing; // 使用スタイルは振り下ろし
			Item.useAnimation = 10;               // 非常に速い攻撃速度
			Item.useTime = 10;                    // 攻撃速度
			Item.damage = 9999;                   // 即死級のダメージを設定 (例: 9999)
			Item.knockBack = 0f;                  // ノックバックなし
			Item.width = 40;
			Item.height = 40;
			Item.scale = 1.2f;                    // 少し大きめに
			Item.UseSound = SoundID.Item15;       // 攻撃的な音 (例: スイング音)
			Item.rare = ItemRarityID.Green;       // レアリティは緑
			Item.value = Item.sellPrice(silver: 1); // 価値は非常に低く設定
			Item.DamageType = DamageClass.Melee;  // ダメージタイプは近接
			Item.autoReuse = true;                // オートスイング

			// 注意: この剣は敵NPCへのダメージを無効化し、友好的なNPCにのみダメージを与えます。
		}

		// 攻撃がNPCにヒットしたときに呼び出されます。
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			// 敵ではない（フレンドリーな）NPCにヒットした場合のみ処理を続行します。
			// TownNPCs (町のNPC) や ModNPC の friendly=true のNPCが対象になります。
			if (target.friendly) {
				// 既に即死級のダメージを設定していますが、念のためトータルダメージを強制的に設定することもできます。
				// 例: target.StrikeNPC(target.lifeMax + 1, 0f, 0); // これで確実に即死
			}
			// 敵NPC (target.friendly が false の場合) にヒットした場合は何もしません。
		}

		// プレイヤーに対するツールチップ（説明文）の追加
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "FriendlyKiller", "町のNPCにのみダメージを与える") {
				OverrideColor = Color.Red // 説明文を赤色にする
			});
			tooltips.Add(new TooltipLine(Mod, "InstaKill", "即死級ダメージ") {
				OverrideColor = Color.Red
			});
		}

		// 敵NPCに対してダメージを与えないようにするフック
		// townNPCs (町のNPC) は friendly=true のため、このチェックは主に敵NPCを無効化するために機能します。
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			// ターゲットが友好的（friendly）ではない場合（つまり敵の場合）、ダメージをゼロにします。
			if (!target.friendly) {
				modifiers.FinalDamage *= 0;
			}
		}

		// レシピを追加します。
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.SandBlock, 5) // 砂ブロック 5個
				.AddTile(TileID.WorkBenches)      // 作業台で作成
				.Register();
		}
	}
}