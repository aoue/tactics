EXTERNAL imm_bg(id) //sets background to corresponding element from BackgroundManager without fade. Use for first bg.
EXTERNAL bg(id) //sets background to corresponding element from BackgroundManager
EXTERNAL overlay(id) //sets overlay to corresponding element from BackgroundManager
EXTERNAL snow(strength) //turns weather on or off, and sets the particle count*5. -1: off, otherwise, creates strength*12 particles. Also turns off rain.
EXTERNAL rain(strength) //turns weather on or off, and sets the particle count*5. -1: off, otherwise, Needs higher strength than snow. Also turns off snow. 
EXTERNAL wind(strength) //turns wind on or off. -1 to turn off, anything else to turn on.
EXTERNAL shake(intensity, duration) //Both parameters are ints. Don't be shy with intensity. 20 is moderate, 50 is heavy. Duration is in seconds.
EXTERNAL inside(wait) //reorders characters so that they appear before overlay and weather. Enter 0 at start, or 1 if switching with a bg call.
EXTERNAL outside(wait) //reorders characters so that they appear behind overlay and weather. Enter 0 at start, or 1 if switching with a bg call.

EXTERNAL center(state) //1: turns on. 0: turns off
EXTERNAL n(name) //sets name text, or call with empty string to hide namebox.
EXTERNAL c(id) //id is a string. sets colour of name text and sentence text. Input as a string, e.g. "red"

EXTERNAL p(pId) //pId to show corresponding BOX portrait. -1 to hide.
EXTERNAL p_holo(pId) //turns hologram shader on to the name sprite. -1 to turn off.
EXTERNAL show(whichSlot, portraitID) //shows full portrait
EXTERNAL holo(whichSlot, state) //applies hologram shade to portrait slot. Instant. -1 to turn off.
EXTERNAL speaker(whichSlot, state) //SUSPENDED; applies the speaker effect to the portrait slot. -1  to turn off.
EXTERNAL hide(whichSlot) //hides full portrait
EXTERNAL v_wiggle(whichSlot, power, repeats) //causes the character portrait in the specified slot to wiggle vertically. Moves power percentage. Repeats x times.
EXTERNAL h_wiggle(whichSlot, power, repeats) //causes the character portrait in the specified slot to wiggle horizontally. Moves power percentage. Repeats x times.
EXTERNAL msg_popup(name) //causes a message popup to slide in, then fade out. Displays something like: 'Message request from [name]'
EXTERNAL msg_popup_hide() //hides it.

EXTERNAL stop_music()
EXTERNAL play_music(id) //plays track, looping
EXTERNAL play_sound(id) //plays sound, once.

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

~inc_stat(0, 0, 50)
~inc_stat(0, 3, 0.05)

~play_music(0)
~imm_bg(3)
~outside(0)

~show(3, 000)
~show(4, 100)



~v_wiggle(3, 0.1, 1)
//vertical wiggle or nod, yep.

~n("Friday")
~p(000)
I'm talking a little bit.
> I hope that no extra set of quotation marks was added.

~n("")
~p(-1)
end of test.
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

~msg_popup("Yvette")
testing msg popup control a second time smilew


-> END
