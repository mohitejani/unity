using System;
using System.Collections.Generic;
using System.Linq;

namespace PubNubAPI
{
    public class UpdateSpaceRequestBuilder : PubNubNonSubBuilder<UpdateSpaceRequestBuilder, PNSpaceResult>, IPubNubNonSubscribeBuilder<UpdateSpaceRequestBuilder, PNSpaceResult>
    {
        private PNUserSpaceInclude[] UpdateSpaceInclude { get; set; }
        private string UpdateSpaceID { get; set; }
        private string UpdateSpaceName { get; set; }
        private string UpdateSpaceDescription { get; set; }
        private Dictionary<string, object> UpdateSpaceCustom { get; set; }

        public UpdateSpaceRequestBuilder(PubNubUnity pn) : base(pn, PNOperationType.PNUpdateSpaceOperation)
        {
        }

        #region IPubNubBuilder implementation
        public void Async(Action<PNSpaceResult, PNStatus> callback)
        {
            this.Callback = callback;
            base.Async(this);
        }
        #endregion

        public UpdateSpaceRequestBuilder Include(PNUserSpaceInclude[] include)
        {
            UpdateSpaceInclude = include;
            return this;
        }

        public UpdateSpaceRequestBuilder ID(string id)
        {
            UpdateSpaceID = id;
            return this;
        }

        public UpdateSpaceRequestBuilder Name(string name)
        {
            UpdateSpaceName = name;
            return this;
        }

        public UpdateSpaceRequestBuilder Description(string description)
        {
            UpdateSpaceDescription = description;
            return this;
        }

        public UpdateSpaceRequestBuilder Custom(Dictionary<string, object> custom)
        {
            UpdateSpaceCustom = custom;
            return this;
        }

        protected override void RunWebRequest(QueueManager qm)
        {
            RequestState requestState = new RequestState();
            requestState.OperationType = OperationType;
            requestState.httpMethod = HTTPMethod.Patch;

            var cub = new
            {
                id = UpdateSpaceID,
                name = UpdateSpaceName,
                description = UpdateSpaceDescription,
                custom = UpdateSpaceCustom,
            };

            string jsonUserBody = Helpers.JsonEncodePublishMsg(cub, "", this.PubNubInstance.JsonLibrary, this.PubNubInstance.PNLog);
#if (ENABLE_PUBNUB_LOGGING)
            this.PubNubInstance.PNLog.WriteToLog(string.Format("jsonUserBody: {0}", jsonUserBody), PNLoggingMethod.LevelInfo);
#endif
            requestState.POSTData = jsonUserBody;

            string[] includeString = Enum.GetValues(typeof(PNUserSpaceInclude))
                .Cast<int>()
                .Select(x => x.ToString())
                .ToArray();

            Uri request = BuildRequests.BuildObjectsUpdateSpaceRequest(
                    UpdateSpaceID,
                    string.Join(",", includeString),
                    this.PubNubInstance,
                    this.QueryParams
                );
            base.RunWebRequest(qm, request, requestState, this.PubNubInstance.PNConfig.NonSubscribeTimeout, 0, this);
        }

        protected override void CreatePubNubResponse(object deSerializedResult, RequestState requestState)
        {
            object[] c = deSerializedResult as object[];
            //{"status":200,"data":{"id":"id17","name":"name 17","externalId":null,"profileUrl":null,"email":"email 17","created":"2019-10-25T10:52:58.366074Z","updated":"2019-10-25T10:52:58.366074Z","eTag":"AdnSjuyx7KmDngE"}}
            PNSpaceResult pnSpaceResult = new PNSpaceResult();
            PNStatus pnStatus = new PNStatus();

            try
            {
                Dictionary<string, object> dictionary = deSerializedResult as Dictionary<string, object>;

                if (dictionary != null)
                {
                    object objData;
                    dictionary.TryGetValue("data", out objData);
                    if (objData != null)
                    {
                        Dictionary<string, object> objDataDict = objData as Dictionary<string, object>;
                        if (objDataDict != null)
                        {
                            pnSpaceResult.ID = Utility.ReadMessageFromResponseDictionary(objDataDict, "id");
                            pnSpaceResult.Name = Utility.ReadMessageFromResponseDictionary(objDataDict, "name");
                            pnSpaceResult.Description = Utility.ReadMessageFromResponseDictionary(objDataDict, "description");
                            pnSpaceResult.Created = Utility.ReadMessageFromResponseDictionary(objDataDict, "created");
                            pnSpaceResult.Updated = Utility.ReadMessageFromResponseDictionary(objDataDict, "updated");
                            pnSpaceResult.ETag = Utility.ReadMessageFromResponseDictionary(objDataDict, "eTag");
                            pnSpaceResult.Custom = Utility.ReadDictionaryFromResponseDictionary(objDataDict, "custom");

                        }
                        else
                        {
                            pnSpaceResult = null;
                            pnStatus = base.CreateErrorResponseFromException(new PubNubException("objDataDict null"), requestState, PNStatusCategory.PNUnknownCategory);
                        }
                    }
                    else
                    {
                        pnSpaceResult = null;
                        pnStatus = base.CreateErrorResponseFromException(new PubNubException("objData null"), requestState, PNStatusCategory.PNUnknownCategory);
                    }
                }
            }
            catch (Exception ex)
            {
                pnSpaceResult = null;
                pnStatus = base.CreateErrorResponseFromException(ex, requestState, PNStatusCategory.PNUnknownCategory);
            }
            Callback(pnSpaceResult, pnStatus);

        }

    }

}