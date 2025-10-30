using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CorpsMod.Content.Items.Weapons
{
	// このファイル内のクラスは、カスタムのuse styleを作成し、使用する方法を示しています。use styleは、アイテムが使用されたときの動きとヒットボックスを制御します。
	// このカスタムuse styleは、通常の（ItemUseStyleID.Swingによる）下向きのスイングではなく、下から上へ剣をスイングします。
	// さらに、このカスタムuse styleは任意の角度でスイングできます。
	// 別の例であるExampleCustomSwingSwordは、カスタムuse styleコードを使用する代わりに、ホールドされる投射物（Held Projectile）を使用して、さらに高度なカスタムスイングを示しています。ホールドされる投射物を使用する方が、高度な動きを実装するのは簡単な場合がありますが、use styleのアプローチを好む人もいるかもしれません。

	// ExampleCustomUseStyleGlobalItemには、実際のカスタムUseStyleロジックが含まれています。ModItemに直接カスタムuse styleコードを記述することも可能ですが、この例ではバニラ（オリジナル）の**Cutlass**武器にも同じカスタムuse styleを使用するため、**GlobalItem**を使用しています。
	// ExampleCustomUseStyleItemSetsは、カスタムアイテムuse styleがアイテムタイプごとに使用する追加データを格納します。
	// ExampleCustomUseStylePlayerは、カスタムuse styleを機能させます。

	public class ExampleCustomUseStyleWeapon : ModItem
	{
		public override void SetDefaults() {
			// ここで、ItemのuseStyleをExampleCustomUseStyleGlobalItemに登録されたカスタムuse style値に設定します。
			// これにより、既存のuseStyleのいずれかのデフォルトロジックの代わりに、ExampleCustomUseStyleGlobalItem内のカスタムロジックが実行されるようになります。
			// このアイテムのためだけにカスタムuse styleを作成し、GlobalItemでカスタムuse styleコードを共有しない場合は、代わりにItem.useStyleを-1に設定し、このクラス内でUseStyle、UseItemHitbox、およびUseItemFrameメソッドを実装できます。
			Item.useStyle = ExampleCustomUseStyleGlobalItem.ExampleCustomUseStyle;

			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.DamageType = DamageClass.Melee;
			Item.damage = 20;
			Item.width = 58;
			Item.height = 58;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;
		}

		public override void SetStaticDefaults() {
			// これは、武器が手からどれだけ離れて保持されるかを制御します。この武器は0を使用していますが（したがって、実際には設定する必要はありません）、ExampleCustomUseStyleGlobalItemのロジックは他の値でも機能します。ここで設定することも、CreateIntSetメソッドで設定することもできます。どちらでも機能します。
			//ExampleCustomUseStyleItemSets.HandOffsets[Type] = 0;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	// カスタムuse styleがスイング角度を決定するためにマウスの位置を使用するため、スイング角度を格納および同期するために対応するModPlayerが必要です。
	// これがないと、他のすべてのプレイヤーは、プレイヤーが真右にスイングしているのを見るだけになります。
	public class ExampleCustomUseStylePlayer : ModPlayer
	{
		public float swingAngle;

		public void SyncDirection(int whoAmI) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)CorpsMod.MessageType.SendCustomUseStylePlayerDirection);
			packet.Write((byte)whoAmI);
			packet.Write(swingAngle);
			packet.Send(ignoreClient: whoAmI);
		}

		public static void ReceiveDirection(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
			}

			ExampleCustomUseStylePlayer useStylePlayer = Main.player[player].GetModPlayer<ExampleCustomUseStylePlayer>();
			useStylePlayer.swingAngle = reader.ReadSingle();

			if (Main.netMode == NetmodeID.Server) {
				useStylePlayer.SyncDirection(player);
			}
		}
	}

	// ReinitializeDuringResizeArraysとカスタムアイテムセットの操作の詳細については、CustomItemSets.csを参照してください。
	[ReinitializeDuringResizeArrays]
	public static class ExampleCustomUseStyleItemSets
	{
		// 必要に応じてカスタムの保持オフセットを格納します。デフォルトは0です。
		// 例えば、Cutlassはプレイヤーにわずかに近づけて保持されます。
		// ExampleCustomUseStyleWeaponは、デフォルトで0になりますが、例としてここに含まれています。
		public static int[] HandOffsets = ItemID.Sets.Factory.CreateIntSet(0, ItemID.Cutlass, -6, ModContent.ItemType<ExampleCustomUseStyleWeapon>(), 0);
	}

	// このクラスには、実際のカスタムUseStyleロジックが含まれています。
	public class ExampleCustomUseStyleGlobalItem : GlobalItem
	{
		public static int ExampleCustomUseStyle;

		public override void Load() {
			// ModItem/GlobalItem.SetDefaultsで使用できるように、値が設定され準備できるように、Loadでカスタムuse style IDを登録します。
			ExampleCustomUseStyle = ItemLoader.RegisterUseStyle(Mod, "ExampleCustomUseStyle");
		}

		// 各メソッドでitem.useStyleをチェックするのではなく、**AppliesToEntity**を使用して、このクラスのロジックがカスタムuse styleを使用するように設定されたアイテムに対してのみ実行されるようにします。
		// ここでItemID.Cutlassをチェックする必要があるのは、そうしないと以下のSetDefaultsがCutlassに対して実行されず、そのItem.useStyleを変更できないためです。
		public override bool AppliesToEntity(Item item, bool lateInstantiation) {
			return lateInstantiation && (item.type == ItemID.Cutlass || item.useStyle == ExampleCustomUseStyle);
		}

		public override void SetDefaults(Item item) {
			// Cutlassはこれでカスタムuse styleを使用するようになり、通常の振り下ろしではなく、振り上げが行われます。
			if (item.type == ItemID.Cutlass) {
				item.useStyle = ExampleCustomUseStyle;
			}
		}

		public override void SetStaticDefaults() {
			//ExampleCustomUseStyleItemSets.HandOffsets[ItemID.Cutlass] = -6; // HandOffsetsを設定する別の方法
		}

		// **UseStyle**メソッドを使用して、武器アニメーション中にアイテムが描画される場所を決定します。
		public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) {
			// このuse styleは任意の方向にスイングできるため、武器アニメーションを適切に同期するために、ModPlayerを使用してスイング方向を保存および同期する必要があります。
			ExampleCustomUseStylePlayer useStylePlayer = player.GetModPlayer<ExampleCustomUseStylePlayer>();

			// スイングの進行度を0から1の間（0%から100%）で求めます。
			// player.itemAnimationは最大値（player.itemAnimationMax）から始まり、0までカウントダウンします。
			// 0に達すると、プレイヤーは（通常）アイテムアニメーションを終了します。
			float percentDone = 1 - (float)player.itemAnimation / player.itemAnimationMax;

			// アイテムがスイング全体でカバーする合計角度
			float swingArcRange = MathHelper.ToRadians(115);

			// アニメーションが開始されたときに、スイングの方向を決定します。このコードは、マウスポインターの位置が関与するため、ローカルプレイヤーでのみ実行する必要があります。
			if (player.ItemAnimationJustStarted && player.whoAmI == Main.myPlayer) {
				// カーソルへの角度を計算します。このコードは、逆重力も適切に処理します。
				useStylePlayer.swingAngle = ((Main.MouseWorld - player.MountedCenter) * new Vector2(1, player.gravDir)).ToRotation();

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					// 正しいスイング角度を他のプレイヤーが見られるように、この値を送信します。
					useStylePlayer.SyncDirection(Main.myPlayer);
				}
			}

			// ターゲット角度に応じて、プレイヤーが左向きか右向きかを設定します。
			player.direction = Utils.ToDirectionInt(useStylePlayer.swingAngle.ToRotationVector2().X >= 0);

			// 開始および終了の回転値を計算します。
			float start = useStylePlayer.swingAngle + (swingArcRange * .5f * player.direction);
			float end = useStylePlayer.swingAngle - (swingArcRange * .5f * player.direction);

			// そして、それらを使用して、武器アニメーションが再生されてからの時間に基づいて現在の回転値を計算します。
			float currentAngle = MathHelper.Lerp(start, end, percentDone);

			// ここでアイテムの回転を設定します。武器スプライトがそのように方向付けられているため、45度（PiOver4）を追加する必要があります。左向きの場合、スプライトが反転していることを考慮して、さらに回転を追加します。
			if (player.direction > 0) {
				player.itemRotation = currentAngle + MathHelper.PiOver4;
			}
			else {
				player.itemRotation = currentAngle + (MathHelper.PiOver4 * 3);
			}

			// ここで前面の腕の描画パラメーターを設定します。これは、より新しい腕のレンダリングアプローチを使用しています。
			// 通常のバニラSwingはこのアプローチを使用せず、代わりにUseItemFrame中にplayer.bodyFrame.Yを設定する古いアプローチのみを使用します。
			// 古いアプローチは個別の腕の位置を持ちますが、新しいアプローチは任意の角度で腕を描画できます。
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);
			// SetCompositeArmBackを使用して、もう一方の腕の描画パラメーターを設定することもできます。

			// アイテムが描画されるべき場所を示すためにitemLocationを設定します。
			player.itemLocation = player.MountedCenter + currentAngle.ToRotationVector2() * new Vector2(ExampleCustomUseStyleItemSets.HandOffsets[item.type]);

			// このFlipItemLocationAndRotationForGravityメソッドは、逆重力を考慮してitemRotationとitemLocationを調整することを処理します。
			player.FlipItemLocationAndRotationForGravity();
			/* このメソッドが行うことは次のとおりです:
			if (player.gravDir == -1f) {
				player.itemRotation = 0f - player.itemRotation;
				player.itemLocation.Y = player.position.Y + (float)player.height + (player.position.Y - player.itemLocation.Y);
			}
			*/
		}

		// **UseItemHitbox**を使用して、武器アニメーション中のアイテムのヒットボックスを決定します。
		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			// 手の方向を計算します。
			Vector2 handDirection = (player.compositeFrontArm.rotation + MathHelper.PiOver2).ToRotationVector2() * player.gravDir;

			// アイテムのヒットボックスを計算します。渡されるhitboxパラメーターにはすでにアイテムのスケールが適用されているため、ここで再計算する方が柔軟性があります。
			Rectangle drawHitbox = Item.GetDrawHitbox(item.type, player);
			Vector2 itemSize = Main.dedServ ? new Vector2(32) : new Vector2(drawHitbox.Width, drawHitbox.Height);

			// アイテムの拡大縮小効果を考慮して、柄から武器の先端までの距離を計算します。
			float itemLength = (itemSize * player.GetAdjustedItemScale(item)).Length(); // Item.Size/width/heightはドロップされたときのアイテムのヒットボックスの値であるため、これらは使用しません。

			// 柄と先端の位置を計算します。
			Vector2 handlePosition = handDirection * new Vector2(ExampleCustomUseStyleItemSets.HandOffsets[item.type]) + player.MountedCenter;
			Vector2 tipPosition = handlePosition + handDirection * itemLength;

			// これで、これらの値を使用してアイテムのヒットボックスを作成します。
			hitbox = Utils.CornerRectangle(handlePosition.ToPoint(), tipPosition.ToPoint());
			hitbox.Inflate(1, 1); // ヒットボックスをわずかに大きくします。
		}

		// **UseItemFrame**を使用して、武器アニメーション中のプレイヤーアニメーションを駆動します。
		public override void UseItemFrame(Item item, Player player) {
			// 腕のアニメーションを設定するためにSetCompositeArmFrontを使用していますが、まれな状況での視覚的な問題を避けるために、player.bodyFrame.Yを適切な値に設定する必要があります。
			// 一例として、オオカミへの変身中（Lilith's Necklace）の攻撃アニメーションがあります。
			player.bodyFrame.Y = player.bodyFrame.Height;
		}
	}
}


