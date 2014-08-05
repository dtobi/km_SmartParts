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
    internal static class Channel
    {    
        public static List<KM_RadioControl> radioListeners = new List<KM_RadioControl>();

    }

    public class KM_RadioControl : PartModule 
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Sync. Head."),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        private bool enableSync= false;
        private int updateCounter = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Channel") , UI_FloatRange(minValue = 1f, maxValue = 20f, stepIncrement = 1f)]
        public float channel = 1;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group"),
            UI_ChooseOption(
                options = new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" },
                display = new String[] { "Stage", "AG1", "AG2", "AG3", "AG4", "AG5", "AG6", "AG7", "AG8", "AG9", "AG10", "Lights", "RCS", "SAS", "Brakes", "Abort" }
            )]
        public string group = "0";

        double fireTime = 0;
        double lightOnTime = 2;

        [KSPField]
        public string rcv_sound = "";

        [KSPAction("Transmit")]
        public void transmit_AG (KSPActionParam param)
        {
            transmitCommand (float.Parse(group));
        }

        [KSPAction("Transmit Stage")]
        public void transmit_AG0 (KSPActionParam param)
        {
            transmitCommand (0);
        }

        [KSPAction("Transmit AG1")]
        public void transmit_AG1 (KSPActionParam param)
        {
            transmitCommand (1);
        }

        [KSPAction("Transmit AG2")]
        public void transmit_AG2 (KSPActionParam param)
        {
            transmitCommand (2);
        }

        [KSPAction("Transmit AG3")]
        public void transmit_AG3 (KSPActionParam param)
        {
            transmitCommand (3);
        }

        [KSPAction("Transmit AG4")]
        public void transmit_AG4 (KSPActionParam param)
        {
            transmitCommand (4);
        }

        [KSPAction("Transmit AG5")]
        public void transmit_AG5 (KSPActionParam param)
        {
            transmitCommand (5);
        }

        [KSPAction("Transmit AG6")]
        public void transmit_AG6 (KSPActionParam param)
        {
            transmitCommand (6);
        }

        [KSPAction("Transmit AG7")]
        public void transmit_AG7 (KSPActionParam param)
        {
            transmitCommand (7);
        }

        [KSPAction("Transmit AG8")]
        public void transmit_AG8 (KSPActionParam param)
        {
            transmitCommand (8);
        }

        [KSPAction("Transmit AG9")]
        public void transmit_AG9 (KSPActionParam param)
        {
            transmitCommand (9);
        }

        [KSPAction("Transmit AG10")]
        public void transmit_AG10 (KSPActionParam param)
        {
            transmitCommand (10);
        }


        [KSPAction("Transmit Light")]
        public void transmit_Light (KSPActionParam param)
        {
            transmitCommand (11);
        }

        [KSPAction("Transmit RCS")]
        public void transmit_RCS (KSPActionParam param)
        {
            transmitCommand (12);
        }

        [KSPAction("Transmit SAS")]
        public void transmit_SAS (KSPActionParam param)
        {
            transmitCommand (13);
        }

        [KSPAction("Transmit Brakes")]
        public void transmit_Brakes (KSPActionParam param)
        {
            transmitCommand (14);
        }

        [KSPAction("Transmit Abort")]
        public void transmit_Abort (KSPActionParam param)
        {
            transmitCommand (15);
        }

        [KSPEvent(guiName = "Transmit Command", guiActive = true)]
        public void transmit_GUI ()
        {
            transmitCommand (float.Parse(group));
        }

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Throttle") , UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float throttle = 1;


        [KSPAction("Transmit Throttle")]
        public void transmit_ThrottleAG (KSPActionParam param)
        {
            transmitThrottle ();
        }

        [KSPEvent(guiName = "Transmit Throttle", guiActive = true)]
        public void transmit_Throttle ()
        {
            transmitThrottle ();
        }

        [KSPAction("Match Heading")]
        public void transmit_pro_rotationAG (KSPActionParam param)
        {
            transmitRotation (this.vessel.GetTransform().rotation, true);
        }

        [KSPEvent(guiName = "Match Heading", guiActive = true)]
        public void transmit_pro_rotation ()
        {
            transmitRotation (this.vessel.GetTransform().rotation, true);
        }
        /*
        [KSPAction("Head to vessel")]
        public void transmit_rotateTowards (KSPActionParam param)
        {
            transmitRotationTo (vessel.findWorldCenterOfMass(), true);
        }

        [KSPEvent(guiName = "Head to vessel", guiActive = true)]
        public void transmit_rotateTowardsAG ()
        {
            transmitRotationTo (vessel.findWorldCenterOfMass(), true);
        }
        */

        public void transmitRotation(Quaternion rotation, bool playSound){
            foreach (var listener in Channel.radioListeners) {
                if(listener != null && listener.vessel!= null)  
                    listener.receiveRotation (this, rotation, (int) channel, playSound);
            }

            indicateSend ();

        }
        /*
        public void transmitRotationTo(Vector3 target, bool playSound){
            foreach (var listener in Utility.radioListeners) {  
                if(listener != null && listener.vessel!= null)  
                    listener.receiveRotation (this, Quaternion.LookRotation((target-listener.vessel.findWorldCenterOfMass()).normalized), (int) channel, playSound);            }
            indicateSend ();
      
        
        }*/

        public void receiveRotation(KM_RadioControl sender, Quaternion targetUp, int transmitChannel, bool playSound){


            if (this == sender || channel != transmitChannel || this.vessel == FlightGlobals.ActiveVessel) {
                MonoBehaviour.print ("I am the active vessel or the sender or channels are not equal:" + channel + ", " + transmitChannel);
                return;
            }

            this.vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            print ("Listener:" + vessel.vesselName + "received command" + group);    
            this.vessel.vesselSAS.LockHeading(targetUp, true);
            this.vessel.vesselSAS.Update();
            indicateReceive (playSound);
        }



        public void transmitCommand(float groupID)
        {
            foreach (var listener in Channel.radioListeners) {
                if(listener != null && listener.vessel!= null)
                    listener.receiveCommand (this, (int) groupID, (int) channel);
            }

            indicateSend ();
        }

        public void transmitThrottle()
        {
            foreach (var listener in Channel.radioListeners) {
                if(listener != null && listener.vessel!= null)
                    listener.receiveThrottle (this, throttle, (int) channel);
            }
            indicateSend ();

        }

        private void indicateSend(){
            km_Helper.switchLight (this.part, "light-go", true);
            km_Helper.playAnimationSetToPosition(this.part, "glow", 1);
            fireTime = Time.fixedTime;
            print ("Fire Time:" + fireTime);
        }

        private void indicateReceive(bool playSound){
            km_Helper.switchLight(this.part, "light-go", true);
            km_Helper.playAnimationSetToPosition(this.part, "glow", 1);
            fireTime = Time.fixedTime;
            if (playSound) km_Helper.playAudio(this.part, rcv_sound);
            print ("Fire Time:" + fireTime);
        }

        public void receiveCommand(KM_RadioControl sender, int group, int transmitChannel){
            if (this == sender || channel != transmitChannel) {
                MonoBehaviour.print ("I am the sender or channels are not equal:" + channel + ", " + transmitChannel);
                return;
            }
            print ("Listener:" + vessel.vesselName + "received command" + group);
            km_Helper.fireEvent(this.part, (int)group);
            indicateReceive (true);
        }

        public void receiveThrottle(KM_RadioControl sender, float throttle, int transmitChannel){
            if (this == sender || channel != transmitChannel) {
                MonoBehaviour.print ("I am the sender or channels are not equal:" + channel + ", " + transmitChannel);
                return;
            }
            print ("Listener:" + vessel.vesselName + "received command" + group);    
            this.vessel.ctrlState.mainThrottle = throttle;
            indicateReceive (true);
        }
        ~ KM_RadioControl() 
        {
            MonoBehaviour.print ("Destructor called");
        }

        public override void OnInactive ()
        {
            MonoBehaviour.print ("OnInactive called");
            base.OnInactive ();
        }
        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor) {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                this.part.OnJustAboutToBeDestroyed += OnJustAboutToBeDestroyed;



                OnEditorAttach ();
            } else {
                Channel.radioListeners.Add (this);
                foreach (var listener in Channel.radioListeners) {
                    if(listener.vessel!= null)
                        print ("Listener found:"+listener.vessel.vesselName);
                }
            }
        }

 

        public override void OnUpdate()
        {

            if (enableSync && updateCounter % 30 == 0 && this.vessel == FlightGlobals.ActiveVessel) {
                transmitRotation (this.vessel.GetTransform().rotation, false);
            }
            updateCounter++;

            if (fireTime != 0 && fireTime + lightOnTime <= Time.time) {
                km_Helper.switchLight(this.part, "light-go", false);
                km_Helper.playAnimationSetToPosition(this.part, "glow", 0);
            }

            //if (fireTime != 0 && fireTime + 10 <= Time.time) {
            //    this.vessel.vesselSAS.Update ();
            //}

        }

       
        public void OnDetach(bool first){
            Channel.radioListeners.Remove (this);
            MonoBehaviour.print ("OnDetach");
        }

        private void OnJustAboutToBeDestroyed()
        {
            Channel.radioListeners.Remove (this);
            MonoBehaviour.print ("OnJustAboutToBeDestroyed");
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

        }
    }
}

