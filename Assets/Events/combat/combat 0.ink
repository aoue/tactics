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
~jump(3, 5)

//~cg(0)
~n("Voice")
~p(100)
Testy test.

~n("")
~p(-1)
Time to fight!
-> END

=== e_reinforce2 ===
~jump(7,5)
reinforcements just before round 3 starts (two scarabit swarmers)!
//draw attention to these reinforcements, and that they're different.
//show picture in middle cg?

//mc: there's more of them, etc
//friday: ...this changes nothing.
-> END

=== p_reinforce3 ===
~jump(7,5)
reinforcements just before round 4 starts (yve and nai)!
~n("Nai")
~p(300) //nai annoyed
...What the hell's happening on in here?


~n("Yvette")
~p(200) //yve grin
Ha, great! Looks like the boring part's already over.
Just stay behind me, Nai.  
-> END

=== e_reinforce5 ===
~jump(7,5)
reinforcements just before round 6!
//draw attention to these reinforcements, and that they're different AGAIN.
//show picture in middle cg?
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
