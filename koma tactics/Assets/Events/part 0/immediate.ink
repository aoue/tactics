//Link external functions here.
//VN:
EXTERNAL bg(id)
EXTERNAL n(name) //set to empty string to hide namebox.
EXTERNAL p(pId) //pId to show corresponding portrait. -1 to hide.
EXTERNAL talk(mode) //1: use quotes | 0: use parantheses.
EXTERNAL toggle_font()
EXTERNAL show(whichSlot, portraitID)
EXTERNAL hide(whichSlot)
EXTERNAL stop_music()
EXTERNAL play_music(whichTrack)
EXTERNAL play_sound(whichTrack)
EXTERNAL shake(intensity, duration) //camera shake. both parameters are ints. actual duration = 0.05f seconds * duration
//Combat:
EXTERNAL battle(id) //starts battle corresponding to the given id, and stops at the pdm on the way.
EXTERNAL battle_no_prep(id) //starts battle corresponding to the given id. No stop at the pdm; preset deployment.
EXTERNAL add_unit(id) //adds unit to reserve party pool based on unit id.
EXTERNAL remove_unit(id) //removes unit from reserve party pool based on unit id.
EXTERNAL rest_party() //fully restore hp and mp of all party units.
//end of external functions

//variable controllers here. set by EventManager at scene start.
VAR ic = 0
VAR pname = "Pax"
//end variables

//SCENE OUTLINE
~rest_party()
~add_unit(0)
~add_unit(1)
~add_unit(2)

~talk(1)
~n("")
p0 imm.
ok, a battle!

//these two lines: first one sets up the battle, second one is the last line shown before the battle.
//On the next proceed command, the battle is launched.
~battle(0)
battle with prep!

~n("")
YOU SHOULDN'T SEE THIS UNTIL BATTLE OVER.
battle over. There won't be another one, I'm sure.

~battle(1)
Oh frick, a battle!

battle over.
scene over.

-> END
