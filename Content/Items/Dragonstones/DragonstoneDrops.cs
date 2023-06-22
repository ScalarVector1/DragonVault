using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace DragonVault.Content.Items.Dragonstones
{
	internal class DragonstoneDrops : GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			switch (npc.type)
			{
				case NPCID.EyeofCthulhu:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RoseStone>()));
					break;

				case NPCID.EaterofWorldsHead:
				case NPCID.EaterofWorldsBody:
				case NPCID.EaterofWorldsTail:

					IItemDropRule rule = new LeadingConditionRule(new Conditions.LegacyHack_IsABoss());
					rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CitrineStone>()));

					npcLoot.Add(rule);
					break;

				case NPCID.BrainofCthulhu:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CitrineStone>()));
					break;

				case NPCID.SkeletronHead:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RadiantStone>()));
					break;

				case NPCID.WallofFlesh:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<VerdantStone>()));
					break;

				case NPCID.Retinazer:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CeruleanStone>()));
					break;

				case NPCID.Spazmatism:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CeruleanStone>()));
					break;

				case NPCID.SkeletronPrime:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CeruleanStone>()));
					break;

				case NPCID.TheDestroyer:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CeruleanStone>()));
					break;

				case NPCID.DukeFishron:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AzureStone>()));
					break;

				case NPCID.MoonLordCore:
					npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MidnightStone>()));
					break;

				default:
					npcLoot.Add(ItemDropRule.ByCondition(new PostMoonlordCondition(), ModContent.ItemType<PureStone>(), 1000000));
					break;
			}
		}
	}

	internal class PostMoonlordCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return NPC.downedMoonlord;
		}

		public bool CanShowItemDropInUI()
		{
			return NPC.downedMoonlord;
		}

		public string GetConditionDescription()
		{
			return "After the moon lord falls";
		}
	}
}
