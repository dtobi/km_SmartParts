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

namespace KM_Lib
{
    public class KM_Timer : PartModule
    {
        #region Fields

        [KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Group")]
        public String groupName = "Stage";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Select") , UI_FloatRange(minValue = 0f, maxValue = 16f, stepIncrement = 1f)]
        public float group = 0;
        private float lastGroup = 0;

        // remember the time wehen the countdown was started
        [KSPField(isPersistant = true, guiActive = false)]
        private double triggerTime = 0;

        // default delay is 0. This can be overwritten by action groups.
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Delay", guiUnits = "s"), UI_FloatRange(minValue = 0.5f, maxValue = 20f, stepIncrement = 0.5f)]
        private float triggerDelay = 5;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Remaining Time", guiFormat = "F2")]
        private double remainingTime = 0;

        [KSPField(isPersistant = true)]
        private Boolean allowStage = true;

        #endregion
        

        #region Events
        
        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Enable Staging")]
        public void activateStaging() {
           enableStaging();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Disable Staging")]
        public void deactivateStaging() {
            disableStaging();
        }

        [KSPEvent(guiName = "Start Countdown", guiActive = true)]
        public void activateTimer () {
            setTimer();
        }

        [KSPAction("Start Countdown")]
        public void activateTimerAG(KSPActionParam param) {
            setTimer();
        }

        [KSPEvent(guiName = "Reset", guiActive = true)]
        public void resetTimer () {
            reset();
        }

        [KSPAction("Reset")]
        public void resetTimerAG(KSPActionParam param)
        {
            reset();
        }

        #endregion


        #region Variables

        private Boolean armed = true;

        #endregion


        #region Overrides

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor)
            {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                OnEditorAttach();
            }
            if (allowStage) {
                Events["activateStaging"].guiActiveEditor = false;
                Events["deactivateStaging"].guiActiveEditor = true;
            }
            else {
                Invoke("disableStaging", 0.25f);
            }
            GameEvents.onVesselChange.Add(onVesselChange);
            part.ActivatesEvenIfDisconnected = true;
        }

        public override void OnActive()
        {            
            //If staging enabled, set timer
            if(allowStage) {
                setTimer();
            }
        }

        public override void OnUpdate()
        {
            //Watch for changes to the selected group. If any, update display with proper name.
            if (group != lastGroup) {
                groupName = Utility.KM_dictAGNames [(int)group];
                lastGroup = group;
            }

            //Check to see if the timer has been dragged in the staging list. If so, reset icon color
            if (allowStage && armed == false && this.part.inverseStage < Staging.CurrentStage) {
                reset();
            }

            //If the timer has been activated, start the countdown, activate the model's LED, and change the icon color
            if (triggerTime > 0 && armed == true) {
                remainingTime = triggerTime + triggerDelay - Time.time;
                Utility.switchLight(this.part, "light-go", true);
                Utility.playAnimationSetToPosition(this.part, "glow", 1);
                this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);

                //Once the timer hits 0 activate the stage/AG, disable the model's LED, and change the icon color
                if (remainingTime < 0) {
                    print ("Stage:"+Utility.KM_dictAGNames [(int)group]);
                    Utility.fireEvent (this.part, (int)group);
                    this.part.stackIcon.SetIconColor(XKCDColors.Red);
                    triggerTime = 0;
                    remainingTime = 0;
                    //Disable timer until reset
                    armed = false;
                }
            }
        }

        #endregion


        #region Methods

        public void onVesselChange(Vessel newVessel) {
            if (newVessel == this.vessel && !allowStage) {
                Invoke("disableStaging", 0.25f);
            }
        }

        private void enableStaging() {
                part.stackIcon.CreateIcon();
                Staging.SortIcons();
                allowStage = true;

                if(HighLogic.LoadedSceneIsEditor) {
                    //Toggle button visibility so currently inactive mode's button is visible
                    Events["activateStaging"].guiActiveEditor = false;
                    Events["deactivateStaging"].guiActiveEditor = true;
                }
        }

        private void disableStaging() {
                part.stackIcon.RemoveIcon();
                Staging.SortIcons();
                allowStage = false;

                if(HighLogic.LoadedSceneIsEditor) {
                    //Toggle button visibility so currently inactive mode's button is visible
                    Events["activateStaging"].guiActiveEditor = true;
                    Events["deactivateStaging"].guiActiveEditor = false;
                }
        }

        private void setTimer() {
            if (armed == true)
            {
                //Set the trigger time, which will be caught in OnUpdate
                triggerTime = Time.fixedTime;
                print("Activating Timer: " + triggerDelay);
                }
        }

        private void reset() {
            //Reset trigger and remaining time to 0
            triggerTime = 0;
            remainingTime = 0;
            //Switch off model lights
            Utility.switchLight(this.part, "light-go", false);
            Utility.playAnimationSetToPosition(this.part, "glow", 0);
            //Reset icon color to white
            this.part.stackIcon.SetIconColor(XKCDColors.White);
            //Reset armed variable
            armed = true;
        }

        private void OnEditorAttach() {
            RenderingManager.AddToPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDetach() {            
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDestroy() {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void updateEditor(){
            if (group != lastGroup) {
                groupName = Utility.KM_dictAGNames [(int)group];
                lastGroup = group;
            }

        }

        #endregion
    }
}
