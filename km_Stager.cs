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
    public class KM_Stager : PartModule
    {
        [KSPField (isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Group")]
        public String groupName = "Stage";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Select") , UI_FloatRange(minValue = 0f, maxValue = 16f, stepIncrement = 1f)]
        public float group = 0;
        private float lastGroup = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Detection"),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool isActive = true;



        [KSPAction("Activate Detection")]
        public void doActivateAG(KSPActionParam param)
        {
            isActive = true;
        }

        [KSPAction("Deactivate Detection")]
        public void doDeActivateAG(KSPActionParam param)
        {
            isActive = false;
        }

        private Part observedPart = null;

        private double lastFill = 0; // save the last fill level when the tank drains

        private int executionLimiter = 0;

        private int executionFrequency = 10; // the update loop will only execute every executionFrequency'th time

        private Part getObservedPart(){

            if (this.part.Resources.Count > 0) {
                print ("KM Stager: Monitoring this part");
                return this.part;
            } else if (this.part.parent.Resources.Count > 0) {
                print ("KM Stager: Monitoring parent part");
                return this.part.parent;
            }
            return null;
        }


        public override void OnStart(StartState state)
        {

            if (state == StartState.Editor) {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                OnEditorAttach ();
            } else {
                observedPart = getObservedPart ();
                print ("KM Stager: Set observed part to " + observedPart + "Active is: " + isActive);
            }
            print("KM Stager Started");
            base.OnStart (state);
        }

        public override void OnUpdate()
        {
            if(executionLimiter++ % executionFrequency == 0){

                if (group != lastGroup) {
                    groupName = Utility.KM_dictAGNames [(int)group];
                    lastGroup = group;
                }

                if(!isActive){
                    Utility.switchLight (this.part, "light-go", false);
                    Utility.playAnimationSetToPosition (this.part, "glow", 0);
                    return;
                }

                if (isActive && observedPart != null) {

                    double currentFill = 0;
                    foreach (PartResource resource in observedPart.Resources) {
                        currentFill += resource.amount;
                    }
                    if (currentFill < 2 && lastFill == currentFill) {
                        Utility.fireEvent (this.part, (int)group);
                        print ("Tank empty. Fire event");
                        isActive = false;

                        if (this.part.name == "km_smart_fuel") {
                            Utility.switchLight (this.part, "light-go", true);             
                            Utility.playAnimationSetToPosition (this.part, "glow", 1);
                        } else {
                            print ("Not animating staging for part:" + this.part.name); 
                        }
                    
                    }
                    lastFill = currentFill;
                }
           } 

        }

        public override string GetInfo ()
        {
            return "Built-in auto staging smart part";
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

