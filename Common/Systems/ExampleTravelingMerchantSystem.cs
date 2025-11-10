using CorpsMod.Content.NPCs;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CorpsMod.Common.Systems
{
	// ExampleTravelingMerchantSystem の代わりに TravelingMerchantSystem の名前を使用します
	public class TravelingMerchantSystem : ModSystem
	{
		// クエスト完了による定住フラグ。ワールドに保存され、定住NPCスポーンの条件となります。
		public static bool HasSettled = false;

		// ExampleTravelingMerchant.UpdateTravelingMerchant のロジックはこのシステムで呼び出されます。
		public override void PreUpdateWorld() {
			// 定住していない場合にのみ、行商人としての出現ロジックを実行します。
			if (!HasSettled) {
				ExampleTravelingMerchant.UpdateTravelingMerchantLogic();
			}
		}

		public override void SaveWorldData(TagCompound tag) {
			tag["HasSettled"] = HasSettled; // 定住フラグを保存

			// 定住後は行商人としてのデータは不要ですが、互換性のため残しておきます。
			tag["shopItems"] = ExampleTravelingMerchant.shopItems;
			if (ExampleTravelingMerchant.spawnTime != double.MaxValue) {
				tag["spawnTime"] = ExampleTravelingMerchant.spawnTime;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			HasSettled = tag.GetBool("HasSettled"); // 定住フラグをロード

			ExampleTravelingMerchant.shopItems.Clear();
			ExampleTravelingMerchant.shopItems.AddRange(tag.Get<List<Item>>("shopItems"));
			if (!tag.TryGet("spawnTime", out ExampleTravelingMerchant.spawnTime)) {
				ExampleTravelingMerchant.spawnTime = double.MaxValue;
			}
		}

		public override void ClearWorld() {
			HasSettled = false; // 定住フラグをリセット

			ExampleTravelingMerchant.shopItems.Clear();
			ExampleTravelingMerchant.spawnTime = double.MaxValue;
		}

		public override void NetSend(BinaryWriter writer) {
			// 定住フラグを同期
			writer.Write(HasSettled);

			// ショップアイテムを同期
			writer.Write(ExampleTravelingMerchant.shopItems.Count);
			foreach (Item item in ExampleTravelingMerchant.shopItems) {
				ItemIO.Send(item, writer, writeStack: true);
			}
		}

		public override void NetReceive(BinaryReader reader) {
			HasSettled = reader.ReadBoolean(); // 定住フラグを受信

			ExampleTravelingMerchant.shopItems.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++) {
				ExampleTravelingMerchant.shopItems.Add(ItemIO.Receive(reader, readStack: true));
			}
		}
	}
}