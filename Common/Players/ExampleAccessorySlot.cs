using CorpsMod.Common.Configs; // CorpsMod.Common.Configs を使用
using Microsoft.Xna.Framework; // Microsoft.Xna.Framework を使用
using Terraria; // Terraria を使用
using Terraria.ID; // Terraria.ID を使用
using Terraria.Localization; // Terraria.Localization を使用
using Terraria.ModLoader; // Terraria.ModLoader を使用

namespace CorpsMod.Common.Players
{
	public class CorpsModAccessorySlot1 : ModAccessorySlot
	{
		// クラスが空の場合、すべては基本的なバニラスロットにデフォルト設定されます。
	}

	public class ExampleCustomLocationAndTextureSlot : ModAccessorySlot
	{
		// スロットをマップの中央に配置します。内部のUI処理には従わないという決定です。
		public override Vector2? CustomLocation => new Vector2(Main.screenWidth / 2, 3 * Main.screenHeight / 4);

		// 染料がある場合にバニティスロットを描画します
		public override bool DrawVanitySlot => !DyeItem.IsAir;

		// 'カスタム'テクスチャを使用します
		// 背景テクスチャ -> 一般的に、既存のバニラテクスチャのほとんどを使用して異なる色を得ることができます
		public override string VanityBackgroundTexture => "Terraria/Images/Inventory_Back14"; // 黄色
		public override string FunctionalBackgroundTexture => "Terraria/Images/Inventory_Back7"; // 薄い青
		public override string DyeBackgroundTexture => "Terraria/Images/Inventory_Back13"; // 白。白なので、BackgroundDrawColorで割り当てられた色がそのまま表示される色になります。

		// アイコンテクスチャ。標準的な画像サイズは32x32です。貯金箱（Piggy bank）は16x24ですが、中央に描画されるため機能します。
		public override string VanityTexture => "Terraria/Images/Item_" + ItemID.PiggyBank;

		// 邪魔にならない例として、ほとんどの時間非表示にします
		public override bool IsHidden() {
			return IsEmpty; // アイテムが含まれている場合にのみ表示します。クイックスワップ（アクセサリーの右クリック）により、機能スロットにアイテムが入ることがあります。
		}

		public override void BackgroundDrawColor(AccessorySlotType context, ref Color color) {
			if (context == AccessorySlotType.DyeSlot) {
				color = Main.DiscoColor * (Main.invAlpha / 255);
			}
		}
	}

	public class CorpsModWingSlot : ModAccessorySlot
	{
		public static LocalizedText WingsText { get; private set; }
		public static LocalizedText WingsDyeText { get; private set; }

		// このスロットは、ロードアウトのサポート有無を切り替えることができます。ロードアウトをサポートしないことにより、プレイヤーは切り替えを計画している各ロードアウトに対して1つではなく、単一の翼をクラフトするだけで済みます。
		// この設定はサーバー側である必要があり、変更された場合はリロードが必要です。
		public override bool HasEquipmentLoadoutSupport => ModContent.GetInstance<CorpsModConfig>().WingSlotLoadoutSupportToggle;

		public override void SetupContent() {
			WingsText = Mod.GetLocalization($"{nameof(CorpsModWingSlot)}.Wings");
			WingsDyeText = Mod.GetLocalization($"{nameof(CorpsModWingSlot)}.WingsDye");
		}

		public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) {
			if (checkItem.wingSlot > 0) // 翼（Wing）の場合、スロットに入れられます
				return true;

			return false; // それ以外はスロットには入れられません
		}

		// 翼を入れるための優先スロットとして指定します。注意：他のスロットが翼を持つことを制限したい場合は、ItemLoader.CanEquipAccessory を使用してください！
		public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo) {
			if (item.wingSlot > 0) // 翼の場合、このスロットに入れることを優先したいです。
				return true;

			return false;
		}

		public override bool IsEnabled() {
			if (Player.armor[0].headSlot >= 0) // プレイヤーがヘルメットを着用している場合（飛行の安全性のため）
				return true; // スロットを使用できます

			return false; // スロットを使用できません
		}

		// 無効化されたアクセサリースロットにアイテムが含まれている場合にアイテムの取り出しを許可するというデフォルトの動作を上書きします
		public override bool IsVisibleWhenNotEnabled() {
			return false; // 有効化されていない場合は表示しないように設定します。注意：これはModがアンロードされた場合の動作には影響しません！
		}

		// アイコンテクスチャ。標準的な画像サイズは32x32です。スロットの中央に配置されます。
		public override string FunctionalTexture => "Terraria/Images/Item_" + ItemID.CreativeWings;

		// マウスがスロットの上にホバーしている間に、様々なものを変更するために使用できます。
		public override void OnMouseHover(AccessorySlotType context) {
			// スロットにアイテムがない間、ホバーテキストが「Wings」と表示されるように変更します。
			switch (context) {
				case AccessorySlotType.FunctionalSlot:
				case AccessorySlotType.VanitySlot:
					Main.hoverItemName = WingsText.Value;
					break;
				case AccessorySlotType.DyeSlot:
					Main.hoverItemName = WingsDyeText.Value;
					break;
			}
		}
	}
}