using System;

using System.Collections.Generic;
using UnityEngine;

namespace PubNubAPI
{
    public class RemoveAllPushChannelsForDeviceRequestBuilder: PubNubNonSubBuilder<RemoveAllPushChannelsForDeviceRequestBuilder, PNPushRemoveAllChannelsResult>, IPubNubNonSubscribeBuilder<RemoveAllPushChannelsForDeviceRequestBuilder, PNPushRemoveAllChannelsResult>
    {      
        public RemoveAllPushChannelsForDeviceRequestBuilder(PubNubUnity pn):base(pn, PNOperationType.PNRemoveAllPushNotificationsOperation){
        }

        private string DeviceIDForPush{ get; set;}

        public void DeviceId(string deviceId){
            DeviceIDForPush = deviceId;
        }

        public PNPushType PushType {get;set;}

        #region IPubNubBuilder implementation

        public void Async(Action<PNPushRemoveAllChannelsResult, PNStatus> callback)
        {
            this.Callback = callback;
            if (string.IsNullOrEmpty (DeviceIDForPush)) {
                PNStatus pnStatus = base.CreateErrorResponseFromMessage("DeviceId is empty", null, PNStatusCategory.PNBadRequestCategory);
                Callback(null, pnStatus);

                return;
            }

            if (PushType.Equals(PNPushType.None)) {
                Debug.Log("PNPushType not selected, using GCM");                
                PushType = PNPushType.GCM;
            }
            base.Async(this);
        }
        #endregion

        protected override void RunWebRequest(QueueManager qm){
            RequestState requestState = new RequestState ();
            requestState.OperationType = OperationType;
            
            /* Uri request = BuildRequests.BuildRemoveAllDevicePushRequest(
                PushType, 
                DeviceIDForPush,
                this.PubNubInstance.PNConfig.UUID,
                this.PubNubInstance.PNConfig.Secure,
                this.PubNubInstance.PNConfig.Origin,
                this.PubNubInstance.PNConfig.AuthKey,
                this.PubNubInstance.PNConfig.SubscribeKey,
                this.PubNubInstance.Version
            ); */
            Uri request = BuildRequests.BuildRemoveAllDevicePushRequest(
                PushType, 
                DeviceIDForPush,
                ref this.PubNubInstance
            );

            base.RunWebRequest(qm, request, requestState, this.PubNubInstance.PNConfig.NonSubscribeTimeout, 0, this);
        }

        protected override void CreatePubNubResponse(object deSerializedResult, RequestState requestState){
            //[1, "Removed Device"] 
            PNPushRemoveAllChannelsResult pnPushRemoveAllChannelsResult = new PNPushRemoveAllChannelsResult();
            Dictionary<string, object> dictionary = deSerializedResult as Dictionary<string, object>;
            PNStatus pnStatus = new PNStatus();
            if(dictionary != null) {
                string message = Utility.ReadMessageFromResponseDictionary(dictionary, "message");
                if(Utility.CheckDictionaryForError(dictionary, "error")){
                    pnPushRemoveAllChannelsResult = null;
                    pnStatus = base.CreateErrorResponseFromMessage(message, requestState, PNStatusCategory.PNUnknownCategory);
                }
            } else if(dictionary==null) {
                object[] c = deSerializedResult as object[];
                
                if (c != null) {
                    string status = "";
                    string statusCode = "0";
                    if(c.Length > 0){
                        statusCode = c[0].ToString();
                    }
                    if(c.Length > 1){
                        status = c[1].ToString();
                    }
                    if(statusCode.Equals("0") || (!status.ToLower().Equals("removed device"))){
                        pnPushRemoveAllChannelsResult = null;
                        pnStatus = base.CreateErrorResponseFromMessage(status, requestState, PNStatusCategory.PNUnknownCategory);
                    } else {
                        pnPushRemoveAllChannelsResult.Message = status;
                    }
                } else {
                    pnPushRemoveAllChannelsResult = null;
                    pnStatus = base.CreateErrorResponseFromMessage("deSerializedResult object is null", requestState, PNStatusCategory.PNMalformedResponseCategory);
                }
            } else {
                pnPushRemoveAllChannelsResult = null;
                pnStatus = base.CreateErrorResponseFromMessage("Response dictionary is null", requestState, PNStatusCategory.PNMalformedResponseCategory);
            }

            Callback(pnPushRemoveAllChannelsResult, pnStatus);
        }

        // protected override void CreateErrorResponse(Exception exception, bool showInCallback, bool level){
            
        // }
        
        
    }
}
