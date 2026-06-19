using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CorpsMod.Content.NPCs
{
	public class ExampleGlobalNPC : GlobalNPC
	{
		// TODO: この値が変更されたときに npc.netUpdate を実行する処理。また、GlobalNPCには SendExtraAI フックが用意されています。
		public bool HasBeenHitByPlayer;

		// 各NPCエンティティ（インスタンス）ごとに独立したデータを持たせるために true に設定します。
		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			// ModNPCの生成処理が走った後（lateInstantiation == true）に、そのエンティティが町NPC（townNPC）であるかどうかをチェックして適用します。
			return lateInstantiation && entity.townNPC;
		}

		// NPCがプロジェクタイル（遠距離攻撃や魔法など）によって攻撃を当てられたときに呼び出されます。
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			// プロジェクタイルの所有者が有効なプレイヤーであり（255は所有者なし/サーバーなどの特殊な値）、かつプレイヤー側の攻撃（friendly）である場合
			if (projectile.owner != 255 && projectile.friendly) {
				HasBeenHitByPlayer = true;
			}
		}

		// NPCがアイテム（近接武器など）によって直接攻撃を当てられたときに呼び出されます。
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
			HasBeenHitByPlayer = true;
		}

		// もしも商人（NPC）がプレイヤーから攻撃を受けていた場合、ショップでの販売価格を2倍にします。
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) {
			// このNPCの ExampleGlobalNPC インスタンスを取得し、プレイヤーから攻撃されたフラグが立っていない場合は処理を抜けます。
			if (!npc.GetGlobalNPC<ExampleGlobalNPC>().HasBeenHitByPlayer) {
				return;
			}

			foreach (Item item in items) {
				// 「空気（何も配置されていない枠）」や、nullのアイテムはスキップします。
				if (item == null || item.type == ItemID.None) {
					continue;
				}

				// カスタム価格（ shopCustomPrice ）が設定されている場合はそれを使い、なければアイテム本来の価値（ value ）を取得します。
				int value = item.shopCustomPrice ?? item.value;
				//カスタム価格に設定
				item.shopCustomPrice = value * 1;
			}
		}

		// ワールド保存時などに、NPCのデータを保存（セーブ）するための処理です。
		public override void SaveData(NPC npc, TagCompound tag) {
			if (HasBeenHitByPlayer) {
				tag.Add("HasBeenHitByPlayer", true);
			}
		}

		// ワールド読み込み時などに、保存されたデータをNPCに読み込む（ロード）するための処理です。
		public override void LoadData(NPC npc, TagCompound tag) {
			// タグ内に "HasBeenHitByPlayer" というキーが存在する場合、true になります。
			HasBeenHitByPlayer = tag.ContainsKey("HasBeenHitByPlayer");
		}
	}
}