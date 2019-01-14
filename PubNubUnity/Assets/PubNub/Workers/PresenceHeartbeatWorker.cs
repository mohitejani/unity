using System;
using UnityEngine;
using System.Linq;

namespace PubNubAPI
{
    internal class PresenceHeartbeatWorker
    {  
        private bool keepPresenceHearbeatRunning;
        private bool isPresenceHearbeatRunning;

        public bool RunIndependentOfSubscribe{
            get;set;
        }

        internal string Channels {get;set;}
        internal string ChannelGroups {get;set;}
        internal string State {get;set;}

        private readonly PNUnityWebRequest webRequest;
        private string webRequestId = "";
        private readonly PubNubUnity PubNubInstance;
        internal PresenceHeartbeatWorker(PubNubUnity pn, PNUnityWebRequest webRequest){
            PubNubInstance  = pn;
            this.webRequest = webRequest;
            this.webRequest.PNLog = this.PubNubInstance.PNLog;
            webRequest.WebRequestComplete += WebRequestCompleteHandler;
        }

        ~PresenceHeartbeatWorker(){
            CleanUp();
        }

        internal void CleanUp(){
            if (webRequest != null) {
                webRequest.WebRequestComplete -= WebRequestCompleteHandler;
                webRequest.AbortRequest(webRequestId, false);
            }
        }

        private void WebRequestCompleteHandler (object sender, EventArgs ea)
        {
            #if (ENABLE_PUBNUB_LOGGING)
            this.PubNubInstance.PNLog.WriteToLog ("WebRequestCompleteHandler PHB", PNLoggingMethod.LevelError);
            #endif
            
            CustomEventArgs cea = ea as CustomEventArgs;

            try {
                if ((cea != null) && (cea.CurrRequestType.Equals(PNCurrentRequestType.PresenceHeartbeat))) {
                    #if (ENABLE_PUBNUB_LOGGING)
                    this.PubNubInstance.PNLog.WriteToLog ("PHB cea not null", PNLoggingMethod.LevelError);
                    #endif
                    
                    PresenceHeartbeatHandler (cea);

                }                
            } catch (Exception ex) {
                #if (ENABLE_PUBNUB_LOGGING)
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("WebRequestCompleteHandler: Exception={0}", ex.ToString ()), PNLoggingMethod.LevelError);
                #endif
            }
        }

