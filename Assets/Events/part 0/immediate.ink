EXTERNAL stop_music()
EXTERNAL play_music(id) //plays track, looping
EXTERNAL play_sound(id) //plays sound, once.

EXTERNAL imm_bg(id) //sets background to corresponding element from BackgroundManager without fade. Use for first bg.
EXTERNAL bg(id) //sets background to corresponding element from BackgroundManager
EXTERNAL overlay(id) //sets overlay to corresponding element from BackgroundManager

EXTERNAL snow(strength) //turns weather on or off, and sets the particle count*5. -1: off, otherwise, creates strength*12 particles. Also turns off rain.
EXTERNAL rain(strength) //turns weather on or off, and sets the particle count*5. -1: off, otherwise, Needs higher strength than snow. Also turns off snow. 
EXTERNAL wind(strength) //turns wind on or off. -1 to turn off, anything else to turn on.
EXTERNAL shake(intensity, duration) //Both parameters are ints. Don't be shy with intensity. 20 is moderate, 50 is heavy. Duration is in seconds.
EXTERNAL inside(wait) //reorders characters so that they appear before overlay and weather. Enter 0 at start, or 1 if switching with a bg call.
EXTERNAL outside(wait) //reorders characters so that they appear behind overlay and weather. Enter 0 at start, or 1 if switching with a bg call.
EXTERNAL font(state) //switches to robot font. For sentence text only. '-1' to turn off, anything else to turn on.

EXTERNAL center(state) //1: turns on. 0: turns off
EXTERNAL n(name) //sets name text, or call with empty string to hide namebox.
EXTERNAL c(id) //id is a string. sets colour of name text and sentence text. Input as a string, e.g. "red"

EXTERNAL p(pId) //pId to show corresponding BOX portrait. -1 to hide.
EXTERNAL p_holo(pId) //turns hologram shader on to the name sprite. -1 to turn off.
EXTERNAL show(whichSlot, portraitID) //shows full portrait
EXTERNAL holo(whichSlot, state) //applies hologram shade to portrait slot. Instant. -1 to turn off.
EXTERNAL speakerglow(whichSlot) //darkens all portrait slots except for whichSlot, making them seem brighter. -1 to restore everyone to full.
EXTERNAL hide(whichSlot) //hides full portrait
EXTERNAL v_wiggle(whichSlot, power, repeats) //causes the character portrait in the specified slot to wiggle vertically. Moves power percentage. Repeats x times.
EXTERNAL h_wiggle(whichSlot, power, repeats) //causes the character portrait in the specified slot to wiggle horizontally. Moves power percentage. Repeats x times.
EXTERNAL program(name, duration) // runs program popup. Name is text displayed above bar. Duration is in seconds.

EXTERNAL unit_state(unit_id, val) //sets the availability of units in the level tree. 0: fully, 1: visible but not clickable, 2: not visible or clickable
EXTERNAL inc_stat(unit_id, stat_id, val) //increases the stat of a unit by val.

//SCENE OUTLINE
//This is the base script that you should start every scene's script from.
//Remember to show the first bg with an imm_bg() call.
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

//temporary: for testing mission 1 directly.

~unit_state(0, 0)
~unit_state(1, 0)
~unit_state(2, 2)
//note: you will be setting all of the unit states here, too.

//~inc_stat(0, 0, 50)
//~inc_stat(0, 3, 0.05)


~play_music(0)
~imm_bg(3)
~outside(0)
~show(3, 100)
~show(4, 200)
~v_wiggle(3, 0.1, 1)
//vertical wiggle or nod, yep.

Suddenly a voice is heard from a speaker overhead.


~n("Friday")
~p(100)
~speakerglow(3)
I'm talking a little bit.

~n("Anse")
~p(200)
~speakerglow(4)
And now I'm talking.

~p(-1)
~speakerglow(-1)
And now nobody is talking...

~n("")
~p(-1)
Okay, end now.

-> END

~bg(2)
Let's try swtiching bg,.

~n("")
~p(-1)
Suddenly a voice is heard from a speaker overhead.

~n("Overhead Speaker")
~font(1)
Welcome to the Aventine. Please enjoy your stay.

~n("")
~font(-1)
...What a way to ruin the mood.

->END

~c("red")
This is red.

~c("orange")
This is orange.

~c("yellow")
This is yellow.

~c("cyan")
This is cyan.

~c("blue")
This is blue.



~c("pink")
This is pink.

~c("grey")
This is grey.

~c("white")
And finally... this is white again.

-> END
