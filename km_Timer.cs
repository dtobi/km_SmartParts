/*
 * Author: dtobi
 * This work is shared under CC BY-NC-ND 3.0 license.
 * Non commercial, no derivatives, attribution if shared unmodified.
 * You may distribute this code and the compiled .dll as is.
 * 
 * Exception from the no-deriviates clause in case of KSP updates:
 * In case of an update of KSP that breaks the code, you may change
 * this code to make it work again and redistribute it under a different
 * class name until the author publishes an updated version. After a
 * release by the author, the right to redistribute the changed code
 * vanishes.
 * 
 * You must keep this boilerplate in the file and give credit to the author
 * in the download file as well as on the webiste that links to the file.
 * 
 * Should you wish to change things in the code, contact me via the KSP forum.
 * Patches are welcome.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace KM_Lib
{
    public class KM_Timer : PartModule 
    {

        [KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Group")]
        public String groupName = "Stage";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Select") , UI_FloatRange(minValue = 0f, maxValue = 15f, stepIncrement = 1f)]
        public float group = 0;
        private float lastGroup = 0;

        // remember the time wehen the countdown was started
        [KSPField(isPersistant = true, guiActive = false)]
        private double triggerTime = 0;

        // default delay is 0. This can be overwritten by action groups.
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Delay", guiUnits = "s"), UI_FloatRange(minValue = 0.5f, maxValue = 20f, stepIncrement = 0.5f)]
        private float triggerDelay = 5;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Remaining Time", guiFormat = "#0.##")]
        private double remainingTime = 0;

        [KSPAction("Start Countdown")]
        public void activateTimerCustom (KSPActionParam param)
        {
            activateTimer (triggerDelay);
        }

        [KSPEvent(guiName = "Start Countdown", guiActive = true)]
        public void activateTimer2 ()
        {
            activateTimer (triggerDelay);
        }

        [KSPEvent(guiName = "Reset", guiActive = true)]
        public void resetTimer ()
        {   triggerTime   = 0;
            remainingTime = 0;
            Utility.switchLight (this.part, "light-go", true);             
            Utility.playAnimationSetToPosition (this.part, "glow", 1);
        }

        public void activateTimer(float delay)
        {
            triggerTime = Time.fixedTime;
            triggerDelay = delay;
            remainingTime = triggerDelay;
            print ("Activating Timer: " + triggerDelay);
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor)
            {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                OnEditorAttach();
            }
            part.ActivatesEvenIfDisconnected = true;
            //this.part.stackIcon.SetIcon(this.part, "Klockheed_Martian/Icons/controller-stage.png", 0, 0);
        }


        public override void OnUpdate()
        {
            if (group != lastGroup) {
                groupName = Utility.KM_dictAGNames [(int)group];
                lastGroup = group;
            }
            if (triggerTime > 0) {
                remainingTime = triggerTime + triggerDelay - Time.time;
                if (remainingTime < 0) {
                    print ("Stage:"+Utility.KM_dictAGNames [(int)group]);
                    Utility.fireEvent (this.part, (int)group);
                    Utility.switchLight (this.part, "light-go", true);             
                    Utility.playAnimationSetToPosition (this.part, "glow", 1);
                    remainingTime = 0;
                    triggerTime = 0;
                }
            }
        }
        private void OnEditorAttach()
        {
            RenderingManager.AddToPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDetach()
        {            

            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDestroy()
        {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);

        }

        private void updateEditor(){
            if (group != lastGroup) {
                groupName = Utility.KM_dictAGNames [(int)group];
                lastGroup = group;
            }

        }
    }
}

