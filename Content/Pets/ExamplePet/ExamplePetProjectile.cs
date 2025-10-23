using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Pets.ExamplePet
{
	public class ExamplePetProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
			Main.projPet[Projectile.type] = true;

			// This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
			// * It uses fluent API syntax, just like Recipe
			// * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
			// * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
			// * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
			// * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
			// このコードは、プレイヤー選択画面のペットの外観をカスタマイズするために必要です。簡単な説明：
			// * Recipe と同様に、Fluent API 構文を使用します。
			// * まず ProjectileID.Sets.SimpleLoop を使用し、開始フレームと終了フレーム、速度を指定します。また、オプションで、端に到達した後に端からアニメーションを開始して実質的に「跳ねる」ようにするかどうかも指定します。
			// * プレイヤーがハイライトされていない、または立っている場合にアニメーションを停止するには（ほとんどの地上ペットと同様に）、.WhenNotSelected(0, 0) を追加します（SimpleLoop と同様にカスタマイズできます）。
			// * オフセットと方向を設定するには、.WithOffset(x, y) と .WithSpriteDirection(-1) を使用します。
			// * ペットの挙動とアニメーションをさらにカスタマイズするには（ペットの AI は実行されないため）、.WithCode() を介して DelegateMethods.CharacterPreview のいくつかのバニラプリセットにアクセスできます。 MinionBossPetProjectileで紹介されているように、自分で作ることもできます。
			ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
				.WithOffset(-10, -20f)
				.WithSpriteDirection(-1)
				.WithCode(DelegateMethods.CharacterPreview.Float);
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ZephyrFish); // Copy the stats of the Zephyr Fish// ゼファーフィッシュの統計情報をコピーする

			AIType = ProjectileID.ZephyrFish; // Mimic as the Zephyr Fish during AI.// AI 中は Zephyr Fish として模倣します。
		}

		public override bool PreAI() {
			Player player = Main.player[Projectile.owner];

			player.zephyrfish = false; // Relic from AIType// AIType からの遺物

			return true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			// Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
			// プレイヤーが死亡しておらず、ペットのバフを持っている限り、発射物が消えないようにします。
			if (!player.dead && player.HasBuff(ModContent.BuffType<ExamplePetBuff>())) {
				Projectile.timeLeft = 2;
			}
		}
	}
}
