//The base combat mission event.
//This one script file will hold all the dialogue scenes for this mission, and we navigate by label.
//A mission dialogue can be played at:
//(mandatory)
//  -start of mission
//  -end of mission (victory)
//  -end of mission (loss)
//(optional)
//  -start of any round after

//Externals here, can:
EXTERNAL play_music(which) //play song using int->sound manager
EXTERNAL n(name) //set name to string
EXTERNAL p(which) //set box portrait using int->portrait library
EXTERNAL jump(x, y) //jump to x, y coord on game map
EXTERNAL slide(x, y) //slide to x, y coord on game map

//Navigation to the right label
VAR label = 0

//before the script begins, labelIndex will be set to start.
//Then we jump to whatever label corresponds. (index is round number.)

//switch statement:
//PLEASE NOTE:
//the index corresponds to the round just before. So if you want a unit to be there for the start of round 3,
//then they must be spawned with index 2.
//the indices for the labels here, in the mission's reinforcement functions, and in the mission's event rounds should all be the same.
{
- label == 0: -> mission_begin
- label == 2: -> e_reinforce2
- label == 3: -> p_reinforce3
- label == 5: -> e_reinforce5
- label == -2: -> mission_end_win
- label == -3: -> mission_end_loss
}

=== mission_begin ===
~play_music(0)
~jump(5, 5)

~n("Anse")
~p(200)
That's a lot of them.
~n("Friday")
~p(100)
This is a spiritual matter. There's no need for you to trouble yourself.
~n("Anse")
~p(200)
...A member of the Circle doesn't shirk his part that easily.
~n("Anse")
(If we can last for a little while, those two should get here...)
-> END

=== e_reinforce2 ===
~jump(7,5)
reinforcements just before round 3 starts (two scarabit responders)!
//draw attention to these reinforcements, and that they're different.
//show picture in middle cg?

//mc: there's more of them, etc
//friday: ...this changes nothing.
-> END

=== p_reinforce3 ===
~jump(7,5)
reinforcements just before round 4 starts (yve and nai)!

//anse is annoyed that they're later
//yve apologizes insincerely
//nai tells yve not to bother apologizing if she's going to do it like that
//...well, it's nice that you're finally here.

~n("Nai")
~p(400) //nai annoyed
...What the hell's happening here?

~n("Yvette")
~p(300) //yve grin
Ha, great! Looks like the boring part's already over.
Just stay behind me, Nai.  
-> END

=== e_reinforce5 ===
~jump(7,5)
reinforcements just before round 6 (6 scarabit swarmers)!
//draw attention to these reinforcements, and that they're different AGAIN.
//show picture in middle cg?

~n("Friday")
~p(100)
...This is the last of them.

~n("Anse")
~p(200)
Hmm? How do you know that?

~n("Friday")
~p(100)
...

~n("Yvette")
~p(300)
Hey now, speak up. Don't be so mysterious.

~n("Anse")
~p(200)
The real mystery is where the hell you two were...

-> END

=== mission_end_win ===
This is dummy text for triumph at the end of the mission.
blah blah blah
-> END

=== mission_end_loss ===
This is dummy text for loss at the end of the mission.
blah blah blah
-> END








//eof
