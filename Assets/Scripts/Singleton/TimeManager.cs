using System;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private float _time = 0f; //Value. Time as the ressource the player will spend between offerings
    private ETimeState _state = ETimeState.DEFAULT; //Value. State for time management logics

    public void ChangeTime(float f) //Access _time and activate/deactivate suddendeath
    {
        if (_state != ETimeState.PAUSED)
        {
            _time += f;
            if (_time <= 0)
            {
                _time = 0;
                _state = ETimeState.SUDDENDEATH;
            }
            else
            {
                _state = ETimeState.DEFAULT;
            }
        }
    }
}