//using Microsoft.Xna.Framework;
//using System.IO;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace CorpsMod.Content.Items.Weapons
//{
//	// The classes in this file showcase making and using a custom use style. A use style controls the movement and hitbox of an item when used.
//	// This custom use style swings the sword up from below instead of the usual down swing of ItemUseStyleID.Swing.
//	// In addition, this custom use style can swing at any angle. 
//	// A separate example, ExampleCustomSwingSword, showcases an even more advanced custom swing using a held projectile instead of using custom use style code. It can be easier to implement advanced movements using a held projectile, but some may prefer the use style approach.

//	// ExampleCustomUseStyleGlobalItem contains the actual custom UseStyle logic. It is possible to put custom use style code directly in a ModItem, but we use a GlobalItem in this example because we use the same custom use style for the vanilla Cutlass weapon as well.
//	// ExampleCustomUseStyleItemSets stores extra data used by the custom item use style on a per-item type basis.
//	// ExampleCustomUseStylePlayer facilitates our custom use style.

//	public class ExampleCustomUseStyleWeapon : ModItem
//	{
//		public override void SetDefaults() {
//			// Here, we set the Item's useStyle to the custom use style value registered in ExampleCustomUseStyleGlobalItem.
//			// This will allow the custom logic in ExampleCustomUseStyleGlobalItem to run instead of the default logic for one of the existing useStyles.
//			// If instead of sharing custom use style code in a GlobalItem we wanted to make a custom use style for just this item, we could instead set Item.useStyle to -1 and implement the UseStyle, UseItemHitbox, and UseItemFrame methods in this class.
//			Item.useStyle = ExampleCustomUseStyleGlobalItem.ExampleCustomUseStyle;

