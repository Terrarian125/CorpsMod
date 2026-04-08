using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CorpsMod.Assets.Textures.Menu
{
	//バニラの背景システム（パララックスやフェード）を完全に無効化するためのクラス
	internal class NullSurfaceBackground : ModSurfaceBackgroundStyle
	{
		public override void ModifyFarFades(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) {
				if (i == Slot) {
					fades[i] += transitionSpeed;
					if (fades[i] > 1f)
						fades[i] = 1f;
				}
				else {
					fades[i] -= transitionSpeed;
					if (fades[i] < 0f)
						fades[i] = 0f;
				}
			}
		}

		//何も描画しないための空のピクセル画像を指定
		private static readonly string TexPath = "CorpsMod/Assets/Textures/Menu/Blank";

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => BackgroundTextureLoader.GetBackgroundSlot(TexPath);
		public override int ChooseFarTexture() => BackgroundTextureLoader.GetBackgroundSlot(TexPath);
		public override int ChooseMiddleTexture() => BackgroundTextureLoader.GetBackgroundSlot(TexPath);

		//バニラの背景描画をスキップ
		public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) => false;
	}
}