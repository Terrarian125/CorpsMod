//using Terraria; // Terraria を使用
//using Terraria.Audio; // Terraria.Audio を使用
//using Terraria.ID; // Terraria.ID を使用
//using Terraria.ModLoader; // Terraria.ModLoader を使用
//using Terraria.UI; // Terraria.UI を使用

//namespace CorpsMod.Common.Players
//{
//	// カーソルをジェルに重ねると、カーソルスタイルが変わります。
//	// Shiftキーを押しながらクリックすると、その色とレア度が変わります。
//	// GelGlobalItem.cs も参照してください。何が起こるかを示すツールチップ行をジェルに追加しています。
//	public class ExampleShiftClickSlotPlayer : ModPlayer
//	{
//		public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
//			// このアイテムがインベントリ内にあり、ジェルである場合に、変更を適用します
//			if (context == ItemSlot.Context.InventoryItem && inventory[slot].type == ItemID.Gel) {
//				inventory[slot].color = Main.DiscoColor; // アイテムの色を「ランダムな」色に変更します
//				inventory[slot].rare = Main.rand.Next(ItemRarityID.Count); // レア度をランダムにします
//				SoundEngine.PlaySound(SoundID.Item4); // マナクリスタル使用時の効果音を再生します

//				// バニラのコードをブロックし、クリックされたときにアイテムが拾われないようにします。
//				return true;
//			}
//			return base.ShiftClickSlot(inventory, context, slot);
//		}

//		// ここでカーソルスタイルをオーバーライドします
//		public override bool HoverSlot(Item[] inventory, int context, int slot) {
//			// このアイテムがインベントリ内にあり、ジェルである場合に、変更を適用します
//			if (context == ItemSlot.Context.InventoryItem && inventory[slot].type == ItemID.Gel) {
//				// プレイヤーがShiftキーを押している場合、特別なアクションが実行されることを示すためにFavoriteStar（お気に入りスター）のテクスチャを使用します
//				if (ItemSlot.ShiftInUse) {
//					Main.cursorOverride = CursorOverrideID.FavoriteStar;
//					return true; // カーソルをオーバーライドする他のものを防ぐために true を返します
//				}
//			}
//			return base.HoverSlot(inventory, context, slot);
//		}
//	}
//}