//			Item.useAnimation = 20;
//			Item.useTime = 20;
//			Item.DamageType = DamageClass.Melee;
//			Item.damage = 20;
//			Item.width = 58;
//			Item.height = 58;
//			Item.knockBack = 2f;
//			Item.value = Item.sellPrice(gold: 1);
//			Item.rare = ItemRarityID.Green;
//			Item.autoReuse = true;
//			Item.UseSound = SoundID.Item1;
//		}

//		public override void SetStaticDefaults() {
//			// This controls how far out the weapon should be held from the hand. This weapon uses 0 (so we don't actually need to set it) but the logic in ExampleCustomUseStyleGlobalItem works for other values as well. We can set it here or in the CreateIntSet method, both work.
//			//ExampleCustomUseStyleItemSets.HandOffsets[Type] = 0;
//		}

//		public override void AddRecipes() {
//			CreateRecipe()
//				.AddIngredient<ExampleItem>()
//				.AddTile<Tiles.Furniture.ExampleWorkbench>()
//				.Register();
//		}
//	}

//	// We need a corresponding ModPlayer to store and sync the swing angle because our custom use style uses the mouse location to determine the swing angle.
//	// Without this, all other players will see the player swinging directly to the right.
//	public class ExampleCustomUseStylePlayer : ModPlayer
//	{
//		public float swingAngle;

