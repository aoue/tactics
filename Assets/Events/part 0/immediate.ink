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

/*
Here's how you do an example choice stuff:
-> example_choices
=== example_choices ===
lalala, back at the top of example choices.
* [Choice 1] -> label1
* [Choice 2] -> label2
* [Choice 3] -> label3
=== label1 ===
you clicked choice 1.
-> done_choices
=== label2 ===
you clicked choice 2.
-> done_choices
=== label3 ===
you clicked choice 3.
-> done_choices
=== done_choices ===
And now we are in done_choices, continuing as normal.
Let's end.
->END
*/

~play_music(0)
~bg(0, 1, "BG NAME")
~show(3, 100)
~show(4, 200)
setup.
skip me 1
skip me 2
skip me 3
skip me 4
skip me 5
skip me 6
skip me 7
skip me 8
end.

-> END

~bg(1, 1, "BG NAME")
second bg switch from prev!

~n("Friday")
~p(100)
~speakerglow(3)
~v_wiggle(3, 0.1, 1)
I'm talking a little bit.

~bg(1, 1, "BG NAME")
~n("Anse")
~p(200)
~speakerglow(4)
And now I'm talking.
lalala.

-> END
