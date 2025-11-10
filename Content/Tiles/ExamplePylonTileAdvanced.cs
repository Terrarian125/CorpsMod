using CorpsMod.Content.Items.Placeable; // CorpsMod.Content.Items.Placeable を使用
using CorpsMod.Content.TileEntities; // CorpsMod.Content.TileEntities を使用
using Microsoft.Xna.Framework; // Microsoft.Xna.Framework を使用
using Microsoft.Xna.Framework.Graphics; // Microsoft.Xna.Framework.Graphics を使用
using ReLogic.Content; // ReLogic.Content を使用
using Terraria; // Terraria を使用
using Terraria.Audio; // Terraria.Audio を使用
using Terraria.DataStructures; // Terraria.DataStructures を使用
using Terraria.GameContent; // Terraria.GameContent を使用
using Terraria.GameContent.ObjectInteractions; // Terraria.GameContent.ObjectInteractions を使用
using Terraria.ID; // Terraria.ID を使用
using Terraria.Localization; // Terraria.Localization を使用
using Terraria.Map; // Terraria.Map を使用
using Terraria.ModLoader; // Terraria.ModLoader を使用
using Terraria.ObjectData; // Terraria.ObjectData を使用

namespace CorpsMod.Content.Tiles
{
	/// <summary>
	/// これは、<seealso cref="ExamplePylonTile"/> の実装をより高度にしたバリエーションであり、
	/// <seealso cref="AdvancedPylonTileEntity"/> と連携して、ModPylonで適用できる高度なテクニックを示しています。
	/// 独自のタイルエンティティや、バニラの標準に準拠しないマルチタイルでModPylonを使用したい場合に適した例です。
	/// バニラと同様に動作する通常のパイロンが必要な場合は、<seealso cref="ExamplePylonTile"/> を確認してください。
	/// </summary>
	/// <remarks>
	/// これは高度な例であるため、<seealso cref="ExamplePylonTile"/> で既に説明された内容は、
	/// あまり詳細には説明しません。文脈上必要な場合は説明します。
	/// </remarks>
	public class ExamplePylonTileAdvanced : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8; // クリスタルの垂直フレーム数

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> crystalHighlightTexture;
		public Asset<Texture2D> mapIcon;

		public override void Load() {
			// 他の Example Pylon のスプライトを使用しますが、それを行うにはまずテクスチャ値を調整する必要があります。
			crystalTexture = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_Crystal");
			crystalHighlightTexture = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_CrystalHighlight");
			mapIcon = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_MapIcon");
		}

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true; // タイルを明るくする
			Main.tileFrameImportant[Type] = true; // タイルのフレームが重要であることを示す

