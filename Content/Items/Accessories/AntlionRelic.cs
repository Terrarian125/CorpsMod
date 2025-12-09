using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CorpsMod.Common.Players;
using CorpsMod.Content.DamageClasses;
using Terraria.Localization;

namespace CorpsMod.Content.Items.Accessories
{
	public class AntlionRelic : ModItem
	{
		// 1. Stat 値の定義 (アイテムのバランス調整を容易にするため)
		public const int DamageReduction = 10;   // 10%
		public const int PickSpeedBonus = 35;    // 35%
		public const int MeleeSpeedBonus = 5;    // 5%

		public override void SetStaticDefaults() {
			// 外見に影響を与えない設定
			// アクセサリーの視覚的な表示を非表示にする
			//Item.Research.SetCount(1);
			//ItemID.Sets.HiddenAppliesToAllItems[Type] = true;
		}

		//修正: ツールチップに定数値をバインドする
		// Tooltipのローカライズキー（en-US.hjson）の {0} に DamageReduction、 {1} に MeleeSpeedBonus の値が代入されます。
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageReduction, MeleeSpeedBonus);

		public override void SetDefaults() {
			// Itemの基本設定
			Item.width = 24; // 適当なサイズ
			Item.height = 28;
			Item.accessory = true; // アクセサリーとして設定
			Item.value = Item.sellPrice(gold: 5); // 販売価格を設定 (例: 5ゴールド)
			Item.rare = ItemRarityID.Lime; // レアリティを設定 (ハードモード中盤相当)
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// 採掘速度の向上
			// 採掘速度の修正子は、値が低いほど速いことを意味します。
			// 35%の増加は、修正子を1 - 0.35 = 0.65倍にすることに相当します。
			player.pickSpeed -= PickSpeedBonus / 100f;

			// 受けるダメージの軽減 (10%)
			// damage taken multiplier を 1.0 から 0.1 減らします。
			player.endurance += DamageReduction / 100f;

			// 近接攻撃速度の向上 (5%)
			player.GetAttackSpeed(DamageClass.Melee) += MeleeSpeedBonus / 100f;
		}
	}
}