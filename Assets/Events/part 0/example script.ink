EXTERNAL stop_music()
EXTERNAL play_music(id) //plays track, looping
EXTERNAL play_sound(id) //plays sound, once.

EXTERNAL bg(id, duration, text) //sets background to corresponding element from BackgroundManager
EXTERNAL thinkbg(id) //turns thinking background on (the top bar). fades it in. -1 to turn off, anything else to turn on.

EXTERNAL snow(strength) //turns snow on or off, higher arg for more particles. -1 to turn off. Also turns off rain.
EXTERNAL rain(strength) //turns rain on or off, higher arg for more particles. -1 to turn off. Also turns off snow. 
EXTERNAL wind(strength) //turns wind on or off, higher arg for more particles. -1 to turn off.
EXTERNAL shake(intensity, duration) //Both parameters are ints. Don't be shy with intensity. 20 is moderate, 50 is heavy. Duration is in seconds.

EXTERNAL n(name) //sets name text, or call with empty string to hide namebox.
EXTERNAL c(id) //id is a string. sets colour of name text and sentence text. Input as a string, e.g. "red"

EXTERNAL p(pId) //pId to show corresponding BOX portrait. -1 to hide.
EXTERNAL p_holo(pId) //turns hologram shader on to the name sprite. -1 to turn off.
EXTERNAL show(whichSlot, portraitID) //shows full portrait
EXTERNAL holo(whichSlot, state) //applies hologram shade to portrait slot. Instant. -1 to turn off.
EXTERNAL speakerglow(whichSlot) //darkens all portrait slots except for whichSlot, making them seem brighter. -1 to restore everyone to full.
EXTERNAL hide(whichSlot) //hides full portrait
EXTERNAL v_wiggle(whichSlot, power, repeats) //int, float, repeats; causes the character portrait in the specified slot to wiggle vertically. Moves power units. Repeats x times.
EXTERNAL h_wiggle(whichSlot, power, repeats) //int, float, repeats; causes the character portrait in the specified slot to wiggle horizontally. Moves power units. Repeats x times.
EXTERNAL program(name, duration) // runs program popup. Name is text displayed above bar. Duration is in seconds.

//SCENE OUTLINE
//This is the base script that you should start every scene's script from.
//Calling inside/outside with wait=-1 also at start. Otherwise, give it any other argument.

~n("")
~bg(1, 1, "BG NAME TWO")
test scene launch.
end.
-> END
