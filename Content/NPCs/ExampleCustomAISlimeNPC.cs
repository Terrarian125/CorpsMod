using CorpsMod.Content.Items.Placeable.Banners;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CorpsMod.Content.NPCs
{
	// このModNPCは、完全にカスタムされたAIの例として機能します。
	public class ExampleCustomAISlimeNPC : ModNPC
	{
		// State（状態）スロットで使用する列挙型（enum）を定義します。
		// aiスロットを「状態」の保存手段として使用すると、フローチャートのように処理を大幅に簡素化できます。
		private enum ActionState
		{
			Asleep, // 睡眠
			Notice, // プレイヤーに気づく
			Jump,   // ジャンプ
			Hover,  // ホバリング（空中浮遊）
			Fall    // 落下
		}

		// テクスチャのサイズは 36x36 ピクセルで、垂直方向に2ピクセルのパディング（余白）があるため、
		// 垂直方向の間隔（フレームの高さ）は38ピクセルになります。
		// これらは開発者が管理しやすくするためのもので、以下のコードで直接数値を使用することもできますが、
		// コードを整理された状態に保つために列挙型を使用しています。
		private enum Frame
		{
			Asleep,
			Notice,
			Falling,
			Flutter1,
			Flutter2,
			Flutter3
		}

		// これらは参照プロパティです。
		// 例えば、AI_Stateをまるで「NPC.ai[0]」であるかのように記述できるようになり、インデックス0番に独自の名前を与えることができます。
		// これにより、AIコードが乱雑になるのを防ぎます。これらがない場合、以下のAIコード内にあるすべての「AI_State」が
		// 「npc.ai[0]」となり、非常に読みづらくなってしまいます。
		// これはすべて、美しく、管理しやすく、クリーンなコードにするための工夫です。
		public ref float AI_State => ref NPC.ai[0];
		public ref float AI_Timer => ref NPC.ai[1];
		public ref float AI_FlutterTime => ref NPC.ai[2];

		public static LocalizedText GotStompedText { get; private set; }

		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6; // ModNPCのフレーム数を必ず設定してください。

			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.ShimmerSlime;

			// このNPCが免疫を持つデバフを指定します。
			// このNPCは「Poisoned（毒）」デバフに対して免疫を持ちます。
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Buffs.ExampleGravityDebuff>()] = true;

			GotStompedText = this.GetLocalization("GotStomped");
		}

		public override void SetDefaults() {
			NPC.width = 36; // NPCのヒットボックスの幅（ピクセル単位）
			NPC.height = 36; // NPCのヒットボックスの高さ（ピクセル単位）
			NPC.aiStyle = -1; // このNPCは完全に独自のAIを持つため、-1に設定します。デフォルトのaiStyle 0はプレイヤーの方を向くため、カスタムAIコードと競合する可能性があります。
			NPC.damage = 7; // このNPCが与えるダメージ量
			NPC.defense = 2; // このNPCの防御力
			NPC.lifeMax = 25; // このNPCの最大体力
			NPC.HitSound = SoundID.NPCHit1; // 被弾時に再生される効果音
			NPC.DeathSound = SoundID.NPCDeath1; // 死亡時に再生される効果音
			NPC.value = 25f; // 死亡時にドロップする銅貨の量

			Banner = Type;
			BannerItem = ModContent.ItemType<ExampleCustomAISlimeNPCBanner>();
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// このNPCを地上（オーバーワールド）にスポーンさせたいと考えています。
			return SpawnCondition.OverworldDaySlime.Chance * 0.1f;
		}

		// ここでのAIの動き：NPCはプレイヤーが範囲内に入るまで待機（睡眠）し、攻撃のためにジャンプし、
		// 落下途中で羽ばたいて（ホバリング）少しの間空中にとどまり、その後地面に落下します。
		// なお、アニメーションの処理は FindFrame で行う必要があります。
		public override void AI() {
			// NPCは最初、プレイヤーが範囲内に入るのを待つ「睡眠（Asleep）」状態から始まります。
			switch (AI_State) {
				case (float)ActionState.Asleep:
					FallAsleep();
					break;
				case (float)ActionState.Notice:
					Notice();
					break;
				case (float)ActionState.Jump:
					Jump();
					break;
				case (float)ActionState.Hover:
					Hover();
					break;
				case (float)ActionState.Fall:
					if (NPC.velocity.Y == 0) {
						NPC.velocity.X = 0;
						AI_State = (float)ActionState.Asleep;
						AI_Timer = 0;
					}

					break;
			}
		}

		// この FindFrame では、NPCの行動に応じて使用するアニメーションフレームを設定します。
		// npc.frame.Y を「x * frameHeight」に設定します。ここで x はスプライトシート内の上から数えてx番目のフレーム（0からカウント）です。
		// 利便性のために、上で列挙型（Frame）を定義しています。
		public override void FindFrame(int frameHeight) {
			// npc.direction に連動して、スプライトを左右反転させます。
			NPC.spriteDirection = NPC.direction;

			// ほとんどの場合、アニメーションは状態（State）と一致します。
			switch (AI_State) {
				case (float)ActionState.Asleep:
					// npc.frame.Y は、アニメーションフレームを変更するための標準的な方法です。
					// npc.frame はピクセル座標で左上隅から始まることに注意してください。
					NPC.frame.Y = (int)Frame.Asleep * frameHeight;
					break;
				case (float)ActionState.Notice:
					// Notice（気づく）から Asleep（睡眠）へ移行する際、NPCがジャンプのためにしゃがんでいるように見せます。
					if (AI_Timer < 10) {
						NPC.frame.Y = (int)Frame.Notice * frameHeight;
					}
					else {
						NPC.frame.Y = (int)Frame.Asleep * frameHeight;
					}

					break;
				case (float)ActionState.Jump:
					NPC.frame.Y = (int)Frame.Falling * frameHeight;
					break;
				case (float)ActionState.Hover:
					// ここでは、サイクルさせたい3つのフレームがあります。
					NPC.frameCounter++;

					if (NPC.frameCounter < 10) {
						NPC.frame.Y = (int)Frame.Flutter1 * frameHeight;
					}
					else if (NPC.frameCounter < 20) {
						NPC.frame.Y = (int)Frame.Flutter2 * frameHeight;
					}
					else if (NPC.frameCounter < 30) {
						NPC.frame.Y = (int)Frame.Flutter3 * frameHeight;
					}
					else {
						NPC.frameCounter = 0;
					}

					break;
				case (float)ActionState.Fall:
					NPC.frame.Y = (int)Frame.Falling * frameHeight;
					break;
			}
		}

		// ここではカスタムAIを使用している（aiStyleが適切なバニラの値に設定されていない）ため、
		// この「フラッタースライム」がすり抜け足場を通り抜けて落下できるタイミングを手動で決定する必要があります。
		public override bool? CanFallThroughPlatforms() {
			if (AI_State == (float)ActionState.Fall && NPC.HasValidTarget && Main.player[NPC.target].Top.Y > NPC.Bottom.Y) {
				// フラッタースライムが現在落下中であり、プレイヤーがNPCの足元よりも下にいる限り、足場を通り抜けて落下させます。
				return true;
			}

			return false;
			// ここで null を返してバニラの挙動を適用することもできます（カスタムAIの場合は false を返すのと同じ挙動になります）。
		}

		private void FallAsleep() {
			// TargetClosest は、最も近いプレイヤーの player.whoAmI を npc.target に設定します。
			// 引数の faceTarget（true）は、ターゲットされたプレイヤーが右か左にいるかに応じて、npc.direction を自動的に 1 または -1 にすることを意味します。
			// これは、npcが「混乱（confused）」デバフにかかっている場合にも自動的に反転します。
			NPC.TargetClosest(true);

			// ターゲットがまだ有効であり、指定した検知範囲（500ピクセル）内に入っているか確認します。
			if (NPC.HasValidTarget && Main.player[NPC.target].Distance(NPC.Center) < 500f) {
				// 範囲内にターゲットが存在するため、Notice（気づく）状態に移行します。（念のためタイマーを0にリセットします）
				AI_State = (float)ActionState.Notice;
				AI_Timer = 0;
			}
		}

		private void Notice() {
			// ターゲットされたプレイヤーが攻撃範囲内（250ピクセル）にいる場合。
			if (Main.player[NPC.target].Distance(NPC.Center) < 250f) {
				// ここではタイマーを使用して、実際にジャンプするまで0.33秒（20ティック）待ちます。
				// FindFrame内でも、AI_Timerがジャンプ前のしゃがみアニメーションに使用されているのが確認できます。
				AI_Timer++;

				if (AI_Timer >= 20) {
					AI_State = (float)ActionState.Jump;
					AI_Timer = 0;
				}
			}
			else {
				NPC.TargetClosest(true);

				if (!NPC.HasValidTarget || Main.player[NPC.target].Distance(NPC.Center) > 500f) {
					// ターゲットしていたプレイヤーが範囲外に出たようなので、睡眠状態に戻ります。
					AI_State = (float)ActionState.Asleep;
					AI_Timer = 0;
				}
			}
		}

		private void Jump() {
			AI_Timer++;

			if (AI_Timer == 1) {
				// ジャンプフレームに入った最初の1ティック目に初期速度を適用します。テラリアでは -Y が上方向であることに注意してください。
				NPC.velocity = new Vector2(NPC.direction * 2, -10f);
			}
			else if (AI_Timer > 40) {
				// 0.66秒（40ティック）後、ホバリング（Hover）状態に移行します。 // TODO: 重力の影響は？
				AI_State = (float)ActionState.Hover;
				AI_Timer = 0;
			}
		}

		private void Hover() {
			AI_Timer++;

			// ここで、この羽ばたき（ホバリング）がどれくらい続くかを決定します。
			// マルチプレッシャーのクライアント（プレイヤー側）がこのコードを実行するのを防ぐため、 netmode != 1 であるかチェックします。
			// （同様に、プロジェクタイルを生成する処理などもこのようにラップする必要があります）
			// netMode == 0 はシングルプレイ、netMode == 1 はマルチプレイのクライアント、netMode == 2 はマルチプレイのサーバーです。
			// 通常、マルチプレイでは、クライアントとサーバーがそれぞれ確定的なコードを個別に実行することで同じ状態を維持します。
			// ランダムな処理を行いたい場合は、必ずサーバー側で行い、その結果をマルチプレイのクライアントに通知する必要があります。
			if (AI_Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient) {
				// 参考情報として：適切な同期がない場合：https://github.com/user-attachments/assets/27b289c0-37a6-47e8-9e35-ea6f641612f0
				// 適切な同期がある場合：https://github.com/user-attachments/assets/f2bdcea4-8fe6-4eba-aa0d-0d36ade9a16d
				AI_FlutterTime = Main.rand.NextBool() ? 100 : 50;

				// マルチプレイのクライアントへの通知は、npc.netUpdate を true に設定するたびに、
				// ネットワーク経由で npc.ai 配列が自動的に同期されることで行われます。
				// 非決定的（「ランダム」）な処理を行ったとき以外は、netUpdate を設定しないでください。
				NPC.netUpdate = true;
			}

			// ここで、NPCにわずかな上方向の速度を加えます。
			NPC.velocity += new Vector2(0, -.35f);

			// ...さらに、移動速度が遅いときには追加のX方向の速度を加えます。
			if (Math.Abs(NPC.velocity.X) < 2) {
				NPC.velocity += new Vector2(NPC.direction * .05f, 0);
			}

			// 100ティック（1.66秒）以上羽ばたきを続けると、フラッタースライムは疲れてしまうため、落下（Fall）状態に移行します。
			if (AI_Timer > AI_FlutterTime) {
				AI_State = (float)ActionState.Fall;
				AI_Timer = 0;
			}
		}

		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			// ModifyCollisionData を使用して、接触ダメージをカスタマイズできます。
			// ここでは、このNPCが落下状態（Fall）にあり、かつ被害者がNPCのほぼ真下にいる場合にダメージを2倍にします。
			if (AI_State == (float)ActionState.Fall) {
				// npcHitbox を直接変更して動的なヒットボックスを実装することもできますが、
				// この例では、ボーナスダメージを適用するための新しいヒットボックス（範囲）を作成しています。
				// この計算により、元の 36x36 ヒットボックスの下部中央に焦点を当てたヒットボックスが作成されます：
				// --> ☐☐☐
				//     ☐☒☐
				Rectangle extraDamageHitbox = new Rectangle(npcHitbox.X + 12, npcHitbox.Y + 18, npcHitbox.Width - 24, npcHitbox.Height - 18);
				if (victimHitbox.Intersects(extraDamageHitbox)) {
					damageMultiplier *= 2f;
					Main.NewText(GotStompedText.Value);
				}
			}
			return true;
		}
	}
}