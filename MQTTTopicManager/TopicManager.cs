/*Author Dr Shuo Ding La Trobe University, 2021 */


using Microsoft.Extensions.Logging;
using MongoAccess.DataAccess;
using MongoAccess.Model;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using RealtimeSignal;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;
using System.Globalization;
using MQTTnet.Extensions.ManagedClient;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using System.Collections;
using MQTTnet.Client.Receiving;



namespace MQTTTopicManager
{
    public class TopicManager : ITopicManager
    {
        private readonly DeviceMongoDriver<DeviceObject> _driverDeviceObject;
        private readonly DeviceMongoDriver<TimeSeriesObject> _driverTimeSeries;
        private readonly ManagedMqttClientOptions _mqttClientOptions;
        private readonly ManagedMqttClient _mqttClient;
        private readonly ILogger _logger;
        private IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            set { _serviceProvider = value; }
        }
        public SelectionModel MySelecter;
        public TopicManager(ManagedMqttClientOptions ManagedmqttClientOptions, DeviceMongoDriver<DeviceObject> driver_DeviceObject, DeviceMongoDriver<TimeSeriesObject> driver_timeSeries, ILogger logger = null)
        {
            _mqttClientOptions = ManagedmqttClientOptions;
            _driverDeviceObject = driver_DeviceObject;
            _driverTimeSeries = driver_timeSeries;
            _mqttClient = (ManagedMqttClient)(new MqttFactory()).CreateManagedMqttClient();
            if (logger != null)
                _logger = logger;
            else
                _logger = new LoggerFactory().AddFile("MQTTLogs/log-{Date}.txt").CreateLogger(nameof(TopicManager));

            MySelecter = new SelectionModel();
        }
        public async Task TopicManagerInit(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            await ConnectMqttClientAsync();
            await SendAliveMessageAsync();
        }
        private async Task SendAliveMessageAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (_mqttClient.IsConnected)
                        _logger.LogInformation("MQTT is Connected");
                    if (_mqttClient.IsStarted)
                        _logger.LogInformation("MQTT is Started");
                    if (!_mqttClient.IsConnected)
                        _logger.LogInformation("MQTT is NOT Connected");
                    if (!_mqttClient.IsStarted)
                        _logger.LogInformation("MQTT is NOT Started");
                    // Yield the rest of the time slice.

