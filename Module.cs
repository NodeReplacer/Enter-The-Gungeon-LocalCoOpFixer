using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using Dungeonator;
using UnityEngine;
using Random = System.Random;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MultiplayerBasicExample;

namespace LocalCoOpChest
{
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Module : BaseUnityPlugin
    {
        public const string GUID = "Jackass.etg.LocalCoOpChest";
        public const string NAME = "Local Co-Op Chest Changer";
        public const string VERSION = "1.0.0";
        public const string TEXT_COLOR = "#00FFFF";
        
        /*
        private Hook _OpenHook;
        private Hook _OnBrokenHook;
        private Hook _HandleBossClearRewardHook;
        private Hook _HandleRoomClearedHook;
        */
        
        //public Hook ExampleHook;
        
        public static PlayerItem GetRandomPlayerItemOfQualities(Random usedRandom, List<int> excludedIDs, params PickupObject.ItemQuality[] qualities)
        {
	        List<PlayerItem> list = new List<PlayerItem>();
	        for (int i = 0; i < PickupObjectDatabase.Instance.Objects.Count; i++)
	        {
		        if (PickupObjectDatabase.Instance.Objects[i] != null && PickupObjectDatabase.Instance.Objects[i] is PlayerItem)
		        {
			        if (PickupObjectDatabase.Instance.Objects[i].quality != PickupObject.ItemQuality.EXCLUDED && PickupObjectDatabase.Instance.Objects[i].quality != PickupObject.ItemQuality.SPECIAL)
			        {
				        if (!(PickupObjectDatabase.Instance.Objects[i] is ContentTeaserItem))
				        {
					        if (Array.IndexOf<PickupObject.ItemQuality>(qualities, PickupObjectDatabase.Instance.Objects[i].quality) != -1)
					        {
						        if (!excludedIDs.Contains(PickupObjectDatabase.Instance.Objects[i].PickupObjectId))
						        {
							        EncounterTrackable component = PickupObjectDatabase.Instance.Objects[i].GetComponent<EncounterTrackable>();
							        if (component && component.PrerequisitesMet())
							        {
								        list.Add(PickupObjectDatabase.Instance.Objects[i] as PlayerItem);
							        }
						        }
					        }
				        }
			        }
		        }
	        }
	        int num = usedRandom.Next(list.Count);
	        if (num < 0 || num >= list.Count)
	        {
		        return null;
	        }
	        return list[num];
        }
        
        public static void GiveExtraItem(Chest chest)
        {
	        if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
	        {
		        int count = chest.contents.Count;
		        for (int i = 0; i < count; i++)
		        {
			        if (chest.contents[i].quality == PickupObject.ItemQuality.A ||
			            chest.contents[i].quality == PickupObject.ItemQuality.B ||
			            chest.contents[i].quality == PickupObject.ItemQuality.C ||
			            chest.contents[i].quality == PickupObject.ItemQuality.D ||
			            chest.contents[i].quality == PickupObject.ItemQuality.S)
			        {
				        if (chest.contents[i] is Gun)
				        {
					        chest.contents.Add(
						        PickupObjectDatabase.GetRandomGunOfQualities(
							        new Random(),
							        new List<int>(),
							        new PickupObject.ItemQuality[]
							        {
								        chest.contents[i].quality
							        }
						        )
					        );
				        }
				        else if (chest.contents[i] is PassiveItem)
				        {
					        chest.contents.Add(PickupObjectDatabase.GetRandomPassiveOfQualities(new Random(), new List<int>(), new PickupObject.ItemQuality[] {chest.contents[i].quality}));
				        }
				        else if (chest.contents[i] is PlayerItem)
				        {
					        chest.contents.Add(Module.GetRandomPlayerItemOfQualities(new Random(), new List<int>(), new PickupObject.ItemQuality[] {chest.contents[i].quality}));
				        }
			        }
			        else
			        {
				        chest.contents.Add(chest.contents[i]);
			        }
		        }
	        }
        }
        
        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
	        MethodInfo openMethod = typeof(Chest).GetMethod("Open", BindingFlags.NonPublic | BindingFlags.Instance);
	        MethodInfo openReplacement = typeof(Module).GetMethod("OpenHook", BindingFlags.NonPublic | BindingFlags.Static);
	        Hook openHook = new Hook(
		        openMethod, 
		        openReplacement
		    );
	        Hook OnBrokenHook = new Hook(
		        typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic),
		        typeof(Module).GetMethod("OnBrokenHook")
	        );
	        //Debug methods. Using an if statement we can determine which of the two GetMethods failed to find the
	        //method they should have.
	        MethodInfo method = typeof(RoomHandler).GetMethod("HandleBossClearReward", BindingFlags.Instance | BindingFlags.NonPublic);
	        MethodInfo method2 = typeof(Module).GetMethod("HandleBossClearRewardHook", BindingFlags.NonPublic | BindingFlags.Static);
	        /*
	        if (method!=null)
	        {
		        Log("Method Name: " + method.Name);
		        Log("Declaring Type: " + method.DeclaringType.FullName);
		        Log("Method Signature: " + method.ToString());
	        }
	        else
	        {
		        Log("Cringe");
	        }
	        if (method2 != null)
	        {
		        Log("Method Name: " + method2.Name);
		        Log("Declaring Type: " + method2.DeclaringType.FullName);
		        Log("Method Signature: " + method2.ToString());
	        }
	        else
	        {
		        Log("Cringe");
	        }
	        */
	        Hook HandleBossClearRewardHook = new Hook(
		        typeof(RoomHandler).GetMethod("HandleBossClearReward", BindingFlags.Instance | BindingFlags.NonPublic),
		        typeof(Module).GetMethod("HandleBossClearRewardHook", BindingFlags.NonPublic | BindingFlags.Static)
	        );
	        
