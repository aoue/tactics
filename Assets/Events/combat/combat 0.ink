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
//- label == 1: -> p_reinforce1
- label == -2: -> mission_end_win
- label == -3: -> mission_end_loss
}

=== mission_begin ===
~play_music(0)
~jump(9, 5)

~p(200)
Here we go.
-> END

=== p_reinforce1 ===
~jump(9, 5)
//draw attention to these reinforcements, and that they're different.
//show picture in middle cg?

//mc: there's more of them, etc
//friday: ...this changes nothing.
~p(100)
Now I'm here.
~p(200)
Cool.
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