//		public void SyncDirection(int whoAmI) {
//			ModPacket packet = Mod.GetPacket();
//			packet.Write((byte)CorpsMod.MessageType.SendCustomUseStylePlayerDirection);
//			packet.Write((byte)whoAmI);
//			packet.Write(swingAngle);
//			packet.Send(ignoreClient: whoAmI);
//		}

//		public static void ReceiveDirection(BinaryReader reader, int whoAmI) {
//			int player = reader.ReadByte();
//			if (Main.netMode == NetmodeID.Server) {
//				player = whoAmI;
//			}

//			ExampleCustomUseStylePlayer useStylePlayer = Main.player[player].GetModPlayer<ExampleCustomUseStylePlayer>();
//			useStylePlayer.swingAngle = reader.ReadSingle();

//			if (Main.netMode == NetmodeID.Server) {
//				useStylePlayer.SyncDirection(player);
//			}
//		}
//	}

//	// See CustomItemSets.cs to learn more about ReinitializeDuringResizeArrays and working with custom item sets.
//	[ReinitializeDuringResizeArrays]
//	public static class ExampleCustomUseStyleItemSets
//	{
//		// Stores custom hold offsets if needed. Defaults to 0.
//		// Cutlass, for example, is held slightly closer to the player.
//		// ExampleCustomUseStyleWeapon is included here as an example even though it would default to 0 anyway.
//		public static int[] HandOffsets = ItemID.Sets.Factory.CreateIntSet(0, ItemID.Cutlass, -6, ModContent.ItemType<ExampleCustomUseStyleWeapon>(), 0);
//	}

