using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace CorpsMod.Content.Items.Weapons
{
	/// <summary>
	/// タウンNPCのみに即死級のダメージを与える剣。
	/// 敵には一切ダメージを与えない。
	/// タウンNPCを斬ると雷エフェクトと音が発生。
	/// </summary>
	public class SandSword : ModItem
	{
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 10;
			Item.useTime = 10;
			Item.damage = 1; // 敵には効かないので低く
			Item.knockBack = 0f;
			Item.width = 40;
			Item.height = 40;
			Item.scale = 1.2f;
			Item.UseSound = SoundID.Item15;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 1);
			Item.DamageType = DamageClass.Melee;
			Item.autoReuse = true;
		}

		// タウンNPCにのみヒットさせる
		public override bool? CanHitNPC(Player player, NPC target) {
			// TownNPCのみにヒットを許可
			return target.townNPC;
		}

		// ダメージ処理
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			if (!target.townNPC)
				return;

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				// 即死級ダメージを与える（新しい構文）
				NPC.HitInfo info = new NPC.HitInfo {
					Damage = target.lifeMax * 10, // 確実に即死
					Knockback = 0f,
					HitDirection = player.direction,
					Crit = false
				};
				target.StrikeNPC(info);
			}

			// 雷エフェクト
			for (int i = 0; i < 40; i++) {
				Vector2 pos = target.Center + new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-60f, 60f));
				Dust.NewDustPerfect(pos, DustID.Electric, Vector2.Zero, 150, Color.Yellow, 1.5f).noGravity = true;
			}

			// 雷鳴
			SoundEngine.PlaySound(SoundID.Thunder, target.Center);

			// チャットメッセージ
			Main.NewText($"{target.FullName} は砂をなめたようだ", 200, 200, 255);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.SandBlock, 5)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