	        Log("Gonna hook HandleRoomClearRewardHook", TEXT_COLOR);
	        Hook HandleRoomClearedHook = new Hook(
		        typeof(RoomHandler).GetMethod("HandleRoomClearReward", BindingFlags.Instance | BindingFlags.Public),
		        typeof(Module).GetMethod("HandleRoomClearRewardHook", BindingFlags.NonPublic | BindingFlags.Static)
	        );
            
            //ExamplePassive.Register();
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }
        
        public static void Log(string text, string color="#FFFFFF")
        {
	        ETGModConsole.Log($"<color={color}>{text}</color>");
        }
        
        //Hook functions
        //protected void Open(PlayerController player)
        private static void OpenHook(Action<Chest, PlayerController> orig, Chest self, PlayerController player)
        {
	        if (player != null)
	        {
		        self.GetRidOfBowler();
		        if (GameManager.Instance.InTutorial || self.AlwaysBroadcastsOpenEvent)
		        {
			        GameManager.BroadcastRoomTalkDoerFsmEvent("playerOpenedChest");
		        }

		        if (self.m_registeredIconRoom != null)
		        {
			        Minimap.Instance.DeregisterRoomIcon(self.m_registeredIconRoom, self.minimapIconInstance);
		        }

		        if (self.m_isGlitchChest)
		        {
			        if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			        {
				        PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
				        if (otherPlayer && otherPlayer.IsGhost)
				        {
					        self.StartCoroutine(self.GiveCoopPartnerBack(false));
				        }
			        }

			        GameManager.Instance.InjectedFlowPath = "Core Game Flows/Secret_DoubleBeholster_Flow";
			        Pixelator.Instance.FadeToBlack(0.5f, false, 0f);
			        GameManager.Instance.DelayedLoadNextLevel(0.5f);
			        return;
		        }

		        if (self.m_isMimic && !Chest.m_IsCoopMode)
		        {
			        self.DetermineContents(player, 0);
			        self.DoMimicTransformation(self.contents);
			        return;
		        }

		        if (self.ChestIdentifier == Chest.SpecialChestIdentifier.SECRET_RAINBOW)
		        {
			        self.RevealSecretRainbow();
		        }

		        self.pickedUp = true;
		        self.IsOpen = true;
		        self.HandleGeneratedMagnificence();
		        self.m_room.DeregisterInteractable(self);
		        MajorBreakable component = self.GetComponent<MajorBreakable>();
		        if (component != null)
		        {
			        component.usesTemporaryZeroHitPointsState = false;
		        }

		        if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER &&
		            GameManager.Instance.NumberOfLivingPlayers == 1 &&
		            self.ChestIdentifier == Chest.SpecialChestIdentifier.NORMAL)
		        {
			        self.spriteAnimator.Play(
							(!string.IsNullOrEmpty(self.overrideOpenAnimName))
					        ? self.overrideOpenAnimName
					        : self.openAnimName
			        );
			        self.m_isMimic = false;
			        self.StartCoroutine(self.GiveCoopPartnerBack(true));
		        }
		        else if (self.lootTable.CompletesSynergy)
		        {
			        self.StartCoroutine(self.HandleSynergyGambleChest(player));
		        }
		        else
		        {
			        self.DetermineContents(player, 0);
			        if (self.name.Contains("Chest_Red") && self.contents != null && self.contents.Count == 1 &&
			            self.contents[0] && self.contents[0].ItemSpansBaseQualityTiers)
			        {
				        self.contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.Key));
			        }

			        self.spriteAnimator.Play(
				        (!string.IsNullOrEmpty(self.overrideOpenAnimName))
					        ? self.overrideOpenAnimName
					        : self.openAnimName
			        );
			        AkSoundEngine.PostEvent("play_obj_chest_open_01", self.gameObject);
			        AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", self.gameObject);
			        if (!self.m_isMimic)
			        {
				        if (self.SubAnimators != null && self.SubAnimators.Length > 0)
				        {
					        for (int i = 0; i < self.SubAnimators.Length; i++)
					        {
						        self.SubAnimators[i].Play();
					        }
				        }
				        player.TriggerItemAcquisition();
				        if (!self.IsRainbowChest) //If this is not a rainbow chest then we can add an extra item to it
				        {
					        Module.GiveExtraItem(self);
				        }
				        self.StartCoroutine(self.PresentItem());
			        }
		        }
	        }
        }
        
        public static void OnBrokenHook(Action<Chest> orig, Chest self)
        {
            self.GetRidOfBowler();
			if (self.ChestIdentifier == Chest.SpecialChestIdentifier.SECRET_RAINBOW)
			{
				self.RevealSecretRainbow();
			}
			if (self.ChestIdentifier == Chest.SpecialChestIdentifier.SECRET_RAINBOW || self.IsRainbowChest || self.breakAnimName.Contains("redgold") || self.breakAnimName.Contains("black"))
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_GOLD_JUNK, true);
			}
			self.spriteAnimator.Play(string.IsNullOrEmpty(self.overrideBreakAnimName) ? self.breakAnimName : self.overrideBreakAnimName);
			self.specRigidbody.enabled = false;
			self.IsBroken = true;
			IntVector2 intVector = self.specRigidbody.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
			IntVector2 intVector2 = self.specRigidbody.UnitTopRight.ToIntVector2(VectorConversions.Floor);
			for (int i = intVector.x; i <= intVector2.x; i++)
			{
				for (int j = intVector.y; j <= intVector2.y; j++)
				{
					GameManager.Instance.Dungeon.data[new IntVector2(i, j)].isOccupied = false;
				}
			}
			if (self.LockAnimator != null && self.LockAnimator)
			{
				UnityEngine.Object.Destroy(self.LockAnimator.gameObject);
			}
			Transform transform = self.transform.Find("Shadow");
			if (transform != null)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			if (!self.pickedUp)
			{
				self.pickedUp = true;
				self.HandleGeneratedMagnificence();
				self.m_room.DeregisterInteractable(self);
				if (self.m_registeredIconRoom != null)
				{
					Minimap.Instance.DeregisterRoomIcon(self.m_registeredIconRoom, self.minimapIconInstance);
				}
				if (self.spawnAnimName.StartsWith("wood_"))
				{
					GameStatsManager.Instance.RegisterStatChange(TrackedStats.WOODEN_CHESTS_BROKEN, 1f);
				}
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1)
				{
					self.StartCoroutine(self.GiveCoopPartnerBack(false));
				}
				else
				{
					bool flag = PassiveItem.IsFlagSetAtAll(typeof(ChestBrokenImprovementItem));
					bool flag2 = GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_GOLD_JUNK);
					float num = GameManager.Instance.RewardManager.ChestDowngradeChance;
					float num2 = GameManager.Instance.RewardManager.ChestHalfHeartChance;
					float num3 = GameManager.Instance.RewardManager.ChestExplosionChance;
					float num4 = GameManager.Instance.RewardManager.ChestJunkChance;
					float num5 = (!flag2) ? 0f : 0.005f;
					bool flag3 = GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_SER_JUNKAN_UNLOCKED);
					float num6 = (!flag3 || Chest.HasDroppedSerJunkanThisSession) ? 0f : GameManager.Instance.RewardManager.ChestJunkanUnlockedChance;
					if (GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.carriedConsumables.KeyBullets > 0)
					{
						num4 *= GameManager.Instance.RewardManager.HasKeyJunkMultiplier;
					}
					if (SackKnightController.HasJunkan())
					{
						num4 *= GameManager.Instance.RewardManager.HasJunkanJunkMultiplier;
						num5 *= 3f;
					}
					if (self.IsTruthChest)
					{
						num = 0f;
						num2 = 0f;
						num3 = 0f;
						num4 = 1f;
					}
					num4 -= num5;
					float num7 = num5 + num + num2 + num3 + num4 + num6;
					float num8 = UnityEngine.Random.value * num7;
					if (flag2 && num8 < num5)
					{
						self.contents = new List<PickupObject>();
						int goldJunk = GlobalItemIds.GoldJunk;
						self.contents.Add(PickupObjectDatabase.GetById(goldJunk));
						self.m_forceDropOkayForRainbowRun = true;
						Module.GiveExtraItem(self);
						self.StartCoroutine(self.PresentItem());
					}
					else if (num8 < num || flag)
					{
						int tierShift = -4;
						bool flag4 = false;
						if (flag)
						{
							float value = UnityEngine.Random.value;
							if (value < ChestBrokenImprovementItem.PickupQualChance)
							{
								flag4 = true;
								self.contents = new List<PickupObject>();
								PickupObject pickupObject = null;
								while (pickupObject == null)
								{
									GameObject gameObject = GameManager.Instance.RewardManager.CurrentRewardData.SingleItemRewardTable.SelectByWeight(false);
									if (gameObject)
									{
										pickupObject = gameObject.GetComponent<PickupObject>();
									}
								}
								self.contents.Add(pickupObject);
								Module.GiveExtraItem(self);
								self.StartCoroutine(self.PresentItem());
							}
							else if (value < ChestBrokenImprovementItem.PickupQualChance + ChestBrokenImprovementItem.MinusOneQualChance)
							{
								tierShift = -1;
							}
							else if (value < ChestBrokenImprovementItem.PickupQualChance + ChestBrokenImprovementItem.EqualQualChance + ChestBrokenImprovementItem.MinusOneQualChance)
							{
								tierShift = 0;
							}
							else
							{
								tierShift = 1;
							}
						}
						if (!flag4)
						{
							self.DetermineContents(GameManager.Instance.PrimaryPlayer, tierShift);
							Module.GiveExtraItem(self);
							self.StartCoroutine(self.PresentItem());
						}
					}
					else if (num8 < num + num2)
					{
						self.contents = new List<PickupObject>();
						self.contents.Add(GameManager.Instance.RewardManager.HalfHeartPrefab);
						self.m_forceDropOkayForRainbowRun = true;
						Module.GiveExtraItem(self);
						self.StartCoroutine(self.PresentItem());
					}
					else if (num8 < num + num2 + num4)
					{
						bool flag5 = false;
						if (!Chest.HasDroppedSerJunkanThisSession && !flag3 && UnityEngine.Random.value < 0.2f)
						{
							Chest.HasDroppedSerJunkanThisSession = true;
							flag5 = true;
						}
						self.contents = new List<PickupObject>();
						int id = (self.overrideJunkId < 0) ? GlobalItemIds.Junk : self.overrideJunkId;
						if (flag5)
						{
							id = GlobalItemIds.SackKnightBoon;
						}
						self.contents.Add(PickupObjectDatabase.GetById(id));
						self.m_forceDropOkayForRainbowRun = true;
						Module.GiveExtraItem(self);
						self.StartCoroutine(self.PresentItem());
					}
					else if (num8 < num + num2 + num4 + num6)
					{
						Chest.HasDroppedSerJunkanThisSession = true;
						self.contents = new List<PickupObject>();
						self.contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.SackKnightBoon));
						Module.GiveExtraItem(self);
						self.StartCoroutine(self.PresentItem());
					}
					else
					{
						Exploder.DoDefaultExplosion(self.sprite.WorldCenter, Vector2.zero, null, false, CoreDamageTypes.None, false);
					}
				}
				for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
				{
					if (GameManager.Instance.AllPlayers[k].OnChestBroken != null)
					{
						GameManager.Instance.AllPlayers[k].OnChestBroken(GameManager.Instance.AllPlayers[k], self);
					}
				}
			}
        }

        private static void HandleBossClearRewardHook(Action<RoomHandler> orig, RoomHandler self)
        {
	        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
			{
				GameStatsManager.Instance.CurrentResRatShopSeed = UnityEngine.Random.Range(1, 1000000);
			}
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
			if (!self.PlayerHasTakenDamageInThisRoom && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
			{
				if (tilesetId != GlobalDungeonData.ValidTilesets.GUNGEON)
				{
					if (tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON)
					{
						if (tilesetId != GlobalDungeonData.ValidTilesets.MINEGEON)
						{
							if (tilesetId != GlobalDungeonData.ValidTilesets.CATACOMBGEON)
							{
								if (tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
								{
									GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_FORGE, true);
								}
							}
							else
							{
								GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_HOLLOW, true);
							}
						}
						else
						{
							GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_MINES, true);
						}
					}
					else
					{
						GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_CASTLE, true);
					}
				}
				else
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_GUNGEON, true);
				}
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
			{
				return;
			}
			if (tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON)
			{
				return;
			}
			if (tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
			{
				return;
			}
			for (int i = 0; i < self.connectedRooms.Count; i++)
			{
				if (self.connectedRooms[i].area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.EXIT)
				{
					self.connectedRooms[i].OnBecameVisible(GameManager.Instance.BestActivePlayer);
				}
			}
			IntVector2 intVector = IntVector2.Zero;
			if (self.OverrideBossPedestalLocation != null)
			{
				intVector = self.OverrideBossPedestalLocation.Value;
			}
			else if (!self.area.IsProceduralRoom && self.area.runtimePrototypeData.rewardChestSpawnPosition != IntVector2.NegOne)
			{
				intVector = self.area.basePosition + self.area.runtimePrototypeData.rewardChestSpawnPosition;
			}
			else
			{
				UnityEngine.Debug.LogWarning("BOSS REWARD PEDESTALS SHOULD REALLY HAVE FIXED LOCATIONS! The spawn code was written by dave, so no guarantees...");
				intVector = self.GetCenteredVisibleClearSpot(2, 2);
			}
			GameObject gameObject = GameManager.Instance.Dungeon.sharedSettingsPrefab.ChestsForBosses.SelectByWeight();
			Chest chest = gameObject.GetComponent<Chest>();
			bool isRainbowRun = GameStatsManager.Instance.IsRainbowRun;
			if (isRainbowRun)
			{
				chest = null;
			}
			if (chest != null)
			{
				Chest chest2 = Chest.Spawn(chest, intVector, self, false);
				chest2.RegisterChestOnMinimap(self);
			}
			else
			{
				DungeonData data = GameManager.Instance.Dungeon.data;
				RewardPedestal component = gameObject.GetComponent<RewardPedestal>();
				if (component)
				{
					bool flag = tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON;
					bool flag2 = !self.PlayerHasTakenDamageInThisRoom && GameManager.Instance.Dungeon.BossMasteryTokenItemId >= 0 && !GameManager.Instance.Dungeon.HasGivenMasteryToken;
					if (flag && flag2) //Player has not taken damage yet
					{
						intVector += IntVector2.Left;
						//MOD: If player is in co-op
					}
					if (flag2 && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
					{
						intVector += IntVector2.Left;
						//Explanation:
						//So basically the two flags figure out first the stage and if the player hasn't taken any damage from this boss.
						//And also the mastery token hasn't been awarded yet.
						//If so then prepare to spawn the reward next to the existing pedestal. To do that we need a vector direction to spawn it in
						//But we do it twice if we're in co-op because there's twice the reward. If we only stepped once to the left then
						//we would clip through the extra rewards that may be in this position. (maybe not, but don't risk  it).
						//You can see us spawn the reward pedestal below.
					}
					if (flag)
					{
						RewardPedestal rewardPedestal = RewardPedestal.Spawn(component, intVector, self);
						rewardPedestal.IsBossRewardPedestal = true;
						rewardPedestal.lootTable.lootTable = self.OverrideBossRewardTable;
						rewardPedestal.RegisterChestOnMinimap(self);
						data[intVector].isOccupied = true;
						data[intVector + IntVector2.Right].isOccupied = true;
						data[intVector + IntVector2.Up].isOccupied = true;
						data[intVector + IntVector2.One].isOccupied = true;
						if (flag2)
						{
							rewardPedestal.OffsetTertiarySet = true;
						}
						if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1)
						{
							rewardPedestal.ReturnCoopPlayerOnLand = true;
						}
						if (self.area.PrototypeRoomName == "DoubleBeholsterRoom01")
						{
							for (int j = 0; j < 8; j++)
							{
								IntVector2 centeredVisibleClearSpot = self.GetCenteredVisibleClearSpot(2, 2);
								RewardPedestal rewardPedestal2 = RewardPedestal.Spawn(component, centeredVisibleClearSpot, self);
								rewardPedestal2.IsBossRewardPedestal = true;
								rewardPedestal2.lootTable.lootTable = self.OverrideBossRewardTable;
								data[centeredVisibleClearSpot].isOccupied = true;
								data[centeredVisibleClearSpot + IntVector2.Right].isOccupied = true;
								data[centeredVisibleClearSpot + IntVector2.Up].isOccupied = true;
								data[centeredVisibleClearSpot + IntVector2.One].isOccupied = true;
							}
						}
					}
					else if (tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1)
					{
						PlayerController playerController = (!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) ? GameManager.Instance.SecondaryPlayer : GameManager.Instance.PrimaryPlayer;
						playerController.specRigidbody.enabled = true;
						playerController.gameObject.SetActive(true);
						playerController.ResurrectFromBossKill();
					}
					if (flag2)
					{
						GameStatsManager.Instance.RegisterStatChange(TrackedStats.MASTERY_TOKENS_RECEIVED, 1f);
						GameManager.Instance.PrimaryPlayer.MasteryTokensCollectedThisRun++;
						if (flag)
						{
							intVector += new IntVector2(2, 0);
						}
						RewardPedestal rewardPedestal3 = RewardPedestal.Spawn(component, intVector, self);
						data[intVector].isOccupied = true;
						data[intVector + IntVector2.Right].isOccupied = true;
						data[intVector + IntVector2.Up].isOccupied = true;
						data[intVector + IntVector2.One].isOccupied = true;
						GameManager.Instance.Dungeon.HasGivenMasteryToken = true;
						rewardPedestal3.SpawnsTertiarySet = false;
						rewardPedestal3.contents = PickupObjectDatabase.GetById(GameManager.Instance.Dungeon.BossMasteryTokenItemId);
						rewardPedestal3.MimicGuid = null;
						//MOD: Create another reward pedestal if you are in co-op. One extra for the other player.
						//So this code just copies the reward pedestal code above, but the number is 4 now.
						//We forego the flag check which is: "Check if not in FORGEGEON". Hopefully that's okay?
						if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
						{
							intVector += new IntVector2(2, 0);
							RewardPedestal rewardPedestal4 = RewardPedestal.Spawn(component, intVector, self);
							data[intVector].isOccupied = true;
							data[intVector + IntVector2.Right].isOccupied = true;
							data[intVector + IntVector2.Up].isOccupied = true;
							data[intVector + IntVector2.One].isOccupied = true;
							GameManager.Instance.Dungeon.HasGivenMasteryToken = true;
							rewardPedestal4.SpawnsTertiarySet = false;
							rewardPedestal4.contents = PickupObjectDatabase.GetById(GameManager.Instance.Dungeon.BossMasteryTokenItemId);
							rewardPedestal4.MimicGuid = null;
						}
					}
				}
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATHEDRALGEON && GameManager.Options.CurrentGameLootProfile == GameOptions.GameLootProfile.CURRENT)
				{
					IntVector2? randomAvailableCell = self.GetRandomAvailableCell(new IntVector2?(IntVector2.One * 4), new CellTypes?(CellTypes.FLOOR), false, null);
					IntVector2? intVector2 = (randomAvailableCell == null) ? null : new IntVector2?(randomAvailableCell.GetValueOrDefault() + IntVector2.One);
					if (intVector2 != null)
					{
						Chest chest3 = Chest.Spawn(GameManager.Instance.RewardManager.Synergy_Chest, intVector2.Value);
						if (chest3)
						{
							chest3.RegisterChestOnMinimap(self);
						}
					}
				}
			}
        }
        
        private static void HandleRoomClearRewardHook(Action<RoomHandler> orig, RoomHandler self)
        {
            if (GameManager.Instance.IsFoyer || GameManager.Instance.InTutorial)
			{
				return;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
			{
				return;
			}
			if (self.m_hasGivenReward)
			{
				return;
			}
			if (self.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD)
			{
				return;
			}
			self.m_hasGivenReward = true;
			if (self.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && self.area.PrototypeRoomBossSubcategory == PrototypeDungeonRoom.RoomBossSubCategory.FLOOR_BOSS)
			{
				self.HandleBossClearReward();
				return;
			}
			if (self.PreventStandardRoomReward)
			{
				return;
			}
			FloorRewardData currentRewardData = GameManager.Instance.RewardManager.CurrentRewardData;
			LootEngine.AmmoDropType ammoDropType = LootEngine.AmmoDropType.DEFAULT_AMMO;
			bool flag = LootEngine.DoAmmoClipCheck(currentRewardData, out ammoDropType);
			string path = (ammoDropType != LootEngine.AmmoDropType.SPREAD_AMMO) ? "Ammo_Pickup" : "Ammo_Pickup_Spread";
			float value = UnityEngine.Random.value;
			float num = currentRewardData.ChestSystem_ChestChanceLowerBound;
			float num2 = GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.Coolness) / 100f;
			float num3 = -(GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.Curse) / 100f);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num2 += GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.Coolness) / 100f;
				num3 -= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.Curse) / 100f;
			}
			if (PassiveItem.IsFlagSetAtAll(typeof(ChamberOfEvilItem)))
			{
				num3 *= -2f;
			}
			num = Mathf.Clamp(num + GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance, currentRewardData.ChestSystem_ChestChanceLowerBound, currentRewardData.ChestSystem_ChestChanceUpperBound) + num2 + num3;
			bool flag2 = currentRewardData.SingleItemRewardTable != null;
			bool flag3 = false;
			float num4 = 0.1f;
			if (!RoomHandler.HasGivenRoomChestRewardThisRun && MetaInjectionData.ForceEarlyChest)
			{
				flag3 = true;
			}
			if (flag3)
			{
				if (!RoomHandler.HasGivenRoomChestRewardThisRun && (GameManager.Instance.CurrentFloor == 1 || GameManager.Instance.CurrentFloor == -1))
				{
					flag2 = false;
					num += num4;
					if (GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.NumRoomsCleared > 4)
					{
						num = 1f;
					}
				}
				if (!RoomHandler.HasGivenRoomChestRewardThisRun && self.distanceFromEntrance < RoomHandler.NumberOfRoomsToPreventChestSpawning)
				{
					GameManager.Instance.Dungeon.InformRoomCleared(false, false);
					return;
				}
			}
			BraveUtility.Log("Current chest spawn chance: " + num, Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num *= 1.5f;
				//MOD: "num" determines the chances of a chest spawning. if it is 1.5 times bigger then the chance has gone up by 50%.
				//Could tweak if necessary.
			}
			if (value > num)
			{
				if (flag)
				{
					IntVector2 bestRewardLocation = self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
					LootEngine.SpawnItem((GameObject)BraveResources.Load(path, ".prefab"), bestRewardLocation.ToVector3(), Vector2.up, 1f, true, true, false);
				}
				GameManager.Instance.Dungeon.InformRoomCleared(false, false);
				return;
			}
			if (flag2)
			{
				float num5 = currentRewardData.PercentOfRoomClearRewardsThatAreChests;
				if (PassiveItem.IsFlagSetAtAll(typeof(AmazingChestAheadItem)))
				{
					num5 *= 2f;
					num5 = Mathf.Max(0.5f, num5);
				}
				flag2 = (UnityEngine.Random.value > num5);
			}
			if (flag2)
			{
				float num6 = (GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? GameManager.Instance.RewardManager.SinglePlayerPickupIncrementModifier : GameManager.Instance.RewardManager.CoopPickupIncrementModifier;
				GameObject gameObject;
				if (UnityEngine.Random.value < 1f / num6)
				{
					gameObject = currentRewardData.SingleItemRewardTable.SelectByWeight(false);
				}
				else
				{
					gameObject = ((UnityEngine.Random.value >= 0.9f) ? GameManager.Instance.RewardManager.FullHeartPrefab.gameObject : GameManager.Instance.RewardManager.HalfHeartPrefab.gameObject);
				}
				UnityEngine.Debug.Log(gameObject.name + "SPAWNED");
				DebrisObject debrisObject = LootEngine.SpawnItem(gameObject, self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true).ToVector3() + new Vector3(0.25f, 0f, 0f), Vector2.up, 1f, true, true, false);
				Exploder.DoRadialPush(debrisObject.sprite.WorldCenter.ToVector3ZUp(debrisObject.sprite.WorldCenter.y), 8f, 3f);
				AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", debrisObject.gameObject);
				GameManager.Instance.Dungeon.InformRoomCleared(true, false);
			}
			else
			{
				IntVector2 bestRewardLocation = self.GetBestRewardLocation(new IntVector2(2, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
				bool isRainbowRun = GameStatsManager.Instance.IsRainbowRun;
				if (isRainbowRun)
				{
					LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteChest, bestRewardLocation.ToCenterVector2(), self, true);
					RoomHandler.HasGivenRoomChestRewardThisRun = true;
				}
				else
				{
					Chest exists = self.SpawnRoomRewardChest(null, bestRewardLocation);
					if (exists)
					{
						RoomHandler.HasGivenRoomChestRewardThisRun = true;
					}
				}
				GameManager.Instance.Dungeon.InformRoomCleared(true, true);
			}
			if (flag)
			{
				IntVector2 bestRewardLocation = self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
				LootEngine.DelayedSpawnItem(1f, (GameObject)BraveResources.Load(path, ".prefab"), bestRewardLocation.ToVector3() + new Vector3(0.25f, 0f, 0f), Vector2.up, 1f, true, true, false);
			}
        }
    }
}