//	// This class contains the actual custom UseStyle logic.
//	public class ExampleCustomUseStyleGlobalItem : GlobalItem
//	{
//		public static int ExampleCustomUseStyle;

//		public override void Load() {
//			// We register a custom use style ID in Load so that the value is set and ready to use in ModItem/GlobalItem.SetDefaults.
//			ExampleCustomUseStyle = ItemLoader.RegisterUseStyle(Mod, "ExampleCustomUseStyle");
//		}

//		// Rather than checking item.useStyle in each method, we use AppliesToEntity to have the logic in this class only run for items set to use our custom use style.
//		// Checking ItemID.Cutlass is necessary here because SetDefaults below won't run for Cutlass to change its Item.useStyle otherwise.
//		public override bool AppliesToEntity(Item item, bool lateInstantiation) {
//			return lateInstantiation && (item.type == ItemID.Cutlass || item.useStyle == ExampleCustomUseStyle);
//		}

//		public override void SetDefaults(Item item) {
//			// Cutlass will now use out custom use style, making it swing up instead of the normal swing.
//			if (item.type == ItemID.Cutlass) {
//				item.useStyle = ExampleCustomUseStyle;
//			}
//		}

//		public override void SetStaticDefaults() {
//			//ExampleCustomUseStyleItemSets.HandOffsets[ItemID.Cutlass] = -6; // Alternate approach to setting HandOffsets
//		}

//		// We use the UseStyle method to determine where the item will be drawn during the weapon animation
//		public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) {
//			// Due to this use style being able to swing in any direction, we need to use a ModPlayer to store and sync the swing direction to properly sync the weapon animation.
//			ExampleCustomUseStylePlayer useStylePlayer = player.GetModPlayer<ExampleCustomUseStylePlayer>();

//			// Find how far through out swing we are, between 0 and 1 (0% to 100%)
//			// player.itemAnimation starts at its highest value (player.itemAnimationMax), and ticks down to 0
//			// When it hits 0, the player is (usually) finished with their item animation
//			float percentDone = 1 - (float)player.itemAnimation / player.itemAnimationMax;

//			// The total angle that the item will cover throughout its swing
//			float swingArcRange = MathHelper.ToRadians(115);

//			// When the animation starts, determine the swing direction. This code must only run on the local player since it involves the mouse cursor location.
//			if (player.ItemAnimationJustStarted && player.whoAmI == Main.myPlayer) {
//				// Calculate the angle towards the cursor. Note that this code properly handles reverse gravity.
//				useStylePlayer.swingAngle = ((Main.MouseWorld - player.MountedCenter) * new Vector2(1, player.gravDir)).ToRotation();

//				if (Main.netMode == NetmodeID.MultiplayerClient) {
//					// Send this value to other players so they see the correct swing angle.
//					useStylePlayer.SyncDirection(Main.myPlayer);
//				}
//			}

