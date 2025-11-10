using CorpsMod.Content.NPCs; // CorpsMod.Content.NPCs を使用
using System.Collections.Generic; // System.Collections.Generic を使用
using System.IO; // System.IO を使用
using Terraria; // Terraria を使用
using Terraria.ModLoader; // Terraria.ModLoader を使用
using Terraria.ModLoader.IO; // Terraria.ModLoader.IO を使用

namespace CorpsMod.Common.Systems
{
	public class TravelingMerchantSystem : ModSystem
	{
		// ワールドの更新（ゲーム内時間進行）前に呼び出されます
		public override void PreUpdateWorld() {
			// 行商NPCの出現/デスポーンロジックを更新
			ExampleTravelingMerchant.UpdateTravelingMerchant();
		}

		// ワールドデータ保存時に呼び出されます
		public override void SaveWorldData(TagCompound tag) {
			// ショップアイテムのリストを保存
			tag["shopItems"] = ExampleTravelingMerchant.shopItems;
			// スポーン時間がdouble.MaxValueでない（つまり、その日のスポーンが予定されている）場合にのみ保存
			if (ExampleTravelingMerchant.spawnTime != double.MaxValue) {
				tag["spawnTime"] = ExampleTravelingMerchant.spawnTime;
			}
		}

		// ワールドデータロード時に呼び出されます
		public override void LoadWorldData(TagCompound tag) {
			// ショップアイテムリストをクリアし、ロードされたデータで埋めます
			ExampleTravelingMerchant.shopItems.Clear();
			ExampleTravelingMerchant.shopItems.AddRange(tag.Get<List<Item>>("shopItems"));

			// スポーン時間をロード。データが存在しない場合（TryGetが失敗した場合）はdouble.MaxValueに設定します
			if (!tag.TryGet("spawnTime", out ExampleTravelingMerchant.spawnTime)) {
				ExampleTravelingMerchant.spawnTime = double.MaxValue;
			}
		}

		// ワールドデータクリア時（新規ワールド作成時など）に呼び出されます
		public override void ClearWorld() {
			// ショップアイテムリストとスポーン時間をリセット
			ExampleTravelingMerchant.shopItems.Clear();
			ExampleTravelingMerchant.spawnTime = double.MaxValue;
		}

		// マルチプレイヤーでのデータ送信時に呼び出されます (サーバー -> クライアント)
		public override void NetSend(BinaryWriter writer) {
			// 注: NetSendはWorldDataパケットが送信されるたびに呼び出されます。
			// これを利用して、参加中のプレイヤーにショップアイテムを簡単に同期できるようにしています。
			// Mod開発者には、冗長なデータを繰り返し送信することで帯域幅を消費しすぎないよう、
			// WorldDataを頻繁に送信したり、過度に多くのデータで埋めたりしないことを推奨します。
			// 同期するデータ量が非常に多い場合は、WorldDataの代わりにカスタムパケットの送信を検討してください。

			// アイテムリストの個数を書き込み
			writer.Write(ExampleTravelingMerchant.shopItems.Count);
			// 各アイテムのデータを書き込み
			foreach (Item item in ExampleTravelingMerchant.shopItems) {
				ItemIO.Send(item, writer, writeStack: true); // スタック数も書き込む
			}
		}

		// マルチプレイヤーでのデータ受信時に呼び出されます (サーバー <- クライアント、またはサーバーから全クライアント)
		public override void NetReceive(BinaryReader reader) {
			ExampleTravelingMerchant.shopItems.Clear();
			int count = reader.ReadInt32(); // アイテムの個数を読み込み
			for (int i = 0; i < count; i++) {
				// 各アイテムのデータを読み込み、リストに追加
				ExampleTravelingMerchant.shopItems.Add(ItemIO.Receive(reader, readStack: true)); // スタック数も読み込む
			}
		}
	}
}