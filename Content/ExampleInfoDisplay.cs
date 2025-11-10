using CorpsMod.Common.Players;
using CorpsMod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CorpsMod.Content
{
	/// <summary>
	/// 新しい情報アクセサリー（レーダー、ライフフォームアナライザーなど）を追加する方法を示すために、
	/// <seealso cref="ExampleInfoAccessory"/> および <seealso cref="ExampleInfoDisplayPlayer"/> と組み合わされたInfoDisplay。
	/// </summary>
	public class ExampleInfoDisplay : InfoDisplay
	{
		public static LocalizedText CurrentMinionsText { get; private set; }
		public static LocalizedText NoMinionsText { get; private set; }

		public override void SetStaticDefaults() {
			CurrentMinionsText = this.GetLocalization("CurrentMinions"); // ローカライズされたテキスト「CurrentMinions」を取得します。
			NoMinionsText = this.GetLocalization("NoMinions"); // ローカライズされたテキスト「NoMinions」を取得します。
		}

		public static Color RedInfoTextColor => new(255, 19, 19, Main.mouseTextColor); // 赤色の情報テキストカラーを定義します。

		// デフォルトでは、バニラの円形の外枠テクスチャが使用されます。
		// この情報表示は円形ではなく四角いアイコンを使用しているため、バニラの外枠テクスチャの代わりにカスタムの外枠テクスチャを使用する必要があります。
		// カスタムのホバーテクスチャを使用する必要があるのは、情報表示アイコンがバニラの情報表示が使用する形状と完全に一致しない場合のみです。
		public override string HoverTexture => Texture + "_Hover";

		// これにより、この情報表示をアクティブにするかどうかを決定します。
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
		}

		// ここで、ゲーム内で表示される値を変更できます。
		public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor) {
			// 所持しているミニオンの数を数えます。
			// これは、通常のプレイでこのディスプレイを表示するときにアイコンの横に表示される値です。
			int minionCount = 0;
			foreach (var proj in Main.ActiveProjectiles) {
				if (proj.minion && proj.owner == Main.myPlayer) {
					minionCount++;
				}
			}

			bool noInfo = minionCount == 0;
			if (noInfo) {
				// 「ミニオンなし」が表示される場合、DPSメーターやレーダーと同様に、テキストの色を灰色にします。
				displayColor = InactiveInfoTextColor;
			}
			else if (minionCount < Main.LocalPlayer.maxMinions) {
				// この赤色は、プレイヤーがすべてのミニオンを召喚していないことを示す警告として機能します。
				displayColor = RedInfoTextColor;
			}
			/*
			else if (minionCount == Main.LocalPlayer.maxMinions) {
				// ライフフォームアナライザーでゴールドの生物に使用されるゴールドのテキストカラーは、必要に応じて簡単にアクセスできます。
				displayColor = GoldInfoTextColor;
				displayShadowColor = GoldInfoTextShadowColor;
			}
			*/

			// ミニオンの数が0でない場合は現在のミニオン数を、そうでない場合は「ミニオンなし」のテキストを返します。
			return !noInfo ? CurrentMinionsText.Format(minionCount) : NoMinionsText.Value;
		}
	}
}