using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	// ロケットランチャーは、通常、弾薬固有のバリアント（派生形）投射物を持つため特殊です。
	// ExampleRocketLauncher は、バニラのロケットランチャー武器によって指定されたバリアントを継承します。
	public class ExampleRocketLauncher : ModItem
	{
		public override void SetStaticDefaults() {
			// この行により、ExampleRocketLauncher は、下の SpecificLauncherAmmoProjectileMatches で
			// 特に設定されていない弾薬に対応するすべてのバリアント投射物に関して、通常のロケットランチャー（ItemID.RocketLauncher）のように振る舞うことができます。
			AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;

			// SpecificLauncherAmmoProjectileMatches は、特定の弾薬アイテムに対して特定の投射物を提供するために使用できます。
			// この例では、RocketIII（ロケット弾III）が使用された場合、この武器はミーアメア（Meowmere）の投射物を発射するように指示しています。
			// これは、この機能を紹介するためだけのものです。通常、"アップグレード" 目的であれば SpecificLauncherAmmoProjectileFallback だけで十分です。
			// 完全にカスタムなロケットランチャーは、代わりにすべての可能なロケット弾薬に対して新しくユニークな投射物を指定することになります。
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.Add(Type, new Dictionary<int, int> {
				{ ItemID.RocketIII, ProjectileID.Meowmere },
			});

			// Note that some rocket launchers, like Celebration and Electrosphere Launcher, will always
			// use their own projectiles no matter which rocket is used as ammo.
			// This type of behavior can be implemented in ModifyShootStats
			// （注意：セレブレーションやエレクトロスフィアランチャーなど、一部のロケットランチャーは、どのロケットが弾薬として使用されても、常に独自の投射物を使用します。
			// このような動作は、ModifyShootStats で実装できます。）
		}

		public override void SetDefaults() {
			// DefaultToRangedWeapon を使用して、ロケットランチャーに必要な基本設定を行います。
			// (投射物ID, 弾薬ID, 単発の攻撃時間: 30, 弾速: 5f, オート連射: true)
			Item.DefaultToRangedWeapon(ProjectileID.RocketI, AmmoID.Rocket, singleShotTime: 30, shotVelocity: 5f, hasAutoReuse: true);
			Item.width = 50;
			Item.height = 20;
			Item.damage = 55;
			Item.knockBack = 4f;
			Item.UseSound = SoundID.Item11; // 爆発音
			Item.value = Item.buyPrice(gold: 40);
			Item.rare = ItemRarityID.Yellow; // レアリティ: 黄色（ハードモード後期のレアリティ）
		}

		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 2f); // プレイヤーの手の中での武器の位置を調整します。
		}
	}
}