//			// Set the player facing left or right depending on the target angle.
//			player.direction = Utils.ToDirectionInt(useStylePlayer.swingAngle.ToRotationVector2().X >= 0);

//			// Calculate start and end rotational values
//			float start = useStylePlayer.swingAngle + (swingArcRange * .5f * player.direction);
//			float end = useStylePlayer.swingAngle - (swingArcRange * .5f * player.direction);

//			// and use them to calculate the current rotational value based on how long the weapon animation has been playing
//			float currentAngle = MathHelper.Lerp(start, end, percentDone);

//			// Here we set the rotation of the item. We need to add 45 degrees (PiOver4) because the weapon sprite is oriented that way. When facing left we add more rotation to account for the sprite being flipped.
//			if (player.direction > 0) {
//				player.itemRotation = currentAngle + MathHelper.PiOver4;
//			}
//			else {
//				player.itemRotation = currentAngle + (MathHelper.PiOver4 * 3);
//			}

//			// Here we set the front arm drawing parameters. This uses the newer arm rendering approach.
//			// The normal vanilla Swing doesn't use this approach and instead only uses the old approach of setting player.bodyFrame.Y during UseItemFrame.
//			// The old approach has discrete arm positions while the new approach can draw the arm at any angle.
//			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);
//			// We could also use SetCompositeArmBack to set the drawing parameters of the other arm.

//			// We set itemLocation to indicate where the item should be drawn
//			player.itemLocation = player.MountedCenter + currentAngle.ToRotationVector2() * new Vector2(ExampleCustomUseStyleItemSets.HandOffsets[item.type]);

//			// This FlipItemLocationAndRotationForGravity method handles adjusting itemRotation and itemLocation to account for reversed gravity.
//			player.FlipItemLocationAndRotationForGravity();
//			/* This is what the method does:
//			if (player.gravDir == -1f) {
//				player.itemRotation = 0f - player.itemRotation;
//				player.itemLocation.Y = player.position.Y + (float)player.height + (player.position.Y - player.itemLocation.Y);
//			}
//			*/
//		}

//		// We use UseItemHitbox to determine the hitbox of the item during the weapon animation
//		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
//			// Calculate the direction of the hand
//			Vector2 handDirection = (player.compositeFrontArm.rotation + MathHelper.PiOver2).ToRotationVector2() * player.gravDir;

//			// Calculate the item hitbox. The hitbox parameter passed in already has item scale applied so recalculating it here is more flexible.
//			Rectangle drawHitbox = Item.GetDrawHitbox(item.type, player);
//			Vector2 itemSize = Main.dedServ ? new Vector2(32) : new Vector2(drawHitbox.Width, drawHitbox.Height);

//			// Calculate the distance from the handle to the tip of the weapon, taking into account item scaling effects.
//			float itemLength = (itemSize * player.GetAdjustedItemScale(item)).Length(); // We don't use Item.Size/width/height because those values are for the item hitbox when dropped.

//			// Calculate the handle and tip positions
//			Vector2 handlePosition = handDirection * new Vector2(ExampleCustomUseStyleItemSets.HandOffsets[item.type]) + player.MountedCenter;
//			Vector2 tipPosition = handlePosition + handDirection * itemLength;

//			// Now we use those values to create the item hitbox
//			hitbox = Utils.CornerRectangle(handlePosition.ToPoint(), tipPosition.ToPoint());
//			hitbox.Inflate(1, 1); // Make the hitbox slightly bigger.
//		}

//		// We use UseItemFrame to drive the player animation during the weapon animation
//		public override void UseItemFrame(Item item, Player player) {
//			// Even though we are using SetCompositeArmFront to set the arm animation, we still need to set player.bodyFrame.Y to appropriate values to avoid visual issues in rare situations.
//			// One example is animating attacks during the wolf transformation (Lilith's Necklace)
//			player.bodyFrame.Y = player.bodyFrame.Height;
//		}
//	}
//}