        internal void StopPresenceHeartbeat ()
        {
            keepPresenceHearbeatRunning = false;
            PubNubInstance.SubWorker.RequestSentAt =0;
            if (isPresenceHearbeatRunning)
            {
                #if (ENABLE_PUBNUB_LOGGING)
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("Stopping PresenceHeartbeat "), PNLoggingMethod.LevelInfo);
                #endif
                
                isPresenceHearbeatRunning = false;
                webRequest.AbortRequest (webRequestId, false);
            }
        }

        private void PresenceHeartbeatHandler (CustomEventArgs cea){
            #if (ENABLE_PUBNUB_LOGGING)
            this.PubNubInstance.PNLog.WriteToLog (string.Format ("PresenceHeartbeatHandler keepPresenceHearbeatRunning={0} isPresenceHearbeatRunning={1}", keepPresenceHearbeatRunning, isPresenceHearbeatRunning), PNLoggingMethod.LevelError);
            #endif

            isPresenceHearbeatRunning = false;

            #if (ENABLE_PUBNUB_LOGGING)
            if (cea.IsTimeout || cea.IsError) {
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("PresenceHeartbeatHandler: Presence Heartbeat timeout={0}", cea.Message.ToString ()), PNLoggingMethod.LevelError);
            }else {
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("PresenceHeartbeatHandler: Presence Heartbeat response: {0}", cea.Message.ToString ()), PNLoggingMethod.LevelInfo);
            }
            #endif

            if (keepPresenceHearbeatRunning) {
                #if (ENABLE_PUBNUB_LOGGING)
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("PresenceHeartbeatHandler: Restarting PresenceHeartbeat"), PNLoggingMethod.LevelInfo);
                #endif
                RunPresenceHeartbeat (true, PubNubInstance.PNConfig.PresenceInterval);
            }
        }

        void StartPresenceHeartbeat (bool pause, int pauseTime)
        {
            try {
                string channelsJsonState;
                int allNonPresenceChannelsOrChannelGroupsCount;
                string channels;
                string channelGroups;

                if (RunIndependentOfSubscribe){
                    channelsJsonState = State;
                    allNonPresenceChannelsOrChannelGroupsCount = ((ChannelGroups.Length>0)?ChannelGroups.Split(',').Count():0) + ((Channels.Length>0)?Channels.Split(',').Count():0);
                    Debug.Log("allNonPresenceChannelsOrChannelGroupsCount:"+ allNonPresenceChannelsOrChannelGroupsCount);
                    channels = Channels;
                    channelGroups = ChannelGroups;
                } else {
                    channelsJsonState = PubNubInstance.SubscriptionInstance.CompiledUserState;
                    allNonPresenceChannelsOrChannelGroupsCount = PubNubInstance.SubscriptionInstance.AllNonPresenceChannelsOrChannelGroups.Count;
                    channels = Helpers.GetNamesFromChannelEntities(PubNubInstance.SubscriptionInstance.AllNonPresenceChannelsOrChannelGroups, false);
                    channelGroups = Helpers.GetNamesFromChannelEntities(PubNubInstance.SubscriptionInstance.AllNonPresenceChannelsOrChannelGroups, true);
                }

                if (allNonPresenceChannelsOrChannelGroupsCount > 0){
                    isPresenceHearbeatRunning = true;
                    Uri request = BuildRequests.BuildPresenceHeartbeatRequest(
                        channels,
                        channelGroups,
                        channelsJsonState,
                        this.PubNubInstance,
                        null
                    );

                    RequestState requestState = new RequestState ();
                    requestState.OperationType = PNOperationType.PNPresenceHeartbeatOperation;
                    requestState.URL = request.OriginalString; 
                    requestState.Timeout = PubNubInstance.PNConfig.NonSubscribeTimeout;
                    requestState.Pause = pauseTime;
                    requestState.Reconnect = pause;
                    
                    #if (ENABLE_PUBNUB_LOGGING)
                    this.PubNubInstance.PNLog.WriteToLog (string.Format ("presenceheartbeat: /presence/ request.OriginalString {0} ", request.OriginalString ), PNLoggingMethod.LevelError);
                    #endif

                    webRequestId = webRequest.Run(requestState);

                    #if (ENABLE_PUBNUB_LOGGING)
                    this.PubNubInstance.PNLog.WriteToLog (string.Format ("StartPresenceHeartbeat: PresenceHeartbeat running, {0} ", pauseTime), PNLoggingMethod.LevelInfo);
                    #endif
                } 
                #if (ENABLE_PUBNUB_LOGGING)
                else {
                    this.PubNubInstance.PNLog.WriteToLog (string.Format ("StartPresenceHeartbeat: AllNonPresenceChannelsOrChannelGroups < 0 "), PNLoggingMethod.LevelInfo);
                }
                #endif
            }
            catch (Exception ex) {
                #if (ENABLE_PUBNUB_LOGGING)
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("StartPresenceHeartbeat: PresenceHeartbeat exception {0}", ex.ToString ()), PNLoggingMethod.LevelError);
                #endif
            }
        }
        
        internal void RunPresenceHeartbeat (bool pause, int pauseTime)
        {
            #if (ENABLE_PUBNUB_LOGGING)
            this.PubNubInstance.PNLog.WriteToLog (string.Format ("RunPresenceHeartbeat keepPresenceHearbeatRunning={0} isPresenceHearbeatRunning={1}", keepPresenceHearbeatRunning, isPresenceHearbeatRunning), PNLoggingMethod.LevelError);
            #endif

            if ((PubNubInstance.SubWorker.RequestSentAt > 0) && !RunIndependentOfSubscribe){
                int timediff = PubNubInstance.PNConfig.PresenceInterval - (DateTime.UtcNow.Second - PubNubInstance.SubWorker.RequestSentAt);
                if(timediff > 10){
                    #if (ENABLE_PUBNUB_LOGGING)
                    this.PubNubInstance.PNLog.WriteToLog (string.Format ("presenceheartbeat /presence/ timediff: {0} ", timediff), PNLoggingMethod.LevelInfo);
                    #endif

                    StopPresenceHeartbeat();
                    pause = true;
                }
            }
            
            keepPresenceHearbeatRunning = true;
            if (!isPresenceHearbeatRunning) {
                StartPresenceHeartbeat (pause, pauseTime);
            }
            #if (ENABLE_PUBNUB_LOGGING)
            else {
                this.PubNubInstance.PNLog.WriteToLog (string.Format ("RunPresenceHeartbeat: PresenceHeartbeat Already Running "), PNLoggingMethod.LevelInfo);
            }
            #endif
        }

    }
}