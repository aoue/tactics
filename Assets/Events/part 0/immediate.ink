EXTERNAL stop_music()
EXTERNAL play_music(id) //plays track, looping
EXTERNAL play_sound(id) //plays sound, once.

EXTERNAL bg(id, duration, text) //sets background to corresponding element from BackgroundManager

EXTERNAL snow(strength) //turns snow on or off, higher arg for more particles. -1 to turn off. Also turns off rain.
EXTERNAL rain(strength) //turns rain on or off, higher arg for more particles. -1 to turn off. Also turns off snow. 
EXTERNAL wind(strength) //turns wind on or off, higher arg for more particles. -1 to turn off.
EXTERNAL shake(intensity, duration) //Both parameters are ints. Don't be shy with intensity. 20 is moderate, 50 is heavy. Duration is in seconds.

EXTERNAL place(label) //move the textbox, also sets nvl on/off.
EXTERNAL colour(c) //pass in a character's name or "" for default. Basically, just check the labels in EventManager's set_colour() function.
//EXTERNAL textbox(type) //used to switch the sprite of the textbox between normal, yell, whisper, etc.
EXTERNAL voice(label) //plays a character voice sound. Call into it with a label like '[character]-[mood]' and it plays a random voice clip that matches.


EXTERNAL glow(whichSlot) //speakerglow. darkens all portrait slots except for whichSlot, making them seem brighter. -1 to restore everyone to full.
EXTERNAL p(pId) //pId to show corresponding BOX portrait. -1 to hide.
EXTERNAL p_holo(pId) //turns hologram shader on to the name sprite. -1 to turn off.
EXTERNAL show(whichSlot, portraitID) //shows full portrait
EXTERNAL holo(whichSlot, state) //applies hologram shade to portrait slot. Instant. -1 to turn off.
EXTERNAL hide(whichSlot) //hides full portrait
EXTERNAL v_wiggle(whichSlot, power, repeats) //int, float, repeats; causes the character portrait in the specified slot to wiggle vertically. Moves power units. Repeats x times.
EXTERNAL h_wiggle(whichSlot, power, repeats) //int, float, repeats; causes the character portrait in the specified slot to wiggle horizontally. Moves power units. Repeats x times.
EXTERNAL program(name, duration) // runs program popup. Name is text displayed above bar. Duration is in seconds.

//SCENE OUTLINE
//This is the base script that you should start every scene's script from.
//Calling inside/outside with wait=-1 also at start. Otherwise, give it any other argument.

/*
~bg(0, 1, "BG NAME")
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

~bg(0, 1, "BG NAME")
~show(1, 100) // friday at left
~show(3, 200) // anse at right

~place("bottom-narration")
We all stand there in the aftermath.

~glow(1)
~colour("friday")
~voice("friday-whatever")
... Thank you. I have to leave now.

~glow(-1)
~colour("")
I'm momentarily shocked into silence.

~glow(3)
~place("center-right-chin")
~voice("anse-whatever")
~colour("anse")
Huh? What are you even talking about, miss?

I'm not after a wall that'll repel power coming from outside. What I want is the kind of strength to be able to absorb that outside power, to stand up to it. The strength to quietly endure things—unfairness, misfortune, sadness, mistakes, misunderstandings. 

~place("nvl")
There is something fundamentally important and distinctly human about taking a stance and engaging with the world on a deeper level than simple animal reward seeking behavior.
While the poet themself should be a respectable person, their poetry should not be constrained.
You've built it before, they've built it before. Hasn't really worked out yet, but neither has love. Should we stop building love?
Just because you've apologized, you think you have a right to be forgiven?

~place("nvl")
I hope this cleared nvl man!

~place("bottom-narration")
... What does it even matter.
-> END

~p(200)
... Who the hell are you?
Like, what?

~p(100)
Oh, I thought you... nevermind.
Forget I said anything.
-> END

~bg(1, 1, "BG NAME")
second bg switch from prev!

~p(100)
~v_wiggle(3, 0.1, 1)
I'm talking a little bit.

~bg(1, 1, "BG NAME")
~p(200)
And now I'm talking.
lalala.

-> END
