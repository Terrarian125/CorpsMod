using CorpsMod.Content.Achievements;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CorpsMod.Content.NPCs
{
	/// <summary>
	/// This file shows off a critter npc. The unique thing about critters is how you can catch them with a bug net.
	/// このファイルはクリッターNPCを紹介しています。クリッターのユニークな点は、虫取り網で捕まえられることです。
	/// The important bits are: Main.npcCatchable, NPC.catchItem, and Item.makeNPC.
	/// 重要な部分は、Main.npcCatchable、NPC.catchItem、および Item.makeNPC です。
	/// We will also show off adding an item to an existing RecipeGroup (see ExampleRecipes.AddRecipeGroups).
	/// また、既存のレシピグループにアイテムを追加する方法（ExampleRecipes.AddRecipeGroupsを参照）も紹介します。
	/// Additionally, this example shows an involved IL edit.
	/// さらに、この例は複雑な IL 編集を示しています。
	/// </summary>
	public class ExampleCritterNPC : ModNPC
	{
		private const int ClonedNPCID = NPCID.Frog; // Easy to change type for your modder convenience
													// モッダーの便宜のためにタイプを簡単に変更できます

		public override void Load() {
			IL_Wiring.HitWireSingle += HookFrogStatue;
		}

		/// <summary>
		/// Change the following code sequence in Wiring.HitWireSingle
		/// Wiring.HitWireSingle の以下のコードシーケンスを変更します
		/// <code>
		///case 61:
		///num115 = 361;
		/// </code>
		/// to
		/// へ
		/// <code>
		///case 61:
		///num115 = Main.rand.NextBool() ? 361 : NPC.type
		/// </code>
		/// This causes the frog statue to spawn this NPC 50% of the time
		/// これにより、カエル像がこのNPCを50%の確率でスポーンさせるようになります
		/// </summary>
		/// <param name="ilContext"> </param>
		private void HookFrogStatue(ILContext ilContext) {
			try {
				// Obtain a cursor positioned before the first instruction of the method the cursor is used for navigating and modifying the il
				// カーソル（ILをナビゲートおよび変更するために使用されます）を、メソッドの最初の命令の前に配置します

				// The exact location for this hook is very complex to search for due to the hook instructions not being unique and buried deep in control flow. Switch statements are sometimes compiled to if-else chains, and debug builds litter the code with no-ops and redundant locals.
				// このフックの正確な場所は、フック命令が一意ではなく、制御フローの奥深くに埋もれているため、検索するのが非常に複雑です。Switch 文は if-else チェーンにコンパイルされることがあり、デバッグビルドはコードを no-op（何もしない操作）や冗長なローカル変数で散らかします。
				// In general you want to search using structure and function rather than numerical constants which may change across different versions or compile settings. Using local variable indices is almost always a bad idea.
				// 一般に、バージョンやコンパイル設定によって変わる可能性のある数値定数ではなく、構造と機能を使用して検索することが望ましいです。ローカル変数のインデックスを使用するのは、ほとんどの場合悪い考えです。
				// We can search for
				// 以下を検索できます
				// switch (*)
				//   case 61:
				//     num115 = 361;

				// In general you'd want to look for a specific switch variable, or perhaps the containing switch (type) { case 105: but the generated IL is really variable and hard to match in this case.
				// 一般には、特定の switch 変数、またはそれを包含する switch (type) { case 105: を探したいところですが、このケースでは生成される IL が非常に変動的で、一致させるのが困難です。
				// We'll just use the fact that there are no other switch statements with case 61
				// case 61 を持つ他の switch 文がないという事実を利用します

				// Some optimizing compilers generate a sub so that all the switch cases start at 0:
				// 一部の最適化コンパイラは、すべての switch ケースが 0 から始まるように sub（減算）を生成します

				// Get the label for case 61: if it exists
				// case 61: のラベルが存在する場合、それを取得します

				// Move the cursor to case 61:
				// カーソルを case 61: に移動します
				// Move the cursor after 361 is pushed onto the stack
				// スタックに 361 がプッシュされた後にカーソルを移動します
				// There are lots of extra checks we could add here to make sure we're at the right spot, such as not encountering any branching instructions
				// ここが正しい場所であることを確認するために、分岐命令に遭遇しないことなど、多くの追加チェックを追加できます

				// Now we add additional code to modify the current value that will be assigned to num115
				// 次に、num115 に割り当てられる現在の値を変更するための追加のコードを追加します

				// Hook applied successfully
				// フックが正常に適用されました

				// Couldn't find the right place to insert.
				// 挿入する正しい場所が見つかりませんでした。

				// If there are any failures with the IL editing, this method will dump the IL to Logs/ILDumps/{Mod Name}/{Method Name}.txt
				// IL編集で失敗があった場合、このメソッドはILを Logs/ILDumps/{Mod Name}/{Method Name}.txt にダンプします
			}
			catch {
				// If there are any failures with the IL editing, this method will dump the IL to Logs/ILDumps/{Mod Name}/{Method Name}.txt
				// IL編集で失敗があった場合、このメソッドはILを Logs/ILDumps/{Mod Name}/{Method Name}.txt にダンプします
				MonoModHooks.DumpIL(ModContent.GetInstance<CorpsMod>(), ilContext);
			}
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[ClonedNPCID]; // Copy animation frames
																		// アニメーションフレームをコピーします
			Main.npcCatchable[Type] = true; // This is for certain release situations
											// これは特定のリリース状況（虫取り網による）のためのものです

			// These three are typical critter values
			// これら3つは一般的なクリッターの値です

			// The frog is immune to confused
			// カエルは混乱のデバフに対して耐性があります

			// This is so it appears between the frog and the gold frog
			// これにより、カエルとゴールデンカエルの間に（Bestiaryの）表示順位が入ります
			NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(ClonedNPCID) + 1, Type);
		}

		public override void SetDefaults() {
			// width = 12;
			// height = 10;
			// aiStyle = 7;
			// damage = 0;
			// defense = 0;
			// lifeMax = 5;
			// HitSound = SoundID.NPCHit1;
			// DeathSound = SoundID.NPCDeath1;
			// catchItem = 2121;
			// Sets the above
			// 上記（コメントアウトされた設定）を設定します
			NPC.CloneDefaults(ClonedNPCID);

			NPC.catchItem = ModContent.ItemType<ExampleCritterItem>();
			NPC.lavaImmune = true;
			AIType = ClonedNPCID;
			AnimationType = ClonedNPCID;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
				new FlavorTextBestiaryInfoElement("Mods.CorpsMod.Bestiary.ExampleCritterNPC"));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.Underworld.Chance * 0.1f;
		}

		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 6; i++) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Worm, 2 * hit.HitDirection, -2f);
					if (Main.rand.NextBool(2)) {
						dust.noGravity = true;
						dust.scale = 1.2f * NPC.scale;
					}
					else {
						dust.scale = 0.7f * NPC.scale;
					}
				}
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore_Head").Type, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>($"{Name}_Gore_Leg").Type, NPC.scale);
			}
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			ModContent.GetInstance<AdvancedExampleAchievement>().TotalDamageCondition.Value += damageDone;
			if (item.type == ItemID.IronPickaxe) {
				ModContent.GetInstance<AdvancedExampleAchievement>().IronPickaxeCondition.Complete();
			}
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			// OnHitByProjectile can be called on servers, so we need to check this to make sure to only access the achievement on a client to avoid null exceptions.
			// OnHitByProjectile はサーバーで呼び出される可能性があるため、null 例外を避けるためにクライアントでのみアチーブメントにアクセスするように、このチェックを行う必要があります。
			if (Main.netMode != NetmodeID.Server && !projectile.trap && !projectile.npcProj) {
				ModContent.GetInstance<AdvancedExampleAchievement>().TotalDamageCondition.Value += damageDone;
			}
		}

		public override Color? GetAlpha(Color drawColor) {
			// GetAlpha gives our Lava Frog a red glow.
			// GetAlpha は、私たちの溶岩カエルに赤い輝きを与えます。
			return drawColor with {
				R = 255,
				// Both these do the same in this situation, using these methods is useful.
				// この状況ではどちらも同じことを行いますが、これらのメソッドを使うことは有用です。
				G = Utils.Clamp<byte>(drawColor.G, 175, 255),
				B = Math.Min(drawColor.B, (byte)75),
				A = 255
			};
		}

		public override bool PreAI() {
			// Kills the NPC if it hits water, honey or shimmer
			// NPCが水、ハチミツ、またはシマーに触れた場合、NPCを殺します
			if (NPC.wet && !Collision.LavaCollision(NPC.position, NPC.width, NPC.height)) { // NPC.lavawet not 100% accurate for the frog
																							// カエルにとって NPC.lavawet は 100%正確ではありません
																							// These 3 lines instantly kill the npc without showing damage numbers, dropping loot, or playing DeathSound. Use this for instant deaths
																							// これら3行は、ダメージ数を表示せず、ルートをドロップせず、DeathSoundを再生せずにNPCを即座に殺します。即死のためにこれを使用してください
				NPC.life = 0;
				NPC.HitEffect();
				NPC.active = false;
				SoundEngine.PlaySound(SoundID.NPCDeath16, NPC.position); // plays a fizzle sound
																		 // ジュッという音を再生します
			}

			return true;
		}

		public override void OnCaughtBy(Player player, Item item, bool failed) {
			if (failed) {
				return;
			}

			Point npcTile = NPC.Center.ToTileCoordinates();

			if (!WorldGen.SolidTile(npcTile.X, npcTile.Y)) { // Check if the tile the npc resides the most in is non solid
															 // NPCが最も多く存在するタイルが非ソリッドであるか確認します
				Tile tile = Main.tile[npcTile];
				tile.LiquidAmount = tile.LiquidType == LiquidID.Lava ? // Check if the tile has lava in it
																	   // そのタイルに溶岩があるか確認します
					Math.Max((byte)Main.rand.Next(50, 150), tile.LiquidAmount) // If it does, then top up the amount
																			   // ある場合は、量を最大値に増やします
					: (byte)Main.rand.Next(50, 150); // If it doesn't, then overwrite the amount. Technically this distinction should never be needed bc it will burn but to be safe it's here
													 // ない場合は、量を上書きします。技術的には、燃えてしまうためこの区別は必要ないはずですが、安全のためにここにあります
				tile.LiquidType = LiquidID.Lava; // Set the liquid type to lava
												 // 液体タイプを溶岩に設定します
				WorldGen.SquareTileFrame(npcTile.X, npcTile.Y, true); // Update the surrounding area in the tilemap
																	  // タイルマップ内の周囲のエリアを更新します
			}
		}
	}

	public class ExampleCritterItem : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaBait[Type] = true; // While this item is not bait, this will require a lava bug net to catch.
												 // このアイテムは餌ではありませんが、これを捕まえるには溶岩用の虫取り網が必要になります。
		}

		public override void SetDefaults() {
			// useStyle = 1;
			// autoReuse = true;
			// useTurn = true;
			// useAnimation = 15;
			// useTime = 10;
			// maxStack = CommonMaxStack;
			// consumable = true;
			// width = 12;
			// height = 12;
			// makeNPC = 361;
			// noUseGraphic = true;

			// Cloning ItemID.Frog sets the preceding values
			// ItemID.Frog をクローンすることで、先行する値（コメントアウトされた設定）が設定されます
			Item.CloneDefaults(ItemID.Frog);
			Item.makeNPC = ModContent.NPCType<ExampleCritterNPC>();
			Item.value += Item.buyPrice(0, 0, 30, 0); // Make this critter worth slightly more than the frog
													  // このクリッターをカエルよりもわずかに高価にします
			Item.rare = ItemRarityID.Blue;
		}
	}
}