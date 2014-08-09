/*
 * Author: dtobi, Firov
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
using KSPAPIExtensions;

namespace KM_Lib
{
    public class KM_Timer : PartModule
    {
        #region Fields

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group"),
            UI_ChooseOption (
            options = new String[] {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15"
            },
            display = new String[] {
                "Stage",
                "AG1",
                "AG2",
                "AG3",
                "AG4",
                "AG5",
                "AG6",
                "AG7",
                "AG8",
                "AG9",
                "AG10",
                "Lights",
                "RCS",
                "SAS",
                "Brakes",
                "Abort"
            }
        )]
        public string group = "0";

        // remember the time wehen the countdown was started
        [KSPField (isPersistant = true, guiActive = false)]
        private double triggerTime = 0;

        // Delay in seconds. Used for precise measurement
        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Seconds", guiFormat = "F2", guiUnits = "sec"),
            UI_FloatEdit (scene = UI_Scene.All, minValue = 0f, maxValue = 120f, incrementLarge = 20f, incrementSmall = 1f, incrementSlide = .05f)]
        public float triggerDelaySeconds = 0;

        // Delay in minutes. Used for longer term measurement
        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Minutes", guiFormat = "F2", guiUnits = "min"),
            UI_FloatEdit (scene = UI_Scene.All, minValue = 0f, maxValue = 360f, incrementLarge = 60f, incrementSmall = 5f, incrementSlide = .25f)]
        public float triggerDelayMinutes = 0;

        [KSPField (isPersistant = true, guiActive = true, guiName = "Remaining Time", guiFormat = "F2")]
        private double remainingTime = 0;

        [KSPField (isPersistant = true)]
        private Boolean allowStage = true;

        [KSPField (isPersistant = true)]
        private Boolean useSeconds = true;

        #endregion


        #region Events

        [KSPEvent (guiActive = false, guiActiveEditor = false, guiName = "Use Seconds")]
        public void setSeconds ()
        {
            useSeconds = true;
            updateButtons ();
        }

        [KSPEvent (guiActive = false, guiActiveEditor = false, guiName = "Use Minutes")]
        public void setMinutes ()
        {
            useSeconds = false;
            updateButtons ();
        }

        [KSPEvent (guiActive = false, guiActiveEditor = false, guiName = "Enable Staging")]
        public void activateStaging ()
        {
            enableStaging ();
        }

        [KSPEvent (guiActive = false, guiActiveEditor = false, guiName = "Disable Staging")]
        public void deactivateStaging ()
        {
            disableStaging ();
        }

        [KSPEvent (guiName = "Start Countdown", guiActive = true)]
        public void activateTimer ()
        {
            reset ();
            setTimer ();
        }

        [KSPAction ("Start Countdown")]
        public void activateTimerAG (KSPActionParam param)
        {
            reset ();
            setTimer ();
        }

        [KSPEvent (guiName = "Reset", guiActive = true)]
        public void resetTimer ()
        {
            reset ();
        }

        [KSPAction ("Reset")]
        public void resetTimerAG (KSPActionParam param)
        {
            reset ();
        }

        #endregion


        #region Variables

        private Boolean armed = true;
        private int previousStage = 0;

        #endregion


        #region Overrides

        public override void OnStart (StartState state)
        {
            if (state == StartState.Editor) {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                OnEditorAttach ();
            }
            if (allowStage) {
                Events ["activateStaging"].guiActiveEditor = false;
                Events ["deactivateStaging"].guiActiveEditor = true;
            } else {
                Invoke ("disableStaging", 0.25f);
            }
            GameEvents.onVesselChange.Add (onVesselChange);
            part.ActivatesEvenIfDisconnected = true;
            //Initial button layout
            updateButtons ();
        }

        public override void OnActive ()
        {            
            //If staging enabled, set timer
            if (allowStage) {
                setTimer ();
            }
        }

        public override void OnUpdate ()
        {
            //Check to see if the timer has been dragged in the staging list. If so, reset icon color
            if (this.part.inverseStage != previousStage && allowStage && !armed && this.part.inverseStage < Staging.CurrentStage) {
                reset ();
            }
            previousStage = this.part.inverseStage;

            //If the timer has been activated, start the countdown, activate the model's LED, and change the icon color
            if (triggerTime > 0 && armed) {
                remainingTime = triggerTime + (useSeconds ? triggerDelaySeconds : triggerDelayMinutes * 60) - Time.time;
                Utility.switchLight (this.part, "light-go", true);
                Utility.playAnimationSetToPosition (this.part, "glow", 1);
                this.part.stackIcon.SetIconColor (XKCDColors.BrightYellow);

                //Once the timer hits 0 activate the stage/AG, disable the model's LED, and change the icon color
                if (remainingTime < 0) {
                    print ("Stage:" + km_Helper.KM_dictAGNames [int.Parse (group)]);
                    km_Helper.fireEvent (this.part, int.Parse (group));
                    this.part.stackIcon.SetIconColor (XKCDColors.Red);
                    triggerTime = 0;
                    remainingTime = 0;
                    //Disable timer until reset
                    armed = false;
                }
            }
        }

        #endregion


        #region Methods

        public void onVesselChange (Vessel newVessel)
        {
            if (newVessel == this.vessel && !allowStage) {
                Invoke ("disableStaging", 0.25f);
            }
        }

        private void enableStaging ()
        {
            part.stackIcon.CreateIcon ();
            Staging.SortIcons ();
            allowStage = true;

            //Toggle button visibility so currently inactive mode's button is visible
            Events ["activateStaging"].guiActiveEditor = false;
            Events ["deactivateStaging"].guiActiveEditor = true;
        }

        private void disableStaging ()
        {
            part.stackIcon.RemoveIcon ();
            Staging.SortIcons ();
            allowStage = false;

            //Toggle button visibility so currently inactive mode's button is visible
            Events ["activateStaging"].guiActiveEditor = true;
            Events ["deactivateStaging"].guiActiveEditor = false;
        }

        private void setTimer ()
        {
            if (armed) {
                //Set the trigger time, which will be caught in OnUpdate
                triggerTime = Time.fixedTime;
                print ("Activating Timer: " + (useSeconds ? triggerDelaySeconds : triggerDelayMinutes * 60));
            }
        }

        private void reset ()
        {
            //Reset trigger and remaining time to 0
            triggerTime = 0;
            remainingTime = 0;
            //Switch off model lights
            Utility.switchLight (this.part, "light-go", false);
            Utility.playAnimationSetToPosition (this.part, "glow", 0);
            //Reset icon color to white
            this.part.stackIcon.SetIconColor (XKCDColors.White);
            //Reset armed variable
            armed = true;
        }

        private void updateButtons ()
        {
            if (useSeconds) {
                //Show minute button
                Events ["setMinutes"].guiActiveEditor = true;
                Events ["setMinutes"].guiActive = true;
                //Hide minute scale
                Fields ["triggerDelayMinutes"].guiActiveEditor = false;
                Fields ["triggerDelayMinutes"].guiActive = false;
                //Hide seconds button
                Events ["setSeconds"].guiActiveEditor = false;
                Events ["setSeconds"].guiActive = false;
                //Show seconds scale
                Fields ["triggerDelaySeconds"].guiActiveEditor = true;
                Fields ["triggerDelaySeconds"].guiActive = true;
                //Reset minute scale
                triggerDelayMinutes = 0f;
            } else {
                //Hide minute button
                Events ["setMinutes"].guiActiveEditor = false;
                Events ["setMinutes"].guiActive = false;
                //Show minute scale
                Fields ["triggerDelayMinutes"].guiActiveEditor = true;
                Fields ["triggerDelayMinutes"].guiActive = true;
                //Show seconds button
                Events ["setSeconds"].guiActiveEditor = true;
                Events ["setSeconds"].guiActive = true;
                //Hide seconds scale
                Fields ["triggerDelaySeconds"].guiActiveEditor = false;
                Fields ["triggerDelaySeconds"].guiActive = false;
                //Reset seconds scale
                triggerDelaySeconds = 0;
            }
        }

        private void OnEditorAttach ()
        {
            RenderingManager.AddToPostDrawQueue (99, updateEditor);
        }

        private void OnEditorDetach ()
        {            
            RenderingManager.RemoveFromPostDrawQueue (99, updateEditor);
        }

        private void OnEditorDestroy ()
        {
            RenderingManager.RemoveFromPostDrawQueue (99, updateEditor);
        }

        private void updateEditor ()
        {
            //Update Buttons
            updateButtons ();
        }

        #endregion
    }
}