                    //_mqttClient.Dispose();
                    await ConnectMqttClientAsync();
                    _logger.LogInformation("Reconnct MQTT");
                    Thread.Sleep(30000);
                }
            });
        }

        private async Task SubscribeAsync()
        {
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.SensorId != null && o.SensorId != "UNASSIGNED");
            List<string> Subscriptions = new List<string>();
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                String subscribedTopic = "ToServer/" + obj.SensorId;
                Subscriptions.Add(subscribedTopic);
            }
            List<MqttTopicFilter> mqttTopicFilters = new List<MqttTopicFilter>();
            foreach (string subscription in Subscriptions)
            {
                mqttTopicFilters.Add(new MqttTopicFilter
                {
                    Topic = subscription,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                });
            }
            MqttTopicFilter[] mqttTopicArray = mqttTopicFilters.ToArray();
            await _mqttClient.SubscribeAsync(mqttTopicArray);
        }
        public async Task SubscribeUnSubscribeAsync(string SensorId, string wakeuptimer, bool isSubscribe)
        {
            if (!String.IsNullOrEmpty(SensorId))
            {
                string TopicToServer = "ToServer/" + SensorId;
                string TopicToSensor = "ToSensor/" + SensorId;
                List<MqttTopicFilter> mqttTopicFilters = new List<MqttTopicFilter>();
                mqttTopicFilters.Add(new MqttTopicFilter
                {
                    Topic = TopicToServer,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                });
                MqttTopicFilter[] mqttTopicArray = mqttTopicFilters.ToArray();
                List<string> topiclist = new List<string>();
                topiclist.Add(TopicToServer);
                if (isSubscribe)
                {
                    _logger.LogInformation($"Subscribe to Topic {TopicToServer} \n");
                    await _mqttClient.SubscribeAsync(mqttTopicArray);
                }
                else
                {
                    TimeSeriesObject requestProperties = new TimeSeriesObject();
                    requestProperties.SensorId = SensorId;
                    requestProperties.D3 = "";
                    requestProperties.Command = "SCfg";
                    int wakeuptimersent = Int16.Parse(wakeuptimer);
                    TimeSeriesObjectForJsonSentSCfg JsonMsg = requestProperties.ConvertToTimeSeriesObjectForJsonSentSCfg(wakeuptimersent);
                    //send last message to old sensor                  
                    await PublishMessageAsync(TopicToSensor, JsonSerializer.Serialize(JsonMsg));
                    await _driverTimeSeries.InsertObjectAsync(requestProperties); //insert this last message to times series collection                                
                    _logger.LogInformation($"Unsubscribe to Topic {TopicToServer} \n");
                    await _mqttClient.UnsubscribeAsync(topiclist);
                }
            }
            Thread.Sleep(1500);
        }
        public async Task InsertDeviceObject(string SensorId, string Nickname, string Vendor, string Campus, string Building, string Room, string AirConNo, string IRCode, string Note, int setTemp, int roomTemp, int FanSpeed, bool PowerOnOff, string com, string mode, string d3)
        {
            DeviceObject deviceObject = new DeviceObject();
            deviceObject.SensorId = SensorId;
            deviceObject.NickName = Nickname;
            deviceObject.Campus = Campus;
            deviceObject.Vendor = Vendor;
            deviceObject.Building = Building;
            deviceObject.Room = Room;
            deviceObject.AirConNo = AirConNo;
            deviceObject.IRCode = IRCode;
            deviceObject.Note = Note;
            //deviceObject.Properties.n = new Dictionary item.Add("temp", "3");
            deviceObject.CurrentProperties = new TimeSeriesObject();
            deviceObject.CurrentProperties.SensorId = SensorId;
            deviceObject.CurrentProperties.SetTemperature = setTemp;
            deviceObject.CurrentProperties.RoomTemperature = roomTemp;
            deviceObject.CurrentProperties.FanSpeed = FanSpeed;
            deviceObject.CurrentProperties.PowerOnOff = PowerOnOff;
            deviceObject.CurrentProperties.Command = com;
            deviceObject.CurrentProperties.Mode = mode;
            deviceObject.CurrentProperties.D3 = d3;

            deviceObject.RequestProperties = new TimeSeriesObject();
            deviceObject.RequestProperties.SensorId = SensorId;
            deviceObject.RequestProperties.SetTemperature = setTemp;
            deviceObject.RequestProperties.RoomTemperature = roomTemp;
            deviceObject.RequestProperties.FanSpeed = FanSpeed;
            deviceObject.RequestProperties.PowerOnOff = PowerOnOff;
            deviceObject.RequestProperties.Command = com;
            deviceObject.RequestProperties.Mode = mode;
            deviceObject.RequestProperties.D3 = d3;

            await _driverDeviceObject.InsertObjectAsync(deviceObject);
            await _driverTimeSeries.InsertObjectAsync(deviceObject.CurrentProperties);
            Console.WriteLine("### Insert one data into DeviceObject Collection###");
        }
        public string GetAllCampusList()
        {
            MySelecter.Clear();
            var campusList = _driverDeviceObject.Collection.AsQueryable().Select(c => new { c.Campus }).Distinct().ToList();
            return JsonSerializer.Serialize(campusList.ToList());
        }
        /*
        public string GetAllBuildingList(string campus)
        {
            Selecter.Reset();
            if (!String.IsNullOrEmpty(campus))
                Selecter.Campus = campus;
            var buildingList = _driverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == campus).Select(c => new { c.Building }).Distinct().ToList();
            return JsonSerializer.Serialize(buildingList.ToList());
        }
        */
        public string GetMultipleSelectionBuildingList(string campusList)
        {
            if (String.IsNullOrEmpty(campusList))
                campusList = "Impossible";
            MySelecter.NetZeroNetwork.Campuses.Clear();
            string[] campusSplit = campusList.Split(',');
            var buildingList = _driverDeviceObject.Collection.AsQueryable().Where(c => campusSplit.Contains(c.Campus) && (!c.SensorId.Equals("UNASSIGNED"))).Select(c => new
            {
                c.Campus,
                c.Building
            }).Distinct().ToList();
            //prepare topic for confirmed campus
            if (campusList != "Impossible")
            {
                MySelecter.NetZeroNetwork.Campuses.Clear();
                for (int i = 0; i < campusSplit.Length; i++)
                {
                    Campus newCampus = new Campus();
                    newCampus.Name = campusSplit[i];
                    MySelecter.NetZeroNetwork.Campuses.Add(newCampus);
                    //we need to show the campus setting config
                    //   DeviceObject obj = _driverDeviceObject.Collection.AsQueryable().Where(c => campusSplit[i].Contains(c.Campus) && c.ConfigHierarchy == 1).Single();
                    //    string configInfo = obj.GetLatestConfigInfo();
                }
            }
            string res = JsonSerializer.Serialize(buildingList.ToList());
            MySelecter.GenerateTopicList();
            string configstring = GetMultipleSelectionConfigList("");
            return res + "KINGOFWORLD" + configstring;
        }
        /*
        public string GetMultipleSelectionBuildingConfigList(string campusList)
        {
            if (String.IsNullOrEmpty(campusList))
                return "";
            string[] campusSplit = campusList.Split(',');
           // MySelecter.NetZeroNetwork.Campuses.Clear();
           //  for (int i = 0; i < campusSplit.Length; i++)
          //   {
          //       Campus newCampus = new Campus();
          //       newCampus.Name = campusSplit[i];
          ////       MySelecter.NetZeroNetwork.Campuses.Add(newCampus);
          //   }
         //    MySelecter.GenerateTopicList(); 
            List<string> campustopiclist = new List<string>();
            foreach (string ele in campusSplit)
            {
                string newtopic = "AU/" + ele + "/All/All/All/1";
                campustopiclist.Add(newtopic);
            }
           //foreach (string ele in campustopiclist)
           //  {
          //       var resList = _driverTimeSeries.Collection.AsQueryable().Where(c => c.Mode.Contains("D")).ToList();
          //       if (resList != null)
          //       {
           //          List<Schedule> scheduelist = resList[0].ScheduleBlob.ScheduleList;
          //           string scheduleListJson = JsonSerializer.Serialize(scheduelist.ToList());
          //       }
         //    } 
            
         //   var buildingList = _driverTimeSeries.Collection.AsQueryable().Where(c => campustopiclist.Any(x=>c.Selecter.TopicList.Contains(x))).Select(c //=> new
        //    {
         //       c.Id,
         //       c.Timestamp,
         //       c.Mode,
         //       c.ScheduleBlob.ScheduleList
        //    }).ToList();  
            var configList = _driverTimeSeries.Collection.AsQueryable().Where(c => campustopiclist.Any(x => c.Selecter.TopicList.Contains(x))).Select(c => new ScheduleResult
            {
                Timestamp = c.Timestamp,
                ScheduleBlob = c.ScheduleBlob,
                Selecter = c.Selecter
            }).OrderByDescending(c => c.Timestamp).ToList();
            List<ScheduleResult> listofResult = configList.ToList();
            List<ScheduleUIJson> listofUIResult = new List<ScheduleUIJson>();
            foreach (string campusele in campusSplit)
            {
                foreach (ScheduleResult uiele in listofResult)
                {
                    string Campus = campusele;
                    string Building = "All";
                    string Room = "All";
                    List<string> topicstringlist = uiele.Selecter.TopicList;
                    foreach (string topic in topicstringlist)
                    {
                        string tem = "AU/" + Campus + "/" + Building + "/" + Room + "/All/1";
                        if (topic == tem)
                        {
                            foreach (Schedule scheduleele in uiele.ScheduleBlob.ScheduleList)
                            {
                                ScheduleUIJson newjson = scheduleele.GetScheduleUIJson();
                                newjson.Campus = Campus;
                                newjson.Building = Building;
                                newjson.Room = Room;
                                listofUIResult.Add(newjson);
                            }
                        }
                    }
                }
            }
            string res = JsonSerializer.Serialize(listofUIResult.ToList());
            return res;  //send to UI to process the JSON 
        }*/
        public string GetMultipleSelectionConfigList(string input)
        {
            List<string> te = MySelecter.TopicList;
            string[] topiclist = te.ToArray();
            var configList = _driverTimeSeries.Collection.AsQueryable().Where(c => c.Command.Contains("UCfgSingleTopic") && topiclist.Any(x => c.Selecter.TopicList.Contains(x))).Select(c => new ScheduleResult
            {
                _id = c._id,
                Timestamp = c.Timestamp,
                ScheduleBlob = c.ScheduleBlob,
                Selecter = c.Selecter
            }).OrderByDescending(c => c.Timestamp).ToList();
            List<ScheduleResult> listofResult = configList.ToList();
            List<ScheduleUIJson> listofUIResult = new List<ScheduleUIJson>();
            foreach (string selectedtopic in topiclist)
            {
                string[] topicarray = selectedtopic.Split('/');
                string Campus = topicarray[1];
                string Building = topicarray[2];
                string Room = topicarray[3];
                foreach (ScheduleResult uiele in listofResult)
                {
                    string Id = uiele._id.ToString();
                    List<string> topicstringlist = uiele.Selecter.TopicList;
                    foreach (string topic in topicstringlist)
                    {
                        if (topic == selectedtopic)
                        {
                            foreach (Schedule scheduleele in uiele.ScheduleBlob.ScheduleList)
                            {
                                if (scheduleele.Priority >= 0)
                                {
                                    ScheduleUIJson newjson = scheduleele.GetScheduleUIJson();
                                    newjson.Campus = Campus;
                                    newjson.Building = Building;
                                    newjson.Room = Room;
                                    newjson.Id = Id + "-COMBINE-" + scheduleele.ScheduleIdx.ToString();
                                    newjson.Topic = topic;
                                    listofUIResult.Add(newjson);
                                }
                            }
                        }
                    }
                }
            }
            string res = JsonSerializer.Serialize(listofUIResult.ToList());
            return res;  //send to UI to process the JSON 
        }
        public string GetMultipleSelectionRoomList(string buildingList)
        {
            if (String.IsNullOrEmpty(buildingList))
                buildingList = "Impossible";
            string[] builingSplit = buildingList.Split(',');
            var roomList = _driverDeviceObject.Collection.AsQueryable().Where(c => builingSplit.Contains(c.Building) && (!c.SensorId.Equals("UNASSIGNED"))).Select(c => new { c.Campus, c.Building, c.Room }).Distinct().ToList();

            MySelecter.NetZeroNetwork.ClearAllBuildings();
            foreach (var obj in roomList)
            {
                Campus thisCampus = MySelecter.NetZeroNetwork.Campuses.Find(x => x.Name == obj.Campus);
                bool findSameBuilding = false;
                foreach (Buildings ele in thisCampus.Buildings)
                {
                    if (ele.Name == obj.Building)
                    {
                        findSameBuilding = true;
                    }
                }
                if (!findSameBuilding)
                {   //create new building in this campus 
                    Buildings newBuilding = new Buildings();
                    newBuilding.Name = obj.Building;
                    thisCampus.Buildings.Add(newBuilding);
                }
            }
            string res = JsonSerializer.Serialize(roomList.ToList());
            MySelecter.GenerateTopicList();
            List<string> list = MySelecter.TopicList;
            string configstring = GetMultipleSelectionConfigList("");
            return res + "KINGOFWORLD" + configstring;
        }
        /*
        public string GetAllRoomList(string building)
        {
            Selecter.Room = "All";
            Selecter.messageList.Clear();

            if (!String.IsNullOrEmpty(building))
                Selecter.Building = building;

            var roomList = _driverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == Selecter.Campus && c.Building == Selecter.Building).Select(c => new { c.Room }).Distinct().ToList();
            return JsonSerializer.Serialize(roomList.ToList());
        }
        //no use  
        public async Task<string> GetRoomData(string room)
        {
            Selecter.messageList.Clear();

            if (!String.IsNullOrEmpty(room))
                Selecter.Room = room;

            //  var airconList = _driverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == Selecter.Campus && c.Building == Selecter.Building &&// c.Room == room).Select(c => new { c.Room }).Distinct().ToList();

            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(c => c.Campus == Selecter.Campus && c.Building == Selecter.Building && c.Room == room);

            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                WarningMessage newRoomData = new WarningMessage();
                newRoomData.SensorId = obj.SensorId;
                newRoomData.Vendor = obj.Vendor;
                newRoomData.Campus = obj.Campus;
                newRoomData.Building = obj.Building;
                newRoomData.Room = obj.Room;

                if (!(obj.CurrentProperties is null))
                {
                    newRoomData.Timestamp = obj.CurrentProperties.Timestamp.ToString();
                    newRoomData.Temperature = obj.CurrentProperties.Temperature.ToString();
                    newRoomData.FanSpeed = obj.CurrentProperties.FanSpeed.ToString();
                    newRoomData.PowerOnOff = obj.CurrentProperties.PowerOnOff.ToString();
                }
                Selecter.messageList.Add(newRoomData);
            }
            string result = JsonSerializer.Serialize(Selecter.messageList);
            return result;
        }*/
        virtual
     public async Task<string> MultipleRoomListData(string roomList)
        {
            if (String.IsNullOrEmpty(roomList))
            {
                return "";
            }
            //var airconList = _driverDeviceObject.Collection.AsQueryable().Where(c => c.Campus == Selecter.Campus && c.Building == Selecter.Building &&// c.Room == room).Select(c => new { c.Room }).Distinct().ToList();
            foreach (Campus Ele in MySelecter.NetZeroNetwork.Campuses)
            {
                Ele.ClearAllRooms();
            }
            string[] roomSplit = roomList.Split(',');
            string[] buildingonly = new string[roomSplit.Length];
            string[] roomonly = new string[roomSplit.Length];
            for (int j = 0; j < roomSplit.Length; j++)
            {
                string tem = roomSplit[j];
                string[] buildingroomsplit = tem.Split("-COMBINE-");
                string build = buildingroomsplit[0];
                string room = buildingroomsplit[1];
                buildingonly[j] = build;
                roomonly[j] = room;
            }
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(c => roomonly.Contains(c.Room) && buildingonly.Contains(c.Building) && (!c.SensorId.Equals("UNASSIGNED")));
            List<WarningMessage> tempList = new List<WarningMessage>();
            foreach (DeviceObject obj in obj_list)
            {
                string thisbuilding = obj.Building;
                string thisroom = obj.Room;
                string combine = thisbuilding + "-COMBINE-" + thisroom;
                if (roomList.Contains(combine))
                {
                    if (obj is null)
                        throw new Exception("Null device object found in MongoDB");
                    WarningMessage newRoomData = obj.GeWarningMessages();
                    Campus thisCampus = MySelecter.NetZeroNetwork.Campuses.Find(x => x.Name == obj.Campus);
                    Buildings thisBuilding = thisCampus.Buildings.Find(x => x.Name == obj.Building);
                    if (thisBuilding != null)
                    {
                        List<string> roomlist = thisBuilding.Rooms;
                        if (!roomlist.Contains(thisroom))
                        {
                            thisBuilding.AddRoom(newRoomData);
                            tempList.Add(newRoomData);
                        }
                    }
                }
            }
            string res = JsonSerializer.Serialize(tempList.ToList());
            MySelecter.GenerateTopicList();
            List<string> list = MySelecter.TopicList;
            string configstring = GetMultipleSelectionConfigList("");
            return res + "KINGOFWORLD" + configstring;
        }
        public async Task OperateConfigureTable(string jasondata, bool isDelete)
        {
            if (String.IsNullOrEmpty(jasondata))
                return;
            string jasontrim = jasondata.Trim();
            string[] jasonarray = jasontrim.Split("},{");
            string finaljason = "";
            for (int i = 0; i < jasonarray.Length; i++)
            {
                string jasonentry = jasonarray[i];
                string jasondata2 = jasonentry.Replace("\"WeekDays\":[", "\"WeekDays\":").Replace("],\"Temperature", ",\"Temperature");
                string[] split1 = jasondata2.Split("\"WeekDays\":");
                string right = split1[1];
                string[] rightsplit = right.Split(",\"Temperature");
                string thisone = rightsplit[0];
                string newlist = thisone.Replace("[", "").Replace("]", "").Replace("\"", "");
                newlist = "\"" + newlist + "\"";
                string newjson = split1[0] + "\"WeekDays\":" + newlist + ",\"Temperature" + rightsplit[1];
                finaljason += newjson;
                if (i != jasonarray.Length - 1)
                    finaljason += "},{";
            }
            string jasontoprocess = finaljason;
            List<ScheduleUIJson> listofConfigData = JsonSerializer.Deserialize<List<ScheduleUIJson>>(jasontoprocess);
            List<Task> tasklist = new List<Task>();
            _logger.LogInformation($"Receive UI request - need delete {isDelete} Json payload {jasondata}\n");
            foreach (ScheduleUIJson record in listofConfigData)
            {
                string[] split = record.Id.Split("-COMBINE-");
                string id = split[0];
                string scheduleindex = split[1];
                ObjectId myid = new ObjectId(id);
                TimeSeriesObject obj = await _driverTimeSeries.ReadSingleObjectAsync(o => o._id.Equals(myid) && o.Command.Equals("UCfgSingleTopic"));
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                else
                {
                    obj.Timestamp = DateTime.Now.ToLocalTime();
                    //find this schedule in this obj and make it disabled first 
                    foreach (Schedule ele in obj.ScheduleBlob.ScheduleList)
                    {
                        _logger.LogInformation($"CheckSchedule Temerature {ele.Temperature} Priority  {ele.Priority} index {ele.ScheduleIdx}\n");
                        if (ele.ScheduleIdx.ToString() == scheduleindex)
                        {
                            if (isDelete)
                            {
                                ele.Priority = -123;
                                _logger.LogInformation($"Deleted schedule {jasondata} \n");
                            }
                            else
                            {
                                _logger.LogInformation($"Modify schedule {jasondata} \n");
                                //update this entry based on UI input
                                if (record.PowerStatus == "ON")
                                    ele.PowerStatus = true;
                                if (record.PowerStatus == "OFF")
                                    ele.PowerStatus = false;

                                if (!string.IsNullOrEmpty(record.FanSpeed))
                                {
                                    if (record.FanSpeed == "SPEED_1")
                                        ele.FanSpeed = 0;
                                    if (record.FanSpeed == "SPEED_2")
                                        ele.FanSpeed = 1; 
                                    if (record.FanSpeed == "SPEED_3")
                                        ele.FanSpeed = 2;
                                    if (record.FanSpeed == "SPEED_4")
                                        ele.FanSpeed = 3;
                                    if (record.FanSpeed == "SPEED_5")
                                        ele.FanSpeed = 4;
                                    if(record.FanSpeed == "AUTO")
                                        ele.FanSpeed = 5;
                                    if(record.FanSpeed == "SILENT")
                                        ele.FanSpeed = 6;
                                }
                                    
                                if (!string.IsNullOrEmpty(record.Temperature))
                                    ele.Temperature = Int16.Parse(record.Temperature);
                                if (!string.IsNullOrEmpty(record.Mode))
                                    ele.Mode = record.Mode;
                                if (!string.IsNullOrEmpty(record.ResetTimer))
                                    ele.ResetTimer = Int16.Parse(record.ResetTimer);
                                if (!string.IsNullOrEmpty(record.Priority))
                                {
                                    if (record.Priority == "Daily")
                                        ele.Priority = 1;
                                    if (record.Priority == "Holiday")
                                        ele.Priority = 2;
                                    if (record.Priority == "Temporary")
                                        ele.Priority = 3;
                                }
                                BitArray weekdayarray = new BitArray(8);
                                int finalweekdayint = 0;
                                string weekday = record.WeekDays.Trim();

                                string[] weekdays = weekday.Split(",");
                                foreach (string thisday in weekdays)
                                {
                                    if (thisday == "M")
                                        weekdayarray.Set(1, true);
                                    if (thisday == "Tu")
                                        weekdayarray.Set(2, true);
                                    if (thisday == "We")
                                        weekdayarray.Set(3, true);
                                    if (thisday == "Th")
                                        weekdayarray.Set(4, true);
                                    if (thisday == "F")
                                        weekdayarray.Set(5, true);
                                    if (thisday == "Sa")
                                        weekdayarray.Set(6, true);
                                    if (thisday == "Su")
                                        weekdayarray.Set(0, true);
                                }
                                //0 is sunday, 1 is monday, 2 is tuesday 3 is wednesday, 4 is thur 5 is friday 6 is saturday  
                                int[] array = new int[1];
                                weekdayarray.CopyTo(array, 0);
                                finalweekdayint = array[0];
                                ele.WeekDays = finalweekdayint;
                                if (!string.IsNullOrEmpty(record.StartDate) && !string.IsNullOrEmpty(record.EndDate) &&
                                    !string.IsNullOrEmpty(record.StartTime) && !string.IsNullOrEmpty(record.EndTime))
                                {
                                    CultureInfo enAU = new CultureInfo("en-AU");
                                    DateTime sdate = DateTime.Parse(record.StartDate, enAU);
                                    DateTime edate = DateTime.Parse(record.EndDate, enAU);
                                    DateTime stime = new DateTime();
                                    DateTime etime = new DateTime();
                                    if ((record.StartTime.Contains("AM") || record.StartTime.Contains("PM")) && (record.EndTime.Contains("AM") || record.EndTime.Contains("PM")))
                                    {
                                        stime = DateTime.Parse(record.StartTime);
                                        etime = DateTime.Parse(record.EndTime);
                                    }
                                    ele.StartDate = sdate;
                                    ele.EndDate = edate;
                                    ele.StartTime = stime;
                                    ele.EndTime = etime;
                                }
                            }
                        }
                    }
                }
                await _driverTimeSeries.UpdateObjectAsync(obj, obj._id);
            }
            // await Task.WhenAll(tasklist);
            Thread.Sleep(3000);
            int Count = 0;
            List<Task> publishtasklist = new List<Task>();
            //we need to publish a new meg to announce the change of to this topic
            foreach (ScheduleUIJson record in listofConfigData)
            {
                string affectedtopic = record.Topic;
                if (!String.IsNullOrEmpty(affectedtopic))
                {
                    var configList = _driverTimeSeries.Collection.AsQueryable().Where(c => c.Selecter.TopicList.Contains(affectedtopic) && c.Command.Contains("UCfgSingleTopic")).Select(c => new ScheduleResult
                    {
                        Timestamp = c.Timestamp,
                        ScheduleBlob = c.ScheduleBlob,
                        Selecter = c.Selecter
                    }).OrderByDescending(c => c.Timestamp).ToList();
                    List<ScheduleResult> listofResult = configList.ToList();
                    List<Schedule> scheduleforthistopic = new List<Schedule>();
                    scheduleforthistopic.Clear();
                    foreach (ScheduleResult scheduleresultele in listofResult)
                    {
                        List<string> topicstringlist = scheduleresultele.Selecter.TopicList;
                        foreach (string topic in topicstringlist)
                        {
                            if (topic == affectedtopic)
                            {
                                foreach (Schedule scheduleele in scheduleresultele.ScheduleBlob.ScheduleList)
                                {
                                    Schedule elecopy = scheduleele.GetCopy();
                                    elecopy.StartDate = elecopy.StartDate.ToLocalTime();
                                    elecopy.EndDate = elecopy.EndDate.ToLocalTime();
                                    elecopy.StartTime = elecopy.StartTime.ToLocalTime();
                                    elecopy.EndTime = elecopy.EndTime.ToLocalTime();
                                    bool foundsame = false;
                                    foreach (Schedule existing in scheduleforthistopic)
                                    {
                                        if (elecopy.isSame(existing))
                                            foundsame = true;
                                    }
                                    if (!foundsame)
                                    {
                                        scheduleforthistopic.Add(elecopy);
                                    }
                                }
                            }
                        }
                    }
                    TopicSchedule OneTopicSchedule = new TopicSchedule(affectedtopic, scheduleforthistopic);
                    ScheduleJSONMsg payload = OneTopicSchedule.GenerateFirmwareMsg(1, 16, 28, 39);
                    string payloadJSON = JsonSerializer.Serialize(payload);
                    await PublishMessageAsync(affectedtopic, payloadJSON);
                    Thread.Sleep(200);
                    TimeSeriesObject updateConfig = new TimeSeriesObject();
                    updateConfig.SensorId = "";
                    updateConfig.D3 = payloadJSON + " Topic " + affectedtopic;
                    updateConfig.Command = "ModifySCfgSchedule";
                    updateConfig.ScheduleBlob.ListOfTopicAllSchedule.Add(OneTopicSchedule);
                    await _driverTimeSeries.InsertObjectAsync(updateConfig); //insert this last message to times series collection
                    Thread.Sleep(500);
                    _logger.LogInformation($"Insert Modified Schedule for topic {affectedtopic} no {Count}and Json payload {payloadJSON}\n");
                    Count++;
                }
            }
            await Task.WhenAll(publishtasklist);
            Thread.Sleep(1000);
        }
        public async Task UpdateSensorID(string id, string sensorid, string vendor, string campus, string building, string room, string wakeuptimer, bool isDelete)
        {
            string newid = "";
            string newsensorId = "";
            string newvendor = "";
            string newcampus = "";
            string newbuilding = "";
            string newroom = "";
            string newwakeuptimer = "";
            if (String.IsNullOrEmpty(id) || String.IsNullOrEmpty(sensorid) || String.IsNullOrEmpty(vendor) || String.IsNullOrEmpty(campus)
                || String.IsNullOrEmpty(building) || String.IsNullOrEmpty(room) || String.IsNullOrEmpty(wakeuptimer))
                return;

            newid = id;
            newsensorId = sensorid.ToUpper();
            newvendor = vendor.ToUpper();
            newcampus = campus.ToUpper();
            newbuilding = building.ToUpper();
            newroom = room.ToUpper();
            newwakeuptimer = wakeuptimer.ToUpper();
            bool needpublish = false;
            if ((newid == "NEWENTRY"))
            {
                if (newsensorId != "UNASSIGNED")
                {
                    string modename = "AUTO";
                    string airconno = "UNASSIGNED";
                    string IRcode = "UNASSIGNED";
                    bool power = true;
                    string nickname = newcampus + "#" + newbuilding + "#" + newroom + "#" + newvendor;
                    string com = "SVal";
                    string d3 = "SVal";
                    string note = newwakeuptimer;
                    int roomtemp = 22;
                    int settemp = 22;
                    int fanspeed = 1;
                    await InsertDeviceObject(newsensorId, nickname, newvendor, newcampus, newbuilding, newroom, airconno, IRcode, note, settemp, roomtemp, fanspeed, power, com, modename, d3);
                    //subscribe 
                    _logger.LogInformation($"Add a new sensor and subscribe:  {newcampus} {newbuilding} {newroom} {newsensorId} {newwakeuptimer} {newvendor}\n");
                    needpublish = true;
                }
            }
            else
            {
                //this is existing entry and it has its long _id 
                ObjectId myid = new ObjectId(newid);
                DeviceObject obj = await _driverDeviceObject.ReadSingleObjectAsync(o => o._id.Equals(myid));
                if (obj is null)
                    throw new Exception("The device does not exist or subscribed to the broker.");
                else
                {
                    string oldsensorid = obj.SensorId.ToUpper();
                    string oldvendor = obj.Vendor.ToUpper();
                    string oldroom = obj.Room.ToUpper();
                    string oldbuilding = obj.Building.ToUpper();
                    string oldcampus = obj.Campus.ToUpper();
                    string oldwakeuptimer = obj.Note.ToUpper();

                    if (oldsensorid != "UNASSIGNED")
                    {
                        //unscribe
                        await SubscribeUnSubscribeAsync(oldsensorid, oldwakeuptimer, false);
                    }
                    if (newsensorId != "UNASSIGNED")
                    {
                        //scribe
                        await SubscribeUnSubscribeAsync(newsensorId, newwakeuptimer, true);
                    }

                    if (isDelete)
                    {
                        obj.SensorId = "UNASSIGNED";
                        obj.NickName = "DELETEDFOREVER"; 
                        obj.Vendor = "UNASSIGNED";
                        obj.Note = "60";
                        obj.RequestProperties.SensorId = obj.SensorId;
                        obj.CurrentProperties.SensorId = obj.SensorId;
                    }
                    else
                    {
                        obj.SensorId = newsensorId;
                        obj.Vendor = newvendor;
                        obj.Note = newwakeuptimer;
                        obj.Room = newroom;
                        obj.RequestProperties.SensorId = newsensorId;
                        obj.CurrentProperties.SensorId = newsensorId;
                    }
                    await _driverDeviceObject.UpdateObjectAsync(obj, obj._id); //update sensor ID
                    _logger.LogInformation($"Update Sensor Info in DB:  {newcampus} {newbuilding} {newroom} {newsensorId} {newwakeuptimer} {newvendor}\n");
                    if (oldsensorid != "UNASSIGNED" && oldvendor != "UNASSIGNED" && oldwakeuptimer != "UNASSIGNED")
                    {
                        if (newsensorId != "UNASSIGNED" && newvendor != "UNASSIGNED" && newwakeuptimer != "UNASSIGNED")
                        {
                            if (newwakeuptimer != oldwakeuptimer || newvendor != oldvendor || newsensorId != oldsensorid || newroom != oldroom)
                            {
                                _logger.LogInformation($"Replace sensor info - Old:{oldcampus} {oldbuilding}  {oldroom} {oldsensorid} {oldvendor} {oldwakeuptimer} New:    {newcampus} {newbuilding} {newroom}  {newsensorId}  {newvendor} {newwakeuptimer} \n");
                                needpublish = true;
                            }
                        }
                    }
                }
            }
            if (needpublish)
            {
                //publish to new sensor ID with new location and timer 
                string publishtopic = "ToSensor/" + newsensorId;
                TimeSeriesObject requestProperties = new TimeSeriesObject();
                requestProperties.SensorId = newsensorId;
                string newTopic = newvendor + "," + newcampus + "," + newbuilding + "," + newroom + "," + newsensorId + "," + newwakeuptimer;
                _logger.LogInformation($"Publish a New SCfg msg with new D3: {newTopic}\n");
                requestProperties.D3 = newTopic;
                requestProperties.Command = "SCfg";
                int timer = Int16.Parse(newwakeuptimer);
                TimeSeriesObjectForJsonSentSCfg JsonMsg = requestProperties.ConvertToTimeSeriesObjectForJsonSentSCfg(timer);
                //send last message to old sensor
                Thread.Sleep(800);
                await PublishMessageAsync(publishtopic, JsonSerializer.Serialize(JsonMsg));
                Thread.Sleep(200);
            }
        }
        public async Task<string> GetAddSensorJson()
        {
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.SensorId != null && o.NickName != "DELETEDFOREVER");
            AddSensorUI sensorUI = new AddSensorUI();
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");

                string addcampus = obj.Campus;
                sensorUI.AddNewCampus(addcampus);
            }

            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");

                string addcampus = obj.Campus;
                string addbuiling = obj.Building;
                sensorUI.CheckAddBuildings(addcampus, addbuiling);
            }

            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");

                string addcampus = obj.Campus;
                string addbuiling = obj.Building;
                string addroom = obj.Room;
                sensorUI.CheckAddRooms(addcampus, addbuiling, addroom);
            }
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                string addcampus = obj.Campus;
                string addbuiling = obj.Building;
                string addroom = obj.Room;
                string id = obj._id.ToString();
                string sensorid = obj.SensorId;
                string vendor = obj.Vendor;
                string wakeuptimer = obj.Note;
                sensorUI.CheckAddSensors(addcampus, addbuiling, addroom, id, sensorid, wakeuptimer, vendor);
            }
            List<AddCampus> list = sensorUI.campusList;
            string res = JsonSerializer.Serialize(list.ToList());
            return res;
        }

        public string GetHolidayList()
        {
            List<string> Date = new List<string>();
            /*    string jsonmsgAPI = "https://data.gov.au/data/api/3/action/datastore_search?resource_id=33673aca-0857-42e5-b8f0-9981b4755686&q=vic";
                string jsonmsg = "";
                try
                {
                    jsonmsg = new WebClient().DownloadString(jsonmsgAPI);
                }
                catch (WebException we)
                { } 
                jsonmsg = "{\"help\": \"https://data.gov.au/data/api/3/action/help_show?name=datastore_search\", \"success\": true, \"result\": {\"include_total\": true, \"resource_id\": \"33673aca-0857-42e5-b8f0-9981b4755686\", \"fields\": [{\"type\": \"int\", \"id\": \"_id\"}, {\"type\": \"text\", \"id\": \"Date\"}, {\"type\": \"text\", \"id\": \"Holiday Name\"}, {\"type\": \"text\", \"id\": \"Information\"}, {\"type\": \"text\", \"id\": \"More Information\"}, {\"type\": \"text\", \"id\": \"Jurisdiction\"}], \"records_format\": \"objects\", \"q\": \"vic\", \"records\": [{\"_id\":88,\"Date\":\"20210308\",\"Holiday Name\":\"Labour Day\",\"Information\":\"Always on a Monday, creating a long weekend. It celebrates the eight-hour working day, a victory for workers in the mid-late 19th century.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":97,\"Date\":\"20211225\",\"Holiday Name\":\"Christmas Day\",\"Information\":\"Christmas Day is an annual holiday which celebrates the birth of Jesus Christ over 2000 years ago.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":99,\"Date\":\"20211227\",\"Holiday Name\":\"Christmas (additional day)\",\"Information\":\"Christmas Day is an annual holiday which celebrates the birth of Jesus Christ over 2000 years ago.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":91,\"Date\":\"20210404\",\"Holiday Name\":\"Easter Sunday\",\"Information\":\"Public Holiday as part of Easter.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":96,\"Date\":\"20211102\",\"Holiday Name\":\"Melbourne Cup\",\"Information\":\"All of Victoria unless alternate local holiday has been arranged by non-metro council.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":92,\"Date\":\"20210405\",\"Holiday Name\":\"Easter Monday\",\"Information\":\"Public Holiday as part of Easter.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":94,\"Date\":\"20210614\",\"Holiday Name\":\"Queen's Birthday\",\"Information\":\"Celebrated on second Monday in June except in Western Australia and Queensland.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":87,\"Date\":\"20210126\",\"Holiday Name\":\"Australia Day\",\"Information\":\"Always celebrated on 26 January\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":100,\"Date\":\"20211228\",\"Holiday Name\":\"Boxing Day (additional day)\",\"Information\":\"Boxing Day occurs the day after Christmas. Sydney-to-Hobart yacht race and Boxing Day Test Match (Cricket) start on this day.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":89,\"Date\":\"20210402\",\"Holiday Name\":\"Good Friday\",\"Information\":\"Easter is celebrated with Good Friday and Easter Monday creating a 4 day long weekend.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":95,\"Date\":\"20210924\",\"Holiday Name\":\"Friday before AFL Grand Final\",\"Information\":\"Friday before the AFL Grand Final\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":93,\"Date\":\"20210425\",\"Holiday Name\":\"Anzac Day\",\"Information\":\"Celebrated on the 25 April each year.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":199,\"Date\":\"20220314\",\"Holiday Name\":\"Labour Day\",\"Information\":\"Always on a Monday, creating a long weekend. It celebrates the eight-hour working day, a victory for workers in the mid-late 19th century.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":207,\"Date\":\"20221225\",\"Holiday Name\":\"Christmas Day\",\"Information\":\"Christmas Day is an annual holiday which celebrates the birth of Jesus Christ over 2000 years ago.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":90,\"Date\":\"20210403\",\"Holiday Name\":\"Saturday before Easter Sunday\",\"Information\":\"Easter Saturday is between Good Friday and Easter Sunday in Australia.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":86,\"Date\":\"20210101\",\"Holiday Name\":\"New Year's Day\",\"Information\":\"New Year's Day is the first day of the calendar year and is celebrated each January 1st\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":98,\"Date\":\"20211226\",\"Holiday Name\":\"Boxing Day\",\"Information\":\"Boxing Day occurs the day after Christmas. Sydney-to-Hobart yacht race and Boxing Day Test Match (Cricket) start on this day.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2021\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":196,\"Date\":\"20220101\",\"Holiday Name\":\"New Year's Day\",\"Information\":\"New Year's Day is the first day of the calendar year and is celebrated each January 1st\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":197,\"Date\":\"20220103\",\"Holiday Name\":\"New Year's Day (additional day)\",\"Information\":\"As 1 January 2022 falls on a Saturday in 2022, the following Monday is observed as an additional public holiday.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":198,\"Date\":\"20220126\",\"Holiday Name\":\"Australia Day\",\"Information\":\"Always celebrated on 26 January\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":200,\"Date\":\"20220415\",\"Holiday Name\":\"Good Friday\",\"Information\":\"Easter is celebrated with Good Friday and Easter Monday creating a 4 day long weekend.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":201,\"Date\":\"20220416\",\"Holiday Name\":\"Saturday before Easter Sunday\",\"Information\":\"Easter Saturday is between Good Friday and Easter Sunday in Australia.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":202,\"Date\":\"20220417\",\"Holiday Name\":\"Easter Sunday\",\"Information\":\"Public Holiday as part of Easter.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":203,\"Date\":\"20220418\",\"Holiday Name\":\"Easter Monday\",\"Information\":\"Public Holiday as part of Easter.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":204,\"Date\":\"20220425\",\"Holiday Name\":\"Anzac Day\",\"Information\":\"Celebrated on the 25 April each year.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":205,\"Date\":\"20220613\",\"Holiday Name\":\"Queen's Birthday\",\"Information\":\"Celebrated on second Monday in June except in Western Australia and Queensland.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":206,\"Date\":\"20221101\",\"Holiday Name\":\"Melbourne Cup\",\"Information\":\"All of Victoria unless alternate local holiday has been arranged by non-metro council.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":208,\"Date\":\"20221226\",\"Holiday Name\":\"Boxing Day\",\"Information\":\"Boxing Day occurs the day after Christmas. Sydney-to-Hobart yacht race and Boxing Day Test Match (Cricket) start on this day.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":209,\"Date\":\"20221227\",\"Holiday Name\":\"Christmas (additional day)\",\"Information\":\"As 25 December (Christmas Day) falls on a Sunday in 2022, there is an additional public holiday on the Tuesday.\",\"More Information\":\"https://www.business.vic.gov.au/victorian-public-holidays-and-daylight-saving/victorian-public-holidays-2022\",\"Jurisdiction\":\"vic\",\"rank\":0.0573088}], \"_links\": {\"start\": \"/api/3/action/datastore_search?q=vic&resource_id=33673aca-0857-42e5-b8f0-9981b4755686\", \"next\": \"/api/3/action/datastore_search?q=vic&offset=100&resource_id=33673aca-0857-42e5-b8f0-9981b4755686\"}, \"total\": 29}}"; 
                   _logger.LogInformation("Load Holiday Json");*/
             

            StreamReader r = new StreamReader("DBCreate/holiday.json");
            string jsonmsg = r.ReadToEnd();

            /* string[] subs = jsonmsg.Split("\"q\": \"vic\", \"records\":"); 
             string record = "" + subs[1];
             string[] subs2 = record.Split(", \"_links\":");
             string record1 = subs2[0]; */

            string recordfinal = jsonmsg.Replace("More Information", "MoreInformation").Replace("Holiday Name", "HolidayName");
            /*string test = "[{\"_id\":88,\"Date\":\"20210308\",\"HolidayName\":\"Labour Day\",\"Information\":\"Always on a Monday, creating a long weekend. It celebrates the eight-hour working day, a victory for workers in the mid-late 19th century.\",\"MoreInformation\":\"f21 \",\"Jurisdiction\":\"vic\",\"rank\":0.0573088},{\"_id\":89,\"Date\":\"20210308\",\"HolidayName\":\"Labour Day\",\"Information\":\"Always on a Monday, creating a long weekend. It celebrates the eight-hour working day, a victory for workers in the mid-late 19th century.\",\"MoreInformation\":\"f21 \",\"Jurisdiction\":\"vic\",\"rank\":0.0573088}]";*/
            List<PublicHoliday> res = JsonSerializer.Deserialize<List<PublicHoliday>>(recordfinal);
            List<PublicHolidayRecord> HolidaylistRecord = new List<PublicHolidayRecord>();
            List<PublicHolidayToUI> HolidaylistToUI = new List<PublicHolidayToUI>();
            foreach (PublicHoliday ele in res)
            {
                PublicHolidayRecord newrecord = ele.GetShortRecord();
                HolidaylistRecord.Add(newrecord);
            }
            var result = HolidaylistRecord.OrderBy(o => o.Date);
            foreach (PublicHolidayRecord newrecord in result)
            {
                if (!newrecord.Expired)
                {
                    PublicHolidayToUI uiele = newrecord.GetStringToUI();
                    HolidaylistToUI.Add(uiele);
                }
            }
            foreach (PublicHolidayToUI ele in HolidaylistToUI)
            {
                Console.WriteLine(ele.Date);
            }
            string jsonstring = JsonSerializer.Serialize(HolidaylistToUI.ToList());
            return jsonstring;
        }
        public async Task PublishSimpleMessage(string temperature, string fanspeed, string poweronoff, string mode, string cmd, string sensorid, string schedule, string resettimer)
        {
            int temp = 22, fan_speed = 1, reset_timer = 3;
            string mode_default = "AUTO";
            bool power_onoff = true;
            if (!string.IsNullOrEmpty(temperature))
                temp = Int16.Parse(temperature);
            if (!string.IsNullOrEmpty(fanspeed))
                fan_speed = Int16.Parse(fanspeed);
            if (!string.IsNullOrEmpty(mode))
                mode_default = mode;
            if (!string.IsNullOrEmpty(resettimer))
                reset_timer = Int16.Parse(resettimer);
            if (!string.IsNullOrEmpty(poweronoff))
                power_onoff = Convert.ToBoolean(poweronoff);

            // 18/09/2021,15/10/2021,1:00 AM,1:00 PM,123##26/12/2021,03/01/2022,25/12/2022,University Holiday: 25/09/2021 - 19/10/2021
            // 18 / 09 / 2021,15 / 10 / 2021,1:00 AM,1:00 PM,123##26/12/2021,03/01/2022,25/12/2022,University Holiday: 25/09/2021 - 19/10/2021,University Holiday:      21/09/2021 - 19/11/2021
            string trimed = schedule.Replace(" ", "");
            ScheduleManager ScheduleBlob = new ScheduleManager();
            string[] twostring = trimed.Split("##");
            string dailystring = twostring[0];
            string holidaystring = twostring[1];
            BitArray weekdayarray = new BitArray(8);
            int finalweekdayint = 0;
            if (!string.IsNullOrEmpty(dailystring))
            {
                string[] scheduleSplit = dailystring.Split(',');
                string startdate = scheduleSplit[0];
                string enddate = scheduleSplit[1];
                string starttime = scheduleSplit[2];
                string endtime = scheduleSplit[3];
                string weekday = scheduleSplit[4];
                int weekdaylength = weekday.Length;
                for (int j = 0; j < weekdaylength; j++)
                {
                    string oneweekday = weekday.Substring(j, 1);
                    int weekdayofthis = Int32.Parse(oneweekday);
                    //0 is sunday, 1 is monday, 2 is tuesday 3 is wednesday, 4 is thur 5 is friday 6 is saturday
                    if (weekdayofthis >= 0 && weekdayofthis <= 6)
                        weekdayarray.Set(weekdayofthis, true);

                    // if (weekdayofthis == 0)
                    //    weekdayarray.Set(6, true);
                }
                int[] array = new int[1];
                weekdayarray.CopyTo(array, 0);
                finalweekdayint = array[0];
                ScheduleBlob.AddSchedule(1, startdate, enddate, starttime, endtime, finalweekdayint, temp, mode_default, fan_speed, reset_timer, power_onoff);
            }
            if (!string.IsNullOrEmpty(holidaystring))
            {
                string trimedstring = holidaystring.Replace(" ", "");
                string[] scheduleSplit = trimedstring.Split(',');
                for (int j = 0; j < scheduleSplit.Length; j++)
                {
                    string oneholidaydate = scheduleSplit[j];
                    if (!oneholidaydate.Contains("UniversityHoliday"))
                    {  //only one date in the list                         
                        ScheduleBlob.AddSchedule(2, oneholidaydate, oneholidaydate, "now", "now", 127, 25, "A", 5, 2, false);
                    }
                    else
                    {
                        string[] startenddate = oneholidaydate.Split("UniversityHoliday:");
                        string datarange = startenddate[1];
                        string[] startandendarray = datarange.Split("-");
                        string startdatestring = startandendarray[0];
                        string enddatestring = startandendarray[1];
                        ScheduleBlob.AddSchedule(2, startdatestring, enddatestring, "now", "now", 127, 25, "A", 5, 2, false);
                    }
                }
            }

            TimeSeriesObject TimeSeriesObjectToDB = new TimeSeriesObject();
            /* if (!string.IsNullOrEmpty(sensorid))
             {
                 //yes! user input a sensor id which is valid, so we need directly send messages to it, we must provide sensor ID and topic name to send, no topic name no send 
                 if (!string.IsNullOrEmpty(topic))
                     await PublishMessageAsync(payload.TopicName, JsonSerializer.Serialize(payload));
             }*/
            // List<Task> tasklist =new List<Task>();
            // tasklist.Add(_driverTimeSeries.InsertObjectAsync(TimeSeriesObjecttest));
            // await Task.WhenAll(tasklist);
            // Thread.Sleep(10000);     
            // new code
            List<string> MySelectedTopicList = MySelecter.TopicList;
            string[] topiclist = MySelectedTopicList.ToArray();
            var configList = _driverTimeSeries.Collection.AsQueryable().Where(c => topiclist.Any(x => c.Selecter.TopicList.Contains(x)) && c.Command.Contains("UCfgSingleTopic")).Select(c => new ScheduleResult
            {
                Timestamp = c.Timestamp,
                ScheduleBlob = c.ScheduleBlob,
                Selecter = c.Selecter
            }).OrderByDescending(c => c.Timestamp).ToList();
            List<ScheduleResult> listofResult = configList.ToList();
            List<Task> tasklist = new List<Task>();
            if (MySelectedTopicList.Count > 0)
            {
                foreach (string selectedtopic in MySelectedTopicList)
                {
                    if (!string.IsNullOrEmpty(selectedtopic))
                    {
                        List<Schedule> scheduleforthistopic = new List<Schedule>();
                        scheduleforthistopic.Clear();
                        foreach (ScheduleResult scheduleresultele in listofResult)
                        {
                            List<string> topicstringlist = scheduleresultele.Selecter.TopicList;
                            foreach (string topic in topicstringlist)
                            {
                                if (topic == selectedtopic)
                                {
                                    foreach (Schedule scheduleele in scheduleresultele.ScheduleBlob.ScheduleList)
                                    {
                                        Schedule elecopy = scheduleele.GetCopy();
                                        elecopy.StartDate = elecopy.StartDate.ToLocalTime();
                                        elecopy.EndDate = elecopy.EndDate.ToLocalTime();
                                        elecopy.StartTime = elecopy.StartTime.ToLocalTime();
                                        elecopy.EndTime = elecopy.EndTime.ToLocalTime();
                                        bool foundsame = false;
                                        foreach (Schedule existing in scheduleforthistopic)
                                        {
                                            if (elecopy.isSame(existing))
                                                foundsame = true;
                                        }
                                        if (!foundsame)
                                        {
                                            scheduleforthistopic.Add(elecopy);
                                        }
                                    }
                                }
                            }
                        }
                        foreach (Schedule ele in ScheduleBlob.ScheduleList)
                        {
                            Schedule elecopy = ele.GetCopy();
                            scheduleforthistopic.Add(elecopy);
                        }
                        TopicSchedule OneTopicSchedule = new TopicSchedule(selectedtopic, scheduleforthistopic);
                        ScheduleJSONMsg payload = OneTopicSchedule.GenerateFirmwareMsg(1, 16, 28, 39);
                        string payloadJSON = JsonSerializer.Serialize(payload);
                        tasklist.Add(PublishMessageAsync(selectedtopic, payloadJSON));
                        ScheduleBlob.ListOfTopicAllSchedule.Add(OneTopicSchedule);
                    }
                    Thread.Sleep(50);
                }
            }

            /* if (MySelecter.TopicList.Count > 0)
             {
                 for (int i = 0; i < MySelecter.TopicList.Count; i++)
                 {
                     string topicString = MySelecter.TopicList[i];
                     if (!string.IsNullOrEmpty(topicString))
                     {
                         tasklist.Add(PublishMessageAsync(topicString, payloadJSON));
                     }
                     Thread.Sleep(50);
                 }
             }*/

            await Task.WhenAll(tasklist);
            Thread.Sleep(500);
            ScheduleBlob.GenerateScheduleUIMsg();
            List<TimeSeriesObject> timeserieslist = new List<TimeSeriesObject>();

            foreach (string topicstring in MySelecter.TopicList)
            {
                SelectionModel MySelecterforEachTopic = new SelectionModel();
                TimeSeriesObject newtimeseries = new TimeSeriesObject();
                MySelecterforEachTopic.TopicList.Add(topicstring);
                newtimeseries.SetValue(temp, fan_speed, mode_default, power_onoff, reset_timer, ScheduleBlob, MySelecterforEachTopic, "UCfgSingleTopic");
                timeserieslist.Add(newtimeseries);
            }
            TimeSeriesObjectToDB.SetValue(temp, fan_speed, mode_default, power_onoff, reset_timer, ScheduleBlob, MySelecter, "UCfg");
            timeserieslist.Add(TimeSeriesObjectToDB);
            await _driverTimeSeries.InsertObjectManyAsync(timeserieslist);

            /* TimeSeriesObjectToDB.SetValue(temp, fan_speed, mode_default, power_onoff, reset_timer, ScheduleBlob, MySelecter, "UCfg");
             await _driverTimeSeries.InsertObjectAsync(TimeSeriesObjectToDB);
             Thread.Sleep(100);*/

            //25 Aug 2021 change remove the rest 
            /*insert each topic to DeviceObject collection for each sensor 
            SELECT Bundoora, PY, Melbourne, Room1
            so only room1 in PY, bundoora use this
            Melbourne whole use this
            search table, melbourne and pos = 1, only return latest timestamp, one entry
            read request msg
            if temp and schedule are different, then show this schedule , and show in red, means different 
            if temp and schedule are same, then show it but not red
            search room1 in PY and bundoora, if pos is 3, only return latest timestamp
            if has and dif, show in red, otherwise, not red
            if click change button
            insert whole in timeseries for reference
            return all melbourne sensors, set pos 1, replace the request meg, topic only mel top
            return all ROOM1, set pos 3, replace the request meg- topic only room3
            or return all sensors in PY, set pos 2, replace request meg, topic only PY
            show sensor set temp - show request meg set temp, and schedule, and show this is set by or campus
            show */
            List<Campus> CampusList = MySelecter.NetZeroNetwork.Campuses;
            if (CampusList.Count > 0)
            {
                foreach (Campus ele in CampusList)
                {
                    if (ele.Buildings.Count == 0)
                    {
                        //no buildings, only this campus globally 
                        //we need to search all sensors in this campus and change the request reference - hierarchy = 1
                        List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.Campus == ele.Name);
                        foreach (DeviceObject obj in obj_list)
                        {
                            if (obj is null)
                                throw new Exception("Null device object found in MongoDB");
                            List<Schedule> newlist = new List<Schedule>();
                            newlist = obj.RequestProperties.AppendNewScheduleList(TimeSeriesObjectToDB);
                            obj.RequestProperties = TimeSeriesObjectToDB;
                            obj.RequestProperties.ScheduleBlob.ScheduleList = newlist;
                            obj.RequestProperties.SetHierarchy(ele.Name, "ALL", "ALL", 1);
                            obj.SetConfigHierarchy("Campus");
                            await _driverDeviceObject.UpdateObjectAsync(obj, obj._id); //update send and received    
                        }
                    }
                    else
                    {
                        //has building selected
                        foreach (Buildings building in ele.Buildings)
                        {
                            if (building.Rooms.Count == 0)
                            {
                                //no rooms in this building, so all buildings, create for all buildings                         
                                //Read all sensors in this building
                                List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.Campus == ele.Name && o.Building == building.Name);
                                foreach (DeviceObject obj in obj_list)
                                {
                                    if (obj is null)
                                        throw new Exception("Null device object found in MongoDB");
                                    List<Schedule> newlist = new List<Schedule>();
                                    newlist = obj.RequestProperties.AppendNewScheduleList(TimeSeriesObjectToDB);
                                    obj.RequestProperties = TimeSeriesObjectToDB;
                                    obj.RequestProperties.ScheduleBlob.ScheduleList = newlist;
                                    obj.RequestProperties.SetHierarchy(ele.Name, building.Name, "ALL", 2);
                                    obj.SetConfigHierarchy("Building");
                                    await _driverDeviceObject.UpdateObjectAsync(obj, obj._id); //update send and received                            
                                }
                            }
                            else
                            {
                                //this building has room selected
                                foreach (string rooms in building.Rooms)
                                {
                                    TopicStringArray newTopic = new TopicStringArray(ele.Name, building.Name, rooms, "ALL", 3);
                                    string roomTopic = newTopic.GetTopicString();
                                    //Read all sensors in this room
                                    List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.Campus == ele.Name && o.Building == building.Name && o.Room == rooms);
                                    foreach (DeviceObject obj in obj_list)
                                    {
                                        if (obj is null)
                                            throw new Exception("Null device object found in MongoDB");
                                        List<Schedule> newlist = new List<Schedule>();
                                        newlist = obj.RequestProperties.AppendNewScheduleList(TimeSeriesObjectToDB);
                                        obj.RequestProperties = TimeSeriesObjectToDB;
                                        obj.RequestProperties.ScheduleBlob.ScheduleList = newlist;
                                        obj.RequestProperties.SetHierarchy(ele.Name, building.Name, rooms, 3);
                                        obj.SetConfigHierarchy("Room");
                                        await _driverDeviceObject.UpdateObjectAsync(obj, obj._id); //update send and received       
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Thread.Sleep(100);
        }
        public async Task ProcessSensorPayload(TimeSeriesObject timeSeriesObj)
        {
            string sensorId = timeSeriesObj.SensorId;
            DeviceObject obj = await _driverDeviceObject.ReadSingleObjectAsync(o => o.SensorId == sensorId && o.SensorId != "UNASSIGNED");
            // DeviceObject obj2 = await _driverDeviceObject.ReadSingleObjectAsync(o => o.Building == "" && o.Campus=="");
            if (obj is null)
                throw new Exception("The device does not exist or subscribed to the broker.");
            obj.CurrentProperties = timeSeriesObj; // timeSeriesObj.GetDbProperties();  ready for update received msg 
            if (timeSeriesObj.Command.Contains("RCfg"))
            {
                //the new topic still not available 
                _logger.LogInformation("### RECEIVED Sensor topic RCfg MESSAGE - Init Sensor ###");
                TimeSeriesObject requestProperties = new TimeSeriesObject();
                requestProperties.SensorId = obj.SensorId;
                string newTopic = obj.Vendor.ToUpper() + "," + obj.Campus.ToUpper() + "," + obj.Building.ToUpper() + "," + obj.Room.ToUpper() + "," + obj.SensorId.ToUpper() + "," + obj.Note.ToUpper();
                requestProperties.D3 = newTopic;
                requestProperties.Command = "SCfg";
                int wakeuptimer = 60;

                if (obj.Note.Length > 0 && obj.Note != "UNASSIGNED")
                    wakeuptimer = Int16.Parse(obj.Note);
                TimeSeriesObjectForJsonSentSCfg JsonMsg = requestProperties.ConvertToTimeSeriesObjectForJsonSentSCfg(wakeuptimer);
                obj.RequestProperties = requestProperties; // requestProperties.GetDbProperties(); ready for update request 
                await PublishMessageAsync($"ToSensor/{obj.SensorId}", JsonSerializer.Serialize(JsonMsg));
                await _driverTimeSeries.InsertObjectAsync(requestProperties); //insert this reply message to times series collection                
            }
            Console.WriteLine("### Update Data in DeviceObject Collection and insert into timeSeries Collection###");
            await _driverDeviceObject.UpdateObjectAsync(obj, obj._id); //update send and received 
            await _driverTimeSeries.InsertObjectAsync(timeSeriesObj); //insert received msg in time series collection 

            //temporarily not used 
            /*  string signalRmsg = "Temperature " + timeSeriesObj.RoomTemperature.ToString() + "FanSpeed " + timeSeriesObj.FanSpeed.ToString() + "Time " + timeSeriesObj.Timestamp.ToString();
              await _notificationService.SendMessage(sensorId, signalRmsg);
              var chatHub = (IHubContext<RealtimeSignalHub>)_serviceProvider.GetService(typeof(IHubContext<RealtimeSignalHub>));
                //Send message to all users in SignalR
              await chatHub.Clients.All.SendAsync("Receive Message", "est", "You have received a message");
              var chatHub = (IHubContext<RealtimeSignalHub>)_serviceProvider.GetService(typeof(IHubContext<RealtimeSignalHub>));
              // Send message to all users in SignalR  - not used for now 
              await chatHub.Clients.All.SendAsync("ReceiveMessage", sensorId, signalRmsg);*/
        }
        public async Task<string> GetWarningList()
        {
            List<WarningMessage> warningList = new List<WarningMessage>();
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.SensorId != null & o.SensorId != "UNASSIGNED");
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                TimeSeriesObject currentStatus = obj.CurrentProperties;
                string reason = "";
                if (currentStatus.RoomTemperature > 28)
                    reason += " OH ";
                else if (currentStatus.RoomTemperature <= 14)
                    reason += " OC ";

                if (currentStatus.BatteryPercentage <= 4900 && currentStatus.BatteryPercentage > 4500)
                {
                    reason += " BL ";
                }
                
                if (currentStatus.BatteryPercentage <= 4500)
                {
                    reason += " BD ";
                }
                int sec = (int)(DateTime.Now - currentStatus.Timestamp).TotalSeconds;
                bool isActive = true;

                if (sec <= 3600 * 3)
                    isActive = true;
                else
                    isActive = false; 

                if (!isActive) //second, 3 hour 
                {
                    reason += " LC ";
                }

                if (isActive && currentStatus.PowerOnOff && DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 7)
                {
                    reason += " AO ";
                }

                if (isActive && Math.Abs(currentStatus.RoomTemperature - currentStatus.SetTemperature) > 5)
                {
                    reason += " TE ";
                } 

                if (reason != "")
                {
                    WarningMessage detectedWarningMessage = obj.GeWarningMessages();
                    detectedWarningMessage.Reason = reason;
                    warningList.Add(detectedWarningMessage);
                }
            }
            return JsonSerializer.Serialize(warningList);
        }
        public async Task<string> GetChartsList()
        {
            List<WarningMessage> warningList = new List<WarningMessage>();
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => !o.SensorId.Contains("UNASSIGNED"));
            int countall = 0;
            int countfaulty = 0;
            int countinactive = 0;
            int countactive = 0;
            countall = obj_list.Count;
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                TimeSeriesObject currentStatus = obj.CurrentProperties;
                string reason = "";
                if (currentStatus.RoomTemperature > 28)
                    reason += " OH ";
                else if (currentStatus.RoomTemperature <= 14)
                    reason += " OC ";
                if (currentStatus.BatteryPercentage <= 4800 && currentStatus.BatteryPercentage > 4000)
                {
                    reason += " BL ";
                }
                if (currentStatus.BatteryPercentage <= 4000)
                {
                    reason += " BD ";
                }
                int sec = (int)(DateTime.Now - currentStatus.Timestamp).TotalSeconds;
                bool isActive = true;

                if (sec <= 3600 * 3)
                    isActive = true;
                else
                    isActive = false;

                if (!isActive) //second, 3 hour 
                {
                    reason += " LC ";
                }

                if (isActive && currentStatus.PowerOnOff && DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 7)
                {
                    reason += " AO ";
                }

                if (isActive && Math.Abs(currentStatus.RoomTemperature - currentStatus.SetTemperature) > 5)
                {
                    reason += " TE ";
                }

                if (reason != "")
                {
                    countfaulty++;
                    reason = "Alert - "+ reason;
                }
                else
                {
                    if (currentStatus.PowerOnOff)
                    {
                        reason = "Active";
                        countactive++;
                    }
                    else
                    {
                        reason = "Inactive";
                        countinactive++;
                    }
                }
                WarningMessage detectedWarningMessage = obj.GeWarningMessages();
                detectedWarningMessage.Reason = reason;
                warningList.Add(detectedWarningMessage);
            }
            foreach (WarningMessage ele in warningList)
            {
                ele.Note = countall.ToString() + "KING" + countactive.ToString() + "KING" + countinactive.ToString() + "KING" + countfaulty.ToString();
            }
            string warn = JsonSerializer.Serialize(warningList);
            return warn;
        }

        public async Task<string> GetSensorStatus()
        {
            List<WarningMessage> warningList = new List<WarningMessage>();
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => !o.SensorId.Contains("UNASSIGNED"));
            int countall = 0;
            int countfaulty = 0;
            int countinactive = 0;
            int countactive = 0;
            countall = obj_list.Count;
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                TimeSeriesObject currentStatus = obj.CurrentProperties;
                string reason = "";
                if (currentStatus.RoomTemperature > 28)
                    reason += " OH ";
                else if (currentStatus.RoomTemperature <= 14)
                    reason += " OC ";

                if (currentStatus.BatteryPercentage <= 4900 && currentStatus.BatteryPercentage > 4500)
                {
                    reason += " BL ";
                }

                if (currentStatus.BatteryPercentage <= 4500)
                {
                    reason += " BD ";
                }
                int sec = (int)(DateTime.Now - currentStatus.Timestamp).TotalSeconds;
                bool isActive = true;

                if (sec <= 3600 * 3)
                    isActive = true;
                else
                    isActive = false;

                if (!isActive) //second, 3 hour 
                {
                    reason += " LC ";
                }

                if (isActive && currentStatus.PowerOnOff && DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 7)
                {
                    reason += " AO ";
                }

                if (isActive && Math.Abs(currentStatus.RoomTemperature - currentStatus.SetTemperature) > 5)
                {
                    reason += " TE ";
                }  
 
                if (reason != "")
                {
                    countfaulty++;
                    reason = "Alert - " + reason;
                }
                else
                {
                    if (currentStatus.PowerOnOff)
                    {
                        reason = "Active";
                        countactive++;
                    }
                    else
                    {
                        reason = "Inactive";
                        countinactive++;
                    }
                }
                WarningMessage detectedWarningMessage = obj.GeWarningMessages_API();
                detectedWarningMessage.Reason = reason;
                warningList.Add(detectedWarningMessage);
            }
            foreach (WarningMessage ele in warningList)
            {
                ele.Note = countall.ToString() + "ACTIVE" + countactive.ToString() + "INACTIVE" + countinactive.ToString() + "FAULTY" + countfaulty.ToString();
            }
            string warn = JsonSerializer.Serialize(warningList);
            return warn;
        }


        public async Task<string> GetSensorData()
        {
            List<TimeSeriesObject> obj_list = await _driverTimeSeries.ReadObjectsAsync(o => o.SensorId != null);
            string warn = JsonSerializer.Serialize(obj_list);
            return warn;
        }

        public async Task<string> GetCountCards()
        {
            AirConCount results = new AirConCount();
            List<DeviceObject> obj_list = await _driverDeviceObject.ReadObjectsAsync(o => o.SensorId != null);
            results.Timestamp = DateTime.Now.ToString();
            results.TotalCount = obj_list.Count().ToString();
            int runningCount = 0;
            int stoppingCount = 0;
            int offlineCount = 0;
            foreach (DeviceObject obj in obj_list)
            {
                if (obj is null)
                    throw new Exception("Null device object found in MongoDB");
                TimeSeriesObject currentStatus = obj.CurrentProperties;
                int sec = (int)(DateTime.UtcNow - currentStatus.Timestamp).TotalSeconds;
                if (sec < 1600) //30 mins fresh active one 
                {
                    if (currentStatus.PowerOnOff)
                    {
                        runningCount++;
                    }
                    else
                    {
                        stoppingCount++;
                    }
                }
                else
                {
                    //expired, something wrong, no active, offline 
                    offlineCount++;
                }
            }
            results.RunningCount = runningCount.ToString();
            results.StoppingCount = stoppingCount.ToString();
            results.OfflineCount = offlineCount.ToString();
            return JsonSerializer.Serialize(results);
        }
        public async Task<string> GetPowerStatistics()
        {
            DateTime dt1 = new DateTime();
            dt1 = DateTime.Now;
            DateTime dt2 = dt1.AddSeconds(-1600);
            PowerStatictics results = new PowerStatictics();
            /* var projection = new BsonDocument("_id", "$SomeIdField").
             Add("Result", new BsonDocument("$max", "$someNumberField"));*/
            var obj_list = await _driverTimeSeries.Collection.Aggregate().Match(o => o.PowerOnOff && (o.Timestamp < dt2))
                 .Group(x => new { x.SensorId, x.RoomTemperature },
                            g => new
                            {
                                Result = g.Select(
                                           x => x.RoomTemperature
                                           ).Sum()
                            }
                        )
                .ToListAsync();
            // var obj_list =   _driverTimeSeries.Collection.Aggregate ().Match(o=> (DateTime.Now - o.Timestamp).TotalSeconds > 1600).ToList();
            //   .Group(x => x.SensorId, value => new { MyId = value.Key, MyCount = value.Count(x=>x.SensorId !="0") }).ToList();     
            return JsonSerializer.Serialize(obj_list);
        }
        public async Task ConnectMqttClientAsync()
        {
            _mqttClient.UseConnectedHandler(async e =>
            {
                _logger.LogInformation("## Connected to MQTT broker");
                // Subscribe to a topic
                await SubscribeAsync();
                _logger.LogInformation("Subscribe Succeed");
            });
            _mqttClient.UseDisconnectedHandler(async e =>
            {
                _logger.LogInformation("Disconnected with Broker");
                try
                {
                    Thread.Sleep(500);
                    _logger.LogInformation("Try to reconnect Broker");
                    await Task.Run(async () => { await ConnectMqttClientAsync(); });
                }
                catch { _logger.LogInformation("Reconnect to Broker Failed"); }
            });
            _mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                try
                {
                    Thread.Sleep(300);
                    MqttApplicationMessage ApplicationMessage = e.ApplicationMessage;
                    string Topic = ApplicationMessage.Topic;
                    string Payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    MqttQualityOfServiceLevel QoS = ApplicationMessage.QualityOfServiceLevel;
                    bool Retain = ApplicationMessage.Retain;
                    _logger.LogInformation($"Recv Msg: Topic {Topic} QoS {QoS} Retain {Retain} Payload {Payload} \n");
                    System.Diagnostics.Trace.WriteLine("Handle Recv Message");
                    TimeSeriesObject timeSeriesObj = JsonSerializer.Deserialize<TimeSeriesObjectForJsonRecv>(Payload).ConvertToTimeSeriesObj();
                    await Task.Run(() => ProcessSensorPayload(timeSeriesObj));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Recv Msg {0} Exception caught.", ex);
                }

                // await _mqttClient.ReconnectAsync();
                // Task.Run(() => _mqttClient.PublishAsync(_subscriptionTopicName));             
            });
            /*  _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(async e =>
              {
                  try
                  {
                      MqttApplicationMessage ApplicationMessage = e.ApplicationMessage;
                      string Topic = ApplicationMessage.Topic;
                      string Payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                      MqttQualityOfServiceLevel QoS = ApplicationMessage.QualityOfServiceLevel;
                      bool Retain = ApplicationMessage.Retain;
                      _logger.LogInformation($"New Handler: Recv Msg: Topic {Topic} QoS {QoS} Retain {Retain} Payload {Payload} \n");
                      System.Diagnostics.Trace.WriteLine("Handle Recv Message");
                      TimeSeriesObject timeSeriesObj = JsonSerializer.Deserialize<TimeSeriesObjectForJsonRecv>(Payload).ConvertToTimeSeriesObj();
                      await Task.Run(() => ProcessSensorPayload(timeSeriesObj));
                  }
                  catch
                  {
                      _logger.LogInformation("New Handler: Recv Msg Exception!");
                  }

              });*/
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(async e =>
            {
                try
                {
                    _logger.LogInformation("Connection Failed ");
                    Thread.Sleep(500);
                    _logger.LogInformation("Connection Failed - Try to reconnect broker");
                    await Task.Run(async () => { await ConnectMqttClientAsync(); });
                }
                catch { _logger.LogInformation("Connection Failed - Reconnect Failed Too"); }
            });
            await _mqttClient.StartAsync(_mqttClientOptions);

            _logger.LogInformation("StartAsync Succeed");
        }
        public async Task InitializeSensor(string sensorId)
        {
            _logger.LogInformation("Initialize Dummy Data Sensors");
            StreamReader r = new StreamReader("DBCreate/airconlist.json");
            string jsonString = r.ReadToEnd();

            /* string[] campus = { "Bundoora", "City", "Bendigo", "Mildura" };
             string[] aircon = { "Panasonic", "Daikin", "Mitsubishi", "Hitachi" };
             string[] init = { "BU", "CI", "BE", "MI" };
             string[] mode = { "A", "F", "D", "C", "H" };
             string[] building = { "PE", "DWA", "UNION", "LIB" };*/

            List<AirConInfo> airconlist = JsonSerializer.Deserialize<List<AirConInfo>>(jsonString);
            List<AirConInfo> airconlis1 = airconlist;

            foreach (AirConInfo ele in airconlist)
            {

                Random rnd = new Random();
                /*  int airconID = rnd.Next(4);
                int campusID = rnd.Next(4);
                int modeID = rnd.Next(5);
                int buildingID = rnd.Next(4);
                int room = rnd.Next(100, 300);*/
                string newID = "UNASSIGNED";
                string campusname = ele.Campus.ToUpper();
                string buildingname = ele.Building.ToUpper();
                string airname = "UNASSIGNED";
                if (!String.IsNullOrEmpty(ele.Manufacturer))
                    airname = ele.Manufacturer.ToUpper();
                string roomname = "UNASSIGNED";
                string roomnospace = ele.Room.Replace(".", "").Replace(",", "").Replace(" ", "").ToUpper();
                string levelnospace = ele.Level.Replace(" ", "").Replace(".", "").Replace(",", "").ToUpper();
                if (!String.IsNullOrEmpty(levelnospace) && !String.IsNullOrEmpty(roomnospace))
                    roomname = levelnospace + "-" + roomnospace;
                else
                {
                    if (String.IsNullOrEmpty(levelnospace) && String.IsNullOrEmpty(roomnospace))
                        roomname = "UNASSIGNED";
                    else
                    {
                        if (String.IsNullOrEmpty(levelnospace))
                            roomname = roomnospace;
                        if (String.IsNullOrEmpty(roomnospace))
                            roomname = levelnospace;
                    }
                }
                //  string modename = mode[modeID];
                //  int poweronpercent = rnd.Next(100);

                //  if (!String.IsNullOrEmpty(ele.SerialNo))
                //     airconno = ele.SerialNo.ToUpper();
                string IRcode = "UNASSIGNED";
                string airconno = "UNASSIGNED";
                bool power = true;
                //string nickname = ele.ID.ToString();
                string nickname = campusname + "#" + buildingname + "#" + roomname + "#" + airname;
                if (!String.IsNullOrEmpty(ele.ModelNo))
                    IRcode = ele.ModelNo.ToUpper();
                string com = "SVal";
                string d3 = "SVal";
                string note = "60";
                //  if (!String.IsNullOrEmpty(ele.AssetName))
                //      note = ele.AssetName.ToUpper();

                int roomtemp = rnd.Next(10, 30);
                int settemp = rnd.Next(18, 25);
                int fanspeed = rnd.Next(0, 6);
                await InsertDeviceObject(newID, nickname, airname, campusname, buildingname, roomname, airconno, IRcode, note, settemp, roomtemp, fanspeed, power, com, "A", d3);
            };
            /* for (int i = 0; i < 50; i++)
             {
                 string newID = id + i.ToString();
                 Random rnd = new Random();
                 int airconID = rnd.Next(4);
                 int campusID = rnd.Next(4);
                 int modeID = rnd.Next(5);
                 int buildingID = rnd.Next(4);
                 int room = rnd.Next(100, 300);
                 string campusname = campus[campusID];
                 string buildingname = init[campusID] + building[buildingID];
                 string roomname = buildingname + room.ToString();
                 string airname = aircon[airconID];
                 string modename = mode[modeID];
                 int poweronpercent = rnd.Next(100);
                 int airconno = 1;
                 bool power = true;
                 if (poweronpercent < 30)
                     power = false;
                 if (poweronpercent < 10)
                     airconno = 2;
                 if (poweronpercent < 5)
                     airconno = 3;
                 string nickname = roomname + airconno.ToString();
                 string IRcode = nickname + modename + airname;
                 string com = "SVal";
                 string d3 = "SVal";
                 string note = "Fake";
                 int roomtemp = rnd.Next(10, 30);
                 int settemp = rnd.Next(18, 25);
                 int fanspeed = rnd.Next(0, 6);
                 await InsertDeviceObject(newID, nickname, airname, campusname, buildingname, roomname, airconno, IRcode, note, settemp, roomtemp, fanspeed, power, com, modename, d3);
             }*/
            /*
            DeviceObject obj = await _driverDeviceObject.ReadSingleObjectAsync(o => o.SensorId == sensorId);
            if (obj is null)
                throw new Exception("The device does not exist or subscribed to the broker.");
            string newTopicName = $"{obj.Campus}/{obj.Vendor}/{obj.Building}/{obj.Floor}/{obj.Room}/{obj.AirConNo}/{obj.SensorId}";
            TimeSeriesObject properties = new TimeSeriesObject { SensorId = sensorId, TopicName = newTopicName, Timestamp = DateTime.Now, PowerOnOff = true, FanSpeed = 2, Temperature = 24, Note = "Request" };
            TimeSeriesObjectForJsonMV JsonMsg = properties.ConvertToTimeSeriesObjectForJsonMV();
            await PublishMessageAsync($"Sensor/{obj.SensorId}", JsonSerializer.Serialize(JsonMsg));
            obj.RequestProperties = properties; // properties.GetDbProperties();
            await _driverDeviceObject.UpdateObjectAsync(obj, obj.Id);
            await _driverTimeSeries.InsertObjectAsync(properties);
            Console.WriteLine("### Insert one data into TimeSeries Collection and Update DeviceObject Collection ###");*/
        }
        public async Task PublishMessageAsync(string topic, string payload)
        {
            if (!ValidateTopic(topic))
            {
                _logger.LogInformation("Invalid Topic to publish");
                throw new Exception("The topic is not valid...");
            }
            if (!ValidatePayload(payload))
            {
                _logger.LogInformation("Invalid payload to publish");
                throw new Exception("The payload structure is not valid...");
            }
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithAtLeastOnceQoS()
                .WithRetainFlag()
                .Build();
            MqttClientPublishResult result = await _mqttClient.PublishAsync(message);
            /*
             * After making topic comprehensive and searchable,
             *  add a code to update the database
             */
            _logger.LogInformation($"Published to \"{message.Topic}\" with payload \"{payload}\" ###");
        }
        private bool ValidateTopic(string topic)
        {
            return true;
        }
        private bool ValidatePayload(string payload)
        {
            return true;
        }
    }
}