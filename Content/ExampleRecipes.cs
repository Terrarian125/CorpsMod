using CorpsMod.Common;
using CorpsMod.Content.Items.Placeable;
using CorpsMod.Content.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CorpsMod.Content
{
	// このクラスには、アイテムレシピ作成の思慮深い例が含まれています。
	// レシピは、https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes および https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes の
	// Wikiページで詳しく説明されています。不明点がある場合は、Wikiにアクセスしてレシピについてさらに学んでください。
	public class ExampleRecipes : ModSystem
	{
		// レシピグループを保存する場所。後で簡単に使用できるようにします。
		public static RecipeGroup ExampleRecipeGroup;

		public override void Unload() {
			ExampleRecipeGroup = null;
		}

		public override void AddRecipeGroups() {
			// レシピグループを作成し、保存します。
			// Language.GetTextValue("LegacyMisc.37") は英語で「Any」という単語であり、他の言語でも対応する単語が取得されます。
			ExampleRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ModContent.ItemType<Items.ExampleItem>())}",
				ModContent.ItemType<Items.ExampleItem>(), ModContent.ItemType<Items.ExampleDataItem>());

			// 名前の衝突を避けるために、モッド付きアイテムがレシピグループの象徴的なアイテム（または最初のアイテム）である場合、
			// レシピグループに「ModName:ItemName」と名前を付けます。
			RecipeGroup.RegisterGroup("CorpsMod:ExampleItem", ExampleRecipeGroup);

			// 既存のTerrariaレシピグループにアイテムを追加します。ExampleCritterItemはゴールドではありませんが、これの例として機能します。
			RecipeGroup.recipeGroups[RecipeGroupID.GoldenCritter].ValidItems.Add(ModContent.ItemType<ExampleCritterItem>());

			// また、GlassとMagic Sand Dropperのレシピで使用されるSandグループにExampleSandを追加します。
			RecipeGroup.recipeGroups[RecipeGroupID.Sand].ValidItems.Add(ModContent.ItemType<ExampleSandBlock>());

			// 「IronBar」グループは存在しますが、「SilverBar」は存在しません。tModLoaderは、同じ名前で登録されたレシピグループをマージするため、
			// バニラアイテムを最初のアイテムとして使用してレシピグループを登録する場合、他のモッドが同じ概念のためにこのレシピグループを使用することを
			// 想定しているならば、内部アイテム名のみを使用して登録できます。これにより、複数のモッドが余分な労力なしに同じグループに追加できます。
			// このケースでは、SilverBarグループを追加しています。RecipeGroupインスタンスは格納しないでください。使用されない可能性があり、
			// 代わりにRecipe.AddRecipeGroupを使用するときは、同じ nameof(ItemID.ItemName) または RegisterGroupから返された RecipeGroupID を使用します。
			RecipeGroup SilverBarRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
			ItemID.SilverBar, ItemID.TungstenBar, ModContent.ItemType<Items.Placeable.ExampleBar>());
			RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), SilverBarRecipeGroup);
		}

		public override void AddRecipes() {
			////////////////////////////////////////////////////////////////////////////////////
			// 以下の基本的なレシピは、1つの石のブロックから999個のExampleItemを作成します。//
			////////////////////////////////////////////////////////////////////////////////////

			Recipe recipe = Recipe.Create(ModContent.ItemType<Items.ExampleItem>(), 999);
			// これにより、レシピに1つの石のブロックの要求が追加されます。
			recipe.AddIngredient(ItemID.StoneBlock);
			// 完了したら、これを呼び出してレシピを登録します。
			recipe.Register();

			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// 以下のレシピは、Recipeに存在するすべてのメソッド（関数）を紹介し、説明し、「チェイン」と呼ばれる「高度な」スタイルを使用します。//
			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// 前述のチェインが機能する理由は、Register()を除いて、Recipeのすべてのメソッドがそれ自体のインスタンスを返すためです。
			// これにより、ローカル変数の名前を入力せずに、その戻り値に対して後続のメソッドを呼び出すことができます。
			// チェインを使用する場合、最後の行のみにセミコロン(;)があることに注意してください。

			var resultItem = ModContent.GetInstance<Items.ExampleItem>();

			// 新しいレシピを開始します。
			resultItem.CreateRecipe()
				// バニラの材料を追加します。
				// ItemIDを検索します: https://github.com/tModLoader/tModLoader/wiki/Vanilla-Content-IDs#item-ids
				// 複数の材料タイプを指定するには、複数の recipe.AddIngredient() 呼び出しを使用します。
				.AddIngredient(ItemID.StoneBlock)
				// オプションの第2引数で、アイテムのスタックを指定します。スタック値なしで任意の AddIngredient オーバーロードを呼び出した場合、スタックはデフォルトで1になります。
				.AddIngredient(ItemID.Acorn, 10)
				// 現在のアイテムを材料として指定することもできます
				.AddIngredient(resultItem)
				// モッドの材料を追加します。ItemID.ExampleSword を試さないでください。それは機能しません。
				.AddIngredient<Items.Weapons.ExampleSword>()
				// 上記の代替となる文字列ベースのアプローチ。他のモッドのアイテムにのみ使用するようにしてください。遅くなります。
				.AddIngredient(Mod, "ExampleSword")

				// RecipeGroupsを使用すると、類似の材料のグループからのアイテムを受け入れるレシピを作成できます。
				// たとえば、すべての種類の木材はバニラの「Wood」グループにあります。
				// 他のバニラグループについては、こちらを確認してください: https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#using-existing-recipegroups
				.AddRecipeGroup(RecipeGroupID.Wood)
				// AddIngredientと同様に、デフォルト値が1のスタックパラメーターがあります。
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				// ここではモッドのレシピグループを使用しています。レシピグループの登録方法については、AddRecipeGroups() を確認してください。
				.AddRecipeGroup(ExampleRecipeGroup, 2)
				// 上記の代替となる文字列ベースのアプローチ。他のモッドのグループにのみ使用するようにしてください。遅くなります。
				.AddRecipeGroup("Wood")
				.AddRecipeGroup("CorpsMod:ExampleItem", 2)

				// バニラのタイル要件を追加します。
				// クラフトステーションを指定するには、タイルを指定します。TileIDを検索します: https://github.com/tModLoader/tModLoader/wiki/Vanilla-Tile-IDs
				.AddTile(TileID.WorkBenches)
				// モッドのタイル要件を追加します。複数のクラフトステーションを指定するには、複数の recipe.AddTile() 呼び出しを使用します。
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				// 上記の代替となる文字列ベースのアプローチ。他のモッドのタイルにのみ使用するようにしてください。遅くなります。
				.AddTile(Mod, "ExampleWorkbench")

				// 事前定義された条件を追加します。これら3行を組み合わせることで、レシピが夜間の砂漠の水中でクラフトされる必要があるようになります。
				.AddCondition(Condition.InDesert)
				.AddCondition(Condition.NearWater)
				.AddCondition(Condition.TimeNight)
				// カスタム条件を追加します。レシピが機能するには、プレイヤーの体力が1/2未満である必要があります。
				// ここで使用されるキーは、「Localization/*.hjson」ファイルで定義されています。
				// 2番目の引数は、デリゲートを作成するためにラムダ式を使用します。ラムダの詳細については、Googleで学べます。
				.AddCondition(Language.GetOrRegister("Mods.CorpsMod.Conditions.LowHealth"), () => Main.LocalPlayer.statLife < Main.LocalPlayer.statLifeMax / 2)
				// 静的クラスに格納されているため、他のレシピで簡単に再利用できるカスタム条件を追加します。
				// これはカスタム条件に推奨されるアプローチです: https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#custom-conditions
				.AddCondition(ExampleConditions.InExampleBiome)

				// 完了したら、これを呼び出してレシピを登録します。チェインの最後にセミコロンがあることに注意してください。
				.Register();

			// これらのメソッドに加えて、シマーデクラフトに関連するメソッドもあります。これについては、ShimmerShowcase.csを参照してください。

			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// 以下のレシピは、レシピのクローン作成と、元のレシピと異なるように変更する方法を紹介し、説明します。//
			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// 既存のレシピを少し変更してコピーを作成したい場合は、Mod.CloneRecipeを使用してそのレシピのクローンを作成できます。
			// クローンは、所有者モッドがこのモッドになることを除いて、元のレシピのすべてのプロパティを継承します。必要に応じてクローンを変更できます。
			// モッド内でレシピの複数のバリエーションを作成したい場合は、クローンを作成する代わりにヘルパーメソッドを使用する方が簡単な場合があります。
			// AdjTiles、Recipe Groups、またはさまざまなレシピ条件の偽装を適切に使用することでより適切に処理される状況では、レシピのクローン作成を使用しないように注意してください。

			// まず、コピーしたいレシピを作成します。
			Recipe baseRecipe = Recipe.Create(ModContent.ItemType<Items.ExampleItem>(), 10);
			baseRecipe.AddIngredient(ItemID.Wood, 10)
				.AddIngredient(ItemID.CopperCoin)
				.AddCondition(Condition.InBeach)
				.AddCondition(Condition.TimeDay)
				.Register();

			// 別のレシピをクローンすることで、新しいレシピを開始します。
			Recipe clonedRecipe = baseRecipe.Clone()
				// クローン元のレシピに影響を与えることなく、このレシピに新しいプロパティを追加できます。
				.AddIngredient(ItemID.SilverCoin)
				.AddTile(TileID.Anvils);

			// 特定の材料や条件など、レシピからプロパティを削除することもできます。
			clonedRecipe.RemoveIngredient(ItemID.CopperCoin);
			clonedRecipe.RemoveCondition(Condition.InBeach);

			// 完了したら、これを呼び出してレシピを登録します。
			clonedRecipe.Register();

			// レシピには、カスタムのアイテム消費ロジックを含めることもできます。これは、Alchemy Tableがポーションレシピで消費する材料を減らす方法に似ています。
			// 詳細については、https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#custom-item-consumption を参照してください。
			// この例では、Chainアイテムを材料として必要としますが、DontConsumeChain ConsumeIngredientCallbackにより、Chainは消費されません。
			Recipe.Create(ItemID.AlphabetStatueJ)
				.AddIngredient(ItemID.StoneBlock, 10)
				.AddIngredient(ItemID.Chain)
				.AddConsumeIngredientCallback(ExampleRecipeCallbacks.DontConsumeChain)
				.AddTile(TileID.HeavyWorkBench)
				.Register();

			// レシピは、クラフトされた後にカスタムコードを実行することもできます。
			// 詳細については、https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#custom-recipe-craft-behavior を参照してください。
			// この例では、レシピがクラフトされたときに花火をランダムにスポーンする可能性のあるコードを実行します。
			Recipe.Create(ItemID.AlphabetStatueZ)
				.AddIngredient(ItemID.StoneBlock, 10)
				.AddIngredient(ItemID.Chain)
				.AddOnCraftCallback(ExampleRecipeCallbacks.RandomlySpawnFireworks)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}

		public override void PostAddRecipes() {
			for (int i = 0; i < Recipe.numRecipes; i++) {
				Recipe recipe = Main.recipe[i];
				//	// 木材を必要とするすべてのレシピは、100%多く必要になります。
				//	if (recipe.TryGetIngredient(ItemID.Wood, out Item ingredient)) {
				//		ingredient.stack *= 2;
				//	}

				//// レシピがかまどを要求しているか確認
				//// requiredTileリストにかまどのIDが含まれているかチェックします。
				//if (recipe.requiredTile.Contains(TileID.Furnaces)) {
				//	// 2. 溶鉱炉がまだ追加されていない場合に追加
				//	// すでに溶鉱炉が必要なレシピであれば二重に追加する必要はないため、チェックします。
				//	if (!recipe.requiredTile.Contains(TileID.Hellforge)) {

				//		// 3. Hellforge (溶鉱炉) をクラフトステーションとして追加
				//		recipe.AddTile(TileID.Hellforge);
			}
		}
	}
}