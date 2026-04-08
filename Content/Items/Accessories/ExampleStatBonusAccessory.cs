using CorpsMod.Common.Players;
using CorpsMod.Content.DamageClasses;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Accessories
{
	public class ExampleStatBonusAccessory : ModItem
	{
		// これらの値をここで宣言することで、値を変更すると効果だけでなく、ツールチップ（ヒント）も変更されます。
		public static readonly int AdditiveDamageBonus = 25;
		public static readonly int MultiplicativeDamageBonus = 12;
		public static readonly int BaseDamageBonus = 4;
		public static readonly int FlatDamageBonus = 5;
		public static readonly int MeleeCritBonus = 10;
		public static readonly int RangedAttackSpeedBonus = 15;
		public static readonly int MagicArmorPenetration = 5;
		public static readonly int ExampleKnockback = 100;
		public static readonly int AdditiveCritDamageBonus = 20;

		// ツールチップのローカライゼーションに修飾子値を挿入します。この方法の詳細については、Wikiを参照してください: https://github.com/tModLoader/tModLoader/wiki/Localization#binding-values-to-localizations
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AdditiveDamageBonus, MultiplicativeDamageBonus, BaseDamageBonus, FlatDamageBonus, MeleeCritBonus, RangedAttackSpeedBonus, MagicArmorPenetration, ExampleKnockback, AdditiveCritDamageBonus);

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// GetDamageは、指定されたダメージクラスのダメージ StatModifier（ステータス修飾子）への参照を返します。
			// 値ではなく参照を返すため、数学演算子（+, -, *, /, など）で自由に修正できます。
			// StatModifierは、加算（additive）および乗算（multiplicative）の修飾子、ベースダメージ、およびフラットダメージを個別に保持する構造体です。
			// StatModifierが値に適用されるとき、加算修飾子が乗算修飾子よりも前に適用されます。
			// ベースダメージは武器の基礎ダメージに直接追加され、他のダメージボーナスの影響を受けます。一方、フラットダメージは他のすべての計算の後に適用されます。
			// このケースでは、いくつかのことを行っています:
			// - ダメージを25%（加算的に）追加しています。これはアクセサリーが使用する一般的な「X%のダメージ増加」です。これを使用してください。
			// - ダメージを12%（乗算的に）追加しています。この効果はTerrariaではほとんど使用されません。通常は上記の加算乗算子を使用してください。乗算ボーナスでゲームのバランスを正しく取ることは非常に困難です。
			// - ベースダメージに4を追加しています。
			// - フラットダメージに5を追加しています。
			// DamageClass.Genericを使用しているため、これらのボーナスはプレイヤーが与えるすべてのダメージに適用されます。
			player.GetDamage(DamageClass.Generic) += AdditiveDamageBonus / 100f;
			player.GetDamage(DamageClass.Generic) *= 1 + MultiplicativeDamageBonus / 100f;
			player.GetDamage(DamageClass.Generic).Base += BaseDamageBonus;
			player.GetDamage(DamageClass.Generic).Flat += FlatDamageBonus;

			// GetCritは、GetDamageと同様に、指定されたダメージクラスのクリティカル率への参照を返します。
			// このケースでは、クリティカル率を10%追加していますが、近接（Melee）のDamageClassに限定されます（したがって、近接武器のみがこのボーナスを受け取ります）。
			// 注: すべてのクリティカル計算が完了した後、武器またはクラスの合計クリティカル率は通常 int にキャストされます。それに従って計画してください。
			player.GetCritChance(DamageClass.Melee) += MeleeCritBonus;

			// GetAttackSpeedは、GetDamageやGetKnockbackと機能的に同じです。攻撃速度用です。
			// このケースでは、遠隔武器（Ranged）の全体的な使用速度を15%速くします。
			// 注: これらの計算の結果としてゼロまたは負の値になった場合、例外がスローされます。それに従って計画してください。
			player.GetAttackSpeed(DamageClass.Ranged) += RangedAttackSpeedBonus / 100f;

			// GetArmorPenetrationは、GetCritChanceと機能的に同じですが、代わりにアーマー貫通ステータス用です。
			// このケースでは、魔法武器（Magic）にアーマー貫通を5追加します。
			// 注: すべてのアーマー貫通計算が完了した後、最終的なアーマー貫通量は int にキャストされます。それに従って計画してください。
			player.GetArmorPenetration(DamageClass.Magic) += MagicArmorPenetration;

			// GetKnockbackは、GetDamageと機能的に同じですが、代わりにノックバックステータス用です。
			// このケースでは、カスタムのExample DamageClassに限定してノックバックを100%加算的に追加します（したがって、Exampleクラスの武器のみがこのボーナスを受け取ります）。
			player.GetKnockback<ClassLess>() += ExampleKnockback / 100f;

			player.GetModPlayer<ExampleDamageModificationPlayer>().AdditiveCritDamageBonus += AdditiveCritDamageBonus / 100f;
			// 一部の効果は、この下のExampleStatBonusAccessoryPlayerで適用されます。
			player.GetModPlayer<ExampleStatBonusAccessoryPlayer>().exampleStatBonusAccessory = true;
		}
	}

	// 一部の移動効果は、その計算方法により、ModItem.UpdateAccessoryで修正するのに適していません。
	// ModPlayer.PostUpdateRunSpeeds は、これらの修正に適しています。
	public class ExampleStatBonusAccessoryPlayer : ModPlayer
	{
		public bool exampleStatBonusAccessory = false;

		public override void ResetEffects() {
			exampleStatBonusAccessory = false;
		}

		public override void PostUpdateRunSpeeds() {
			// ExampleStatBonusAccessoryが装備されており、マウントに乗っていない場合にのみ、追加の変更を適用します。
			if (Player.mount.Active || !exampleStatBonusAccessory) {
				return;
			}

			// 以下は、シャドウアーマーのセットボーナスに類似した修正です。
			Player.runAcceleration *= 1.75f; // プレイヤーの走行加速度を修正
			Player.maxRunSpeed *= 1.15f; // 最大走行速度を修正
			Player.accRunSpeed *= 1.15f; // アクセサリーによる走行速度ボーナスを修正
			Player.runSlowdown *= 1.75f; // プレイヤーの減速率を修正
		}
	}
}