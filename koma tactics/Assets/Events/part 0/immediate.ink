//Visual Effects:
EXTERNAL imm_bg(id) //sets background to corresponding element from BackgroundManager without fade. Use for first bg.
EXTERNAL bg(id) //sets background to corresponding element from BackgroundManager
EXTERNAL overlay(id) //sets overlay to corresponding element from BackgroundManager
EXTERNAL snow(strength) //turns weather on or off, and sets the particle count*5. -1: off, otherwise, creates strength*12 particles. Also turns off rain.
EXTERNAL rain(strength) //turns weather on or off, and sets the particle count*5. -1: off, otherwise, Needs higher strength than snow. Also turns off snow. 
EXTERNAL wind(strength) //turns wind on or off. -1 to turn off, anything else to turn on.
EXTERNAL center(state) //1: turns on. 0: turns off
EXTERNAL n(name) //sets name text, or call with empty string to hide namebox.
EXTERNAL p(pId) //pId to show corresponding BOX portrait. -1 to hide.
EXTERNAL show(whichSlot, portraitID) //shows full portrait
EXTERNAL hide(whichSlot) //hides full portrait
EXTERNAL toggle_font()
EXTERNAL shake(intensity, duration) //Both parameters are ints. Don't be shy with intensity. 20 is moderate, 50 is heavy. Duration is in seconds.
EXTERNAL inside(wait) //reorders characters so that they appear before overlay and weather
EXTERNAL outside(wait) //reorders characters so that they appear behind overlay and weather

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
//Remember to show the first bg with an imm_bg() call.
//Calling inside/outside with wait=-1 also at start. Otherwise, give it any other argument.

~add_unit(0)
~add_unit(1)
~add_unit(2)

//temporary: for testing mission 1 directly.

~play_music(0)
~imm_bg(3)
~outside(0)
~snow(40)
~show(4, 000)
~show(3, 300)
...
No one says anything as the snow falls around us.
It's cold, and though I feel the urge to shiver, to move at all for warmth, it's overpowered by the far greater need to stay silent.
~p(000)
...


-> END

~hide(1)
~show(5, 300)
~n("Biter")
Ha!

~n("Me")
~p(100)
He's not hurting anyone is he?

~show(3, 002)
~p(002)
~n("Friday")
I'm trying to concentrate. It's hard enough already.
You keep quiet too.

~n("")
~p(-1)
...Goddamnit.


-> END


~n("")
~p(-1)
Going inside.

~bg(1)
~overlay(-1)
~inside(0)
it should be snowing outside, but not inside.
Next we're testing portraits: (normal)

~show(3, 007)
~show(0, 200)
It sure is cold inside. 

~p(200)
~n("Dog")
...

~p(-1)
~n("")
Not bad, eh?

~show(0, 200)
Bonelord is watching.

~hide(5)
Next: regular scheduled whatever.


~show(4, 100)
~n("")
here's mc on the right.


~show(3, 0)
and there's friday on the left.

~show(3, 2)
She's frowning.

~show(3, 5)
She's disappointed.

~show(3, 8)
She's bored.

~show(3, 10)
She's distant.

~show(0, 200)
and here's the dog on the far left.

~show(0, 202)
~p(202)
~n("Bonelord")
Woof!

~n("")
~p(-1)
well? end.
~hide(0)

~center(0)

~n("Me")
~p(0)
Hi my name is mc. lalalaalalala.

~n("Friday")
~p(1)
...
Tch.

~p(-1)
~n("")
What the hell was that? 
She's mad, isn't she? She's really furious, isn't she?

~hide(3)
~show(0, 1)
She moves as far away as is possible inside the cramped interior.
Oh, how cruel... I'm dead to her.

end
-> END
