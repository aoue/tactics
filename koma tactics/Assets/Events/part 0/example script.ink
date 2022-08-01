//Visual Effects:
EXTERNAL bg(id) //sets background to corresponding element from BackgroundManager
EXTERNAL n(name) //sets name text, or call with empty string to hide namebox.
EXTERNAL p(pId) //pId to show corresponding BOX portrait. -1 to hide.
EXTERNAL show(whichSlot, portraitID) //shows full portrait
EXTERNAL hide(whichSlot) //hides full portrait
EXTERNAL talk(mode) //1: use quotes | 0: use parantheses.
EXTERNAL toggle_font()
EXTERNAL shake(intensity, duration) //camera shake. both parameters are ints. actual duration = 0.05f seconds * duration

//Music:
EXTERNAL stop_music()
EXTERNAL play_music(id)
EXTERNAL play_sound(id)

//Party Manipulation:
EXTERNAL add_unit(id) //adds unit to reserve party pool based on unit id.
EXTERNAL remove_unit(id) //removes unit from reserve party pool based on unit id.
//end of external functions

//SCENE OUTLINE
//This is the base script that you should start every scene's script from.

~n("")
example script.
end.
-> END
