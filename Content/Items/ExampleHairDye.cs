using Microsoft.Xna.Framework; // Microsoft.Xna.Framework を使用
using Terraria; // Terraria を使用
using Terraria.GameContent.Dyes; // Terraria.GameContent.Dyes を使用
using Terraria.Graphics.Shaders; // Terraria.Graphics.Shaders を使用
using Terraria.ID; // Terraria.ID を使用
using Terraria.ModLoader; // Terraria.ModLoader を使用

namespace CorpsMod.Content.Items
{
	public class ExampleHairDye : ModItem
	{
		public override void SetStaticDefaults() {
			// 専用サーバー上でのアセットの読み込みを避けます。サーバーはグラフィックカードを使用しません。
			if (!Main.dedServ) {
				// 次のコードは、髪の色を返すデリゲート（匿名メソッド）を作成し、このアイテムのタイプIDに関連付けます。
				GameShaders.Hair.BindShader(
					Item.type,
					// Main.DiscoColorを返すと、髪がアニメーションする虹色になります。ここでは任意のColorを返すことができます。
					new LegacyHairShaderData().UseLegacyMethod((Player player, Color newColor, ref bool lighting) => Main.DiscoColor)
				);
			}

			// リサーチ（研究）に必要なアイテム数を3に設定します
			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.maxStack = Item.CommonMaxStack; // 最大スタック数を一般的な値に設定
			Item.value = Item.buyPrice(gold: 5); // 買値（5ゴールド）を設定
			Item.rare = ItemRarityID.Green; // レア度を緑（Green）に設定
			Item.UseSound = SoundID.Item3; // 使用時の効果音を設定
			Item.useStyle = ItemUseStyleID.DrinkLiquid; // 使用スタイルを液体の飲用に設定
			Item.useTurn = true; // 使用時に向きを変えることを許可
			Item.useAnimation = 17; // アニメーション時間を17フレームに設定
			Item.useTime = 17; // 使用時間を17フレームに設定
			Item.consumable = true; // 消費品として設定
		}
	}
}