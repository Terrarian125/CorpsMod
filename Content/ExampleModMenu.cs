using CorpsMod.Backgrounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content
{
	public class CorpsModMenu : ModMenu
	{
		private const string menuAssetPath = "CorpsMod/Assets/Textures/Menu"; // テクスチャパスを表す定数を定義します。これにより、同じパスを複数回記述する必要がなくなります。

		private Asset<Texture2D> sunTexture;
		private Asset<Texture2D> moonTexture;

		public override void Load() {
			sunTexture = ModContent.Request<Texture2D>($"{menuAssetPath}/ExampleSun");
			moonTexture = ModContent.Request<Texture2D>($"{menuAssetPath}/ExampliumMoon");
		}

		public override Asset<Texture2D> Logo => base.Logo; // このモッドメニューで使用するロゴテクスチャを返します。ここではバニラのロゴが使用されます。

		public override Asset<Texture2D> SunTexture => sunTexture; // このモッドメニューで使用する太陽のテクスチャを返します。

		public override Asset<Texture2D> MoonTexture => moonTexture; // このモッドメニューで使用する月のテクスチャを返します。

		/*
		CorpsModでは、https://github.com/tModLoader/tModLoader/wiki/Assets#asset-loading-timing で推奨されているように、
		すべての「追加の」テクスチャを**事前にロード**します。
		非常に大きなテクスチャをめったに使用しないなど、まれな状況では、代わりにオンデマンドでテクスチャをロードすることも可能です。
		その場合の記述は次のようになります。
		private Asset<Texture2D> moonTexture;
		public override Asset<Texture2D> MoonTexture => moonTexture ??= ModContent.Request<Texture2D>($"{menuAssetPath}/ExampliumMoon");
		*/

		public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MysteriousMystery"); // このモッドメニューで使用する音楽を返します。

		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<ExampleSurfaceBackgroundStyle>(); // このメニューで使用する背景スタイルを返します。

		public override string DisplayName => "Corps ModMenu"; // メインメニューのモッドメニュー選択画面に表示される名前を返します。

		public override void OnSelected() {
			SoundEngine.PlaySound(SoundID.Thunder); // このModMenuが選択されたときに雷のサウンドを再生します。
		}

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
			//drawColor = Main.DiscoColor; // ロゴの描画色を変更します (ディスコカラーに)。
			return true; // trueを返すと、ロゴが通常通り描画されます。falseを返すと、描画をスキップできます。
		}
	}
}