			// 今回は、3x4ではなく2x3のタイルにします。
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2); // 原点を設定 (左下隅)
			TileObjectData.newTile.LavaDeath = false; // 溶岩で破壊されない
			TileObjectData.newTile.DrawYOffset = 2; // Y軸方向の描画オフセットを設定
			TileObjectData.newTile.StyleHorizontal = true; // スタイルを水平方向に設定

			// より詳細な機能が必要になるため、バニラのPylon TEのOnPlaceやCanPlaceは使用できません。
			AdvancedPylonTileEntity advancedEntity = ModContent.GetInstance<AdvancedPylonTileEntity>();
			// 配置チェックのフック：配置前にチェックする（AdvancedPylonTileEntity内のメソッドを使用）
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(advancedEntity.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			// 配置後のフック：プレイヤーによる配置後に呼び出す（AdvancedPylonTileEntity内のメソッドを使用）
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(advancedEntity.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type); // タイルデータを追加

			TileID.Sets.InteractibleByNPCs[Type] = true; // NPCと対話可能
			TileID.Sets.PreventsSandfall[Type] = true; // 砂の落下を防ぐ

			AddToArray(ref TileID.Sets.CountsAsPylon); // Pylonとしてカウントされるセットに追加

			LocalizedText pylonName = CreateMapEntryName(); // マップエントリー名を作成
			AddMapEntry(Color.Black, pylonName); // マップにエントリーを追加
		}

		public override NPCShop.Entry GetNPCShopEntry() {
			// このパイロンは、いかなる状況下でもすべてのNPCに対して販売されると仮定します。
			return new NPCShop.Entry(ModContent.ItemType<ExamplePylonItemAdvanced>());
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true; // スマートインタラクトを有効にする
		}

		public override bool RightClick(int i, int j) {
			Main.mapFullscreen = true; // マップをフルスクリーン表示にする
			SoundEngine.PlaySound(SoundID.MenuOpen); // メニュー開く音を再生
			return true;
		}

		public override void MouseOver(int i, int j) {
			Main.LocalPlayer.cursorItemIconEnabled = true; // カーソルアイテムアイコンを有効にする
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ExamplePylonItemAdvanced>(); // アイコンをこのパイロンアイテムに設定
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// マルチタイルが破壊されたとき、対応するタイルエンティティも破壊します。
			ModContent.GetInstance<AdvancedPylonTileEntity>().Kill(i, j);
		}

		// 例として、このパイロンはアクティブである限り常にテレポート可能であるため、これら2つのチェックがtrueを返すようにします。
		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return true; // NPCカウントチェックを常に通過させる
		}

		public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo) {
			return true; // 危険性チェックを常に通過させる
		}

		// 以下の2つのステップは、コインの両面が有効かどうか、つまり、
		// 目的地パイロン（マップ上でクリックされたパイロン）が有効なパイロンであるか、およびプレイヤーが近くに立っているパイロン（近くのパイロン）が
		// 有効なパイロンであるかを判断するものです。これらのチェックのいずれかが失敗した場合、errorKeyにカスタムのローカライゼーションキーが設定され、
		// そのテキスト（ローカライズ後）を含むメッセージがプレイヤーに送信されます。
		public override void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) {
			// パターンマッチング記法に馴染みがない場合、これは次のことを尋ねています：
			// 1) 指定された位置のタイルエンティティがAdvancedPylonTileEntityである（AKA nullや他のものではない）
			// 2) タイルエンティティのisActive値がfalseである
			if (TileEntity.ByPosition[destinationPylonInfo.PositionInTiles] is AdvancedPylonTileEntity { isActive: false }) {
				// これらの両方が真である場合、エラーキーを独自の特別なメッセージに設定し（ローカライゼーションファイルを確認）、目的地の値を無効（false）にします。
				destinationPylonValid = false;
				errorKey = "Mods.CorpsMod.MessageInfo.UnstablePylonIsOff"; // ローカライゼーションキーを設定
			}
		}

		public override void ValidTeleportCheck_NearbyPostCheck(TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool anyNearbyValidPylon, ref string errorKey) {
			// 次のチェックは、近くのパイロンが潜在的に不安定であるかどうかを判断し、もしそうであれば、それがアクティブでない場合もテレポートを阻止します。
			if (TileEntity.ByPosition[nearbyPylonInfo.PositionInTiles] is AdvancedPylonTileEntity { isActive: false }) {
				destinationPylonValid = false;
				errorKey = "Mods.CorpsMod.MessageInfo.NearbyUnstablePylonIsOff"; // ローカライゼーションキーを設定
			}
		}

		public override void ModifyTeleportationPosition(TeleportPylonInfo destinationPylonInfo, ref Vector2 teleportationPosition) {
			// このフックのショーケースとして、テレポート時にプレイヤーをパイロンの上空少しに配置してみましょう。
			teleportationPosition = destinationPylonInfo.PositionInTiles.ToWorldCoordinates(8f, -32f);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// 基本的な例と同じですが、今回は光の色がクリスタルと同じディスコカラー（虹色）になります。
			r = Main.DiscoColor.R / 255f * 0.75f;
			g = Main.DiscoColor.G / 255f * 0.75f;
			b = Main.DiscoColor.B / 255f * 0.75f;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			// 今回は、パイロンがアクティブな場合にのみクリスタルを描画します。
			// ここでフレーミングをチェックして、TEが存在する左上隅にある場合にのみTEを取得しようとしていることを保証する必要があります。
			// このチェックを行わないと、TEが存在しない位置でTEを取得しようとし、エラーが発生し、大量の視覚的なバグを引き起こします。
			if (drawData.tileFrameX % 36 == 0 && drawData.tileFrameY == 0 && TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity entity) && entity is AdvancedPylonTileEntity { isActive: true }) {
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j); // 特殊な描画ポイントを追加
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			// このコードは基本的に基本的な例と同じですが、今回はクリスタルの色がディスコ（虹色）になっています。
			// また、私たちのタイルはバニラよりも1タイル小さいため、パイロンのクリスタルがバニラと同じ高さで描画されるように、
			// crystalOffsetパラメータでクリスタルをそれに応じて上に移動する必要があります。
			DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -18f), Main.DiscoColor * 0.1f, Main.DiscoColor, 1, CrystalVerticalFrameCount);
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			if (!TileEntity.ByPosition.TryGetValue(pylonInfo.PositionInTiles, out var te) || te is not AdvancedPylonTileEntity entity) {
				// 何らかの理由でタイルエンティティが見つからない場合は、何も描画しません。
				return;
			}

			// パイロンがアクティブであるかどうかに応じて、アイコンの色が変わります。
			// それ以外の場合は通常通りに動作します。
			drawColor = !entity.isActive ? Color.Gray * 0.5f : drawColor;
			bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1, 1.5f), drawColor, deselectedScale, selectedScale);
			// マップクリック時の処理（マウスオーバーテキストの設定）
			DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<ExamplePylonItemAdvanced>().DisplayName.Key, ref mouseOverText);
		}
	}
}