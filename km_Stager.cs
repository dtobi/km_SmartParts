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

using KSP.IO;
using KSPAPIExtensions;
using KSPAPIExtensions.PartMessage;
using KSPAPIExtensions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KM_Lib
{
    public class KM_Stager : PartModule
    {

        #region Fields

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group"),
            UI_ChooseOption(
                options = new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" },
                display = new String[] { "Stage", "AG1", "AG2", "AG3", "AG4", "AG5", "AG6", "AG7", "AG8", "AG9", "AG10", "Lights", "RCS", "SAS", "Brakes", "Abort" }
            )]
        public string group = "0";

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Resource"),
            UI_ChooseOption(
                options = new string[] { "Empty" }
            )]
        public string monitoredResource = "Empty";

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Resource")]
        public String resourceFlightDisplay = "Empty";

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = true, guiName = "Percentage", guiFormat = "F0", guiUnits = "%"),
            UI_FloatEdit(scene = UI_Scene.All, minValue = 0f, maxValue = 100f, incrementSlide = 1f)]
        public float activationPercentage = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Detection"),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool isActive = true;

        [KSPAction("Activate Detection")]
        public void doActivateAG(KSPActionParam param) {
            isActive = true;
        }

        [KSPAction("Deactivate Detection")]
        public void doDeActivateAG(KSPActionParam param) {
            isActive = false;
        }

        #endregion

        #region Variables

        private Part observedPart = null;

        #endregion

        #region Overrides

        public override void OnStart(StartState state) {
            if (state == StartState.Editor) {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
            }
            print("KM Stager Started");
            //Find which part we should be monitoring in flight
            if (HighLogic.LoadedSceneIsFlight) {
                findObservedPart();
                //Update static resource flight display with correct resource name
                resourceFlightDisplay = monitoredResource;
            }
            //Find which part we should be monitoring, and update the fuel list in the editor
            if(HighLogic.LoadedSceneIsEditor && this.part.parent != null) {
                findObservedPart();
                updateList();
            }
        }

        public override void OnUpdate() {
            if (isActive && observedPart != null && monitoredResource != "Empty") {
                //Check fuel percantage and compare it to target percentage
                if (((observedPart.Resources[monitoredResource].amount / observedPart.Resources[monitoredResource].maxAmount) * 100) <= activationPercentage) {
                    Utility.fireEvent(this.part, int.Parse(group));
                    print("KM Stager: Target percentage hit");
                    isActive = false;
                }
            }
        }

        public override string GetInfo() {
            return "Built-in auto staging smart part";
        }

        #endregion

        #region Methods

        private void findObservedPart() {
            //If this is a smart fuel tank, monitor self
            if (this.part.Resources.Count > 0) {
                print("KM Stager: Monitoring this part");
                observedPart = this.part;
            }
            //Otherwise monitor the parent part
            else {
                print("KM Stager: Monitoring parent part");
                observedPart = this.part.parent;
            }
            print("KM Stager: Set observed part to " + observedPart.partName + "Active is: " + isActive);
        }

        private void updateList() {
            //Create temporary string list to collect resources
            List<string> resourceList = new List<string>();
            //Instantiate monitoredResource options so we can access its option array
            UI_ChooseOption resourceOptions = (UI_ChooseOption)Fields["monitoredResource"].uiControlEditor;
            if (observedPart != null) {
                if (observedPart.Resources.Count > 0) {
                    //Iterate through resources and add them to the resourceList
                    foreach (PartResource resource in observedPart.Resources) {
                        resourceList.Add(resource.resourceName);
                    }
                    //Convert resource list to array and assign it to the monitoredResource options
                    resourceOptions.options = resourceList.ToArray<String>();
                    //If we already have selected a resource, don't reassign it to the default
                    monitoredResource = (resourceList.Contains(monitoredResource) ? monitoredResource : resourceList[0]);
                }
                else {
                    //If there are no resources in the monitored part, set monitoredResource to "Empty"
                    resourceOptions.options = new string[] { "Empty" };
                    monitoredResource = "Empty";
                }
            }
        }

        [PartMessageListener(typeof(PartResourceListChanged), scenes: GameSceneFilter.AnyEditor, relations: PartRelationship.Parent)]
        private void parentChangeListener() {
            findObservedPart();
            updateList();
        }

        [PartMessageListener(typeof(PartResourceListChanged), scenes: GameSceneFilter.AnyEditor, relations: PartRelationship.Self)]
        private void selfChangeListener() {
            findObservedPart();
            updateList();
        }

        private void OnEditorAttach() {
            RenderingManager.AddToPostDrawQueue(99, updateEditor);
            findObservedPart();
            updateList();
        }

        private void OnEditorDetach() {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDestroy() {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void updateEditor() {
        }

        #endregion
    }
}

