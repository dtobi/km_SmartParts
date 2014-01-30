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
    public class KM_Altimeter : PartModule
    {

        #region Fields

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Fire ALT", guiUnits = "m")]
        public int displayAlt = 0;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Group")]
        public String groupName = "Stage";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Select"), UI_FloatRange(minValue = 0f, maxValue = 16f, stepIncrement = 1f)]
        public float group = 0;
        private float lastGroup = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height"), UI_FloatRange(minValue = -25f, maxValue = 500f, stepIncrement = 5f)]
        public float targetAlt = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Units"),
            UI_Toggle(disabledText = "Meters", enabledText = "Kilometers")]
        public bool useKilometer = false;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Detection"),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool isArmed = true;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Fall Only"),
            UI_Toggle(disabledText = "False", enabledText = "True")]
        public bool onlyDescent = false;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Auto Reset"),
            UI_Toggle(disabledText = "False", enabledText = "True")]
        public bool autoReset = false;

        #endregion


        #region Events

        [KSPAction("Activate Detection")]
        public void doActivateAG(KSPActionParam param) {
            isArmed = true;
        }

        [KSPAction("Deactivate Detection")]
        public void doDeActivateAG(KSPActionParam param) {
            isArmed = false;
        }

        #endregion


        #region Variables

        private double lastAlt = 0;
        private double alt = 0;
        private double currentWindow = 0;
        private Boolean illuminated = false;

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
            print("KM Altimeter Detector Started");
        }

        public override void OnUpdate()
        {
            //Check to see if the device has been rearmed, if so, deactivate the lights
            if (isArmed && illuminated) {
                lightsOff();
            }
            //Watch for changes to the selected group. If any, update display with proper name.
            if (group != lastGroup)
            {
                groupName = Utility.KM_dictAGNames[(int)group];
                lastGroup = group;
            }
            //Update tweakables GUI
            Fields["displayAlt"].guiUnits = (useKilometer ? "km" : "m");
            //Prevent negative target
            if (targetAlt < 0) {
                targetAlt = 0;
            }            
            //Update display altitude based on user input
            displayAlt = (int)targetAlt;
        }

        public override void OnFixedUpdate()
        {
            //Check current altitude
            updateAltitude();

            //If the device is armed, check for the trigger altitude
            if(isArmed) {
                //We're ascending. Trigger at or above target height
                if (!onlyDescent && lastAlt < alt && Math.Abs((alt-currentWindow) - (useKilometer ? targetAlt * 1000 : targetAlt)) < currentWindow) {
                    print("Ascending at " + alt + ". Window is " + currentWindow + ". Vertical speed is " + this.vessel.verticalSpeed);
                    Utility.fireEvent(this.part, (int)group);
                    lightsOn();
                    isArmed = false;
                }
                //We're descending. Trigger at or below target height
                else if (lastAlt > alt && Math.Abs((alt+currentWindow) - (useKilometer ? targetAlt * 1000 : targetAlt)) < currentWindow) {
                    print("Descending at " + alt + ". Window is " + currentWindow + ". Vertical speed is " + this.vessel.verticalSpeed);
                    Utility.fireEvent(this.part, (int)group);
                    lightsOn();
                    isArmed = false;
                }
            }

            //If auto reset is enabled, wait for departure from the target window and rearm
            if(!isArmed & autoReset) {
                if (lastAlt < alt && Math.Abs((alt-currentWindow) - (useKilometer ? targetAlt * 1000 : targetAlt)) > currentWindow) {
                    print("Rearming at " + alt + " at a velocity of " + this.vessel.verticalSpeed + ". Current window is " + currentWindow);
                    isArmed = true;
                }
                else if (lastAlt > alt && Math.Abs((alt + currentWindow) - (useKilometer ? targetAlt * 1000 : targetAlt)) > currentWindow) {
                    print("Rearming at " + alt + " at a velocity of " + this.vessel.verticalSpeed + ". Current window is " + currentWindow);
                    isArmed = true;
                }
            }
        }

        #endregion


        #region Methods

        private void updateAltitude() {
            //Sea altitude
            double altSea = this.vessel.mainBody.GetAltitude(this.vessel.CoM);
            //Altitude over terrain. Does not factor in ocean surface.
            double altSurface = altSea - this.vessel.terrainAltitude;
            //Set the last altitude for the purpose of direction determination
            lastAlt = alt;
            //Use the lowest of the two values as the current altitude.
            alt = (altSurface < altSea ? altSurface : altSea);
            //Update target window size based on current vertical velocity
            currentWindow = Math.Pow((Math.Sqrt(Math.Abs(this.vessel.verticalSpeed)) / 10), 2) + 1;
        }

        private void lightsOn() {
            //Switch off model lights
            Utility.switchLight(this.part, "light-go", true);
            Utility.playAnimationSetToPosition(this.part, "glow", 1);
            illuminated = true;
        }

        private void lightsOff() {
            //Switch off model lights
            Utility.switchLight(this.part, "light-go", false);
            Utility.playAnimationSetToPosition(this.part, "glow", 0);
            illuminated = false;
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

        private void updateEditor() {
            //Adjust group name
            if (group != lastGroup)
            {
                groupName = Utility.KM_dictAGNames[(int)group];
                lastGroup = group;
            }
            //Update display altitude units
            Fields["displayAlt"].guiUnits = (useKilometer ? "km" : "m");
            //Prevent a negative target altitude
            if (targetAlt < 0) {
                targetAlt = 0;
            }
            //Update display altitude
            displayAlt = (int)targetAlt;
        }

        #endregion
    }
}

