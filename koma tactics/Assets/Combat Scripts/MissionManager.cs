using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    //very simple, just has a list of all missions.
    //when loading scene, combatgrid just pulls the scheduled mission from here.


    [SerializeField] private Mission[] allMissions;

    public Mission get_mission(int which) { return allMissions[which]; }


}
