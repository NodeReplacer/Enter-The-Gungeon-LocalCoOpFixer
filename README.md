# Introduction

Enter the Gungeon has an issue with its co-op but it's innocently done. The logic goes like this:

1. If there are two players there is more damage coming out so we need to balance the game to maintain the challenge.
2. We can't raise enemy damage because the player only has 6 health, so taking two health per hit will devastate any run.
3. Therefore we will raise enemy health (by 1.4 times).
4. Also we will need to implement some sort of revive mechanic so one player isn't just gone forever.
5. We'll use the chests to respawn players because there should be **some** punishment for dying.

The issue arises in practice: 
1. Chests still only give one item, but enemies have more health.
2. So you use up more ammo to defeat them, but only have the resources meant for one player.
3. Room rewards are also still balanced for one player so buying from the shop
4. This results in both players using their (awful) default guns almost all the way throughout the entire game except on bosses. In a game about interesting guns, the players cannot use guns.
5. Dying replaces chests with a "rescue" chest that respawns the dead player. Further depriving players of guns/resources.
6. Enemies have more random attack patterns based on who they are targeting. Furthermore if the players split up both will have less space to dodge (in local co-op). Both points making it more likely that a player will get hit. 
7. As a result of these points the fundamental concept of the game (using fun weapons and dodging bullets) is compromised. The players have fewer fun weapons (due to them running out of ammmo) and may have a harder time dodging (some of the bosses' movement trajectories make wide circles around their targeted player. Fine for one player but with limited screen space this is guaranteed to get in the way of the other one unless they stick together.)

# Mod Features

This mod attempts to address this by:
- Doubling all chest rewards. They will drop two items instead of one (except rainbow chests). This might result in the same item but usually won't, as the drop table is rolled separately for each item.
- Increase chance of room rewards by 1.5 times. 2 times was out of control. At 1.5 you will have enough money to keep up with buying keys and maybe you will get more but that's more luck dependent. This may make things a bit easier, though I found that even with both characters capable of using guns, the 1.4 health increase for enemies kept things in check.
