using CorpsMod.Common.Players;
using CorpsMod.Content.Items.Accessories;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CorpsMod.Content.Buffs
{
	public class AbsorbTeamDamageBuff : ModBuff
	{
		public override LocalizedText Description => base.Description.WithFormatArgs(AbsorbTeamDamageAccessory.DamageAbsorptionPercent);

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExampleDamageModificationPlayer>().defendedByAbsorbTeamDamageEffect = true;
		}
	}
}
