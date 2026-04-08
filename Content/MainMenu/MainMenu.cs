using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Assets.Textures.Menu
{
	public class RainyMainMenu : ModMenu
	{
		public class RainDrop
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Scale;
			public Color DrawColor;

			public RainDrop(Vector2 pos, Vector2 vel, float scale, Color color) {
				Position = pos;
				Velocity = vel;
				Scale = scale;
				DrawColor = color;
			}
		}

		public static List<RainDrop> Drops = new();

		//メニュー選択画面での表示名
		public override string DisplayName => "Corps Rain Menu";

		//画像パスの指定（Assets/Textures/Menu/ 内を参照）
		public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("CorpsMod/Assets/Textures/Menu/Logo");
		public override Asset<Texture2D> SunTexture => ModContent.Request<Texture2D>("CorpsMod/Assets/Textures/Menu/Blank");
		public override Asset<Texture2D> MoonTexture => ModContent.Request<Texture2D>("CorpsMod/Assets/Textures/Menu/Blank");

		//BGMを雨の音に設定
		public override int Music => MusicID.Rain;

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
			//背景の描画
			Texture2D bgTex = ModContent.Request<Texture2D>("CorpsMod/Assets/Textures/Menu/MenuBackground").Value;

			float xScale = (float)Main.screenWidth / bgTex.Width;
			float yScale = (float)Main.screenHeight / bgTex.Height;
			float scale = Math.Max(xScale, yScale); //画面を覆い尽くす最小倍率

			Vector2 drawOffset = Vector2.Zero;
			if (xScale != yScale) {
				if (yScale > xScale)
					drawOffset.X -= (bgTex.Width * scale - Main.screenWidth) * 0.5f;
				else
					drawOffset.Y -= (bgTex.Height * scale - Main.screenHeight) * 0.5f;
			}

			spriteBatch.Draw(bgTex, drawOffset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			//雨粒の生成ロジック
			if (Drops.Count < 600) {
				for (int i = 0; i < 4; i++) {
					//画面の少し外側から発生させる
					Vector2 startPos = new Vector2(Main.rand.NextFloat(-100, Main.screenWidth + 100), -50f);
					//速度：少し右に流れるようにしつつ、垂直方向に速く
					Vector2 vel = new Vector2(Main.rand.NextFloat(0.5f, 2f), Main.rand.NextFloat(12f, 20f));
					float dropScale = Main.rand.NextFloat(0.4f, 1.0f);

					//雨粒の色（少し青みのある半透明の白）
					Color color = new Color(150, 180, 220, 150) * dropScale;

					Drops.Add(new RainDrop(startPos, vel, dropScale, color));
				}
			}

			//雨粒の更新と描画 
			Texture2D rainTex = ModContent.Request<Texture2D>("CorpsMod/Assets/Textures/Menu/RainDrop").Value;

			for (int i = 0; i < Drops.Count; i++) {
				Drops[i].Position += Drops[i].Velocity;

				//雨が落ちる方向に画像を傾ける計算
				float rotation = Drops[i].Velocity.ToRotation() - MathHelper.PiOver2;

				spriteBatch.Draw(rainTex, Drops[i].Position, null, Drops[i].DrawColor, rotation, rainTex.Size() * 0.5f, Drops[i].Scale, SpriteEffects.None, 0f);

				//画面の下端を超えたらリストから削除
				if (Drops[i].Position.Y > Main.screenHeight + 50) {
					Drops.RemoveAt(i);
					i--;
				}
			}

			//仕上げ 
			drawColor = Color.White; //ロゴが暗くならないように
			Main.time = 27000;       //ゲーム内時間を昼に固定
			Main.dayTime = true;

			return true; //ロゴの描画自体はバニラの処理に任せる
		}
	